using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Kanji.Common.Extensions;
using Kanji.Database.Dao;
using Kanji.Database.Entities;
using Kanji.Database.Entities.Joins;
using System.IO;
using Kanji.Common.Helpers;
using System.Xml;
using SQLite;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Kanji.DatabaseMaker
{
    class VocabEtl : EtlBase
    {
        #region Constants

        private static readonly Regex XmlEntityRegex = new Regex("<\\!ENTITY ([^ ]+) \"([^\"]+)\">",
            RegexOptions.Multiline | RegexOptions.CultureInvariant);
        private static readonly Regex XmlValueNfRegex = new Regex("nf([0-9]+)", RegexOptions.CultureInvariant);

        private static readonly int MaxCommonNfRank = 10;

        private static readonly XNamespace XmlNs = "http://www.w3.org/XML/1998/namespace";

        private static readonly string XmlNode_Entry = "entry";
        private static readonly string XmlNode_EntSeq = "ent_seq";
        private static readonly string XmlNode_KanjiElement = "k_ele";
        private static readonly string XmlNode_KanjiReading = "keb";
        private static readonly string XmlNode_KanjiInfo = "ke_inf";
        private static readonly string XmlNode_KanjiVocabReference = "ke_pri";
        private static readonly string XmlNode_ReadingElement = "r_ele";
        private static readonly string XmlNode_KanaReading = "reb";
        private static readonly string XmlNode_ReadingVocabReference = "re_pri";
        private static readonly string XmlNode_ReadingConstraint = "re_restr";
        private static readonly string XmlNode_ReadingInfo = "re_inf";
        private static readonly string XmlNode_NoKanji = "re_nokanji";
        private static readonly string XmlNode_Meaning = "sense";
        private static readonly string XmlNode_MeaningEntry = "gloss";
        private static readonly string XmlNode_MeaningKanjiConstraint = "stagk";
        private static readonly string XmlNode_MeaningReadingConstraint = "stagr";
        private static readonly string XmlNode_MeaningCategory = "pos";
        private static readonly string XmlNode_MeaningField = "field";
        private static readonly string XmlNode_MeaningMisc = "misc";
        private static readonly string XmlNode_MeaningDialect = "dial";

        private static readonly string XmlAttribute_Language = "lang";

        private static readonly string XmlValue_News1 = "news1";
        private static readonly string XmlValue_News2 = "news2";
        private static readonly string XmlValue_Ichi1 = "ichi1";
        private static readonly string XmlValue_Ichi2 = "ichi2";
        private static readonly string XmlValue_Spec1 = "spec1";
        private static readonly string XmlValue_Spec2 = "spec2";
        private static readonly string XmlValue_Gai1 = "gai1";
        private static readonly string XmlValue_Gai2 = "gai2";

        private static readonly int BatchSize = 5000;

        #endregion

        #region Fields

        private ILogger<VocabEtl> _log;
        private Dictionary<string, string> _cultureDictionary;
        private Dictionary<string, VocabCategory> _categoryDictionary;
        private Dictionary<string, KanjiEntity> _kanjiDictionary;
        private Dictionary<string, int> _topFrequencyWords;
        private Dictionary<string, int> _waniKaniDictionary;

        private SQLiteAsyncConnection connection = DaoConnection.Instance[DaoConnectionEnum.KanjiDatabase];

        #endregion

        #region Properties

        public long VocabCount { get; private set; }
        public long VocabCategoryCount { get; private set; }
        public long VocabMeaningCount { get; private set; }
        public long VocabMeaningEntryCount { get; private set; }
        public long KanjiVocabCount { get; private set; }
        public long VocabVocabCategoryCount { get; private set; }
        public long VocabVocabMeaningCount { get; private set; }
        public long VocabMeaningVocabCategoryCount { get; private set; }

        #endregion

        #region Constructors

        public VocabEtl(ILogger<VocabEtl> logger)
            : base()
        {
            _log = logger;
            _cultureDictionary = GetXmlLangToCultureDictionary();

            // Initialize the category dictionary.
            _categoryDictionary = new Dictionary<string, VocabCategory>();

            // Build the kanji dictionary.
            _kanjiDictionary = new Dictionary<string, KanjiEntity>();
            foreach (KanjiEntity kanji in new KanjiDao().GetAllKanji().Result)
            {
                if (!_kanjiDictionary.ContainsKey(kanji.Character))
                {
                    _kanjiDictionary.Add(kanji.Character, kanji);
                }
            }

            LoadWkDictionary();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets and stores in database the vocab.
        /// </summary>
        public override async Task ExecuteAsync()
        {
            _log.LogInformation("Starting vocab ETL.");

            _log.LogInformation("Getting top frequency words...");
            GetTopFrequencyWords();
            _log.LogInformation("Read {0} top frequency words.", _topFrequencyWords.Count);

            // Read the dictionary and browse each resulting vocab.
            List<VocabEntity> vocabList = new List<VocabEntity>(BatchSize);
            foreach (VocabEntity vocab in await ReadJmDict())
            {
                if (string.IsNullOrEmpty(vocab.KanaWriting))
                {
                    // No kana writing: invalid vocab entry.
                    _log.LogInformation("Cannot parse '{0}': no kana writing.", vocab.KanjiWriting);
                }
                else
                {
                    _log.LogInformation("Parsed vocab {0} ({1}).", vocab.KanjiWriting, vocab.KanaWriting);

                    // Add the vocab to the list.
                    vocabList.Add(vocab);

                    // If the vocab list size exceeds the batch size, write all to the database.
                    if (vocabList.Count >= BatchSize)
                    {
                        await Commit(vocabList);
                        vocabList.Clear();
                    }
                }
            }

            // Flush the remaining data.
            await Commit(vocabList);

            await AttachWordFrequencyOnSingleKanaMatch();
        }

        private async Task AttachWordFrequencyOnSingleKanaMatch()
        {
            _log.LogInformation("Attaching word frequency on single kana match...");
            VocabDao dao = new VocabDao();
            foreach (string line in FileReadingHelper.ReadLineByLine(PathHelper.WordUsagePath, Encoding.UTF8))
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    string[] split = line.Trim().Split('|');
                    long? rank = ParsingHelper.ParseLong(split[0]);
                    if (split.Count() == 3 && rank.HasValue)
                    {
                        string kanjiReading = split[1];
                        string kanaReading = split[2];
                        if (kanjiReading == kanaReading)
                        {
                            if (await dao.UpdateFrequencyRankOnSingleKanaMatch(kanaReading, (int)rank.Value))
                            {
                                _log.LogInformation("{0} has a frequency of {1}", kanaReading, rank.Value);
                            }
                        }
                    }
                }
            }
        }

        private async Task Commit(List<VocabEntity> vocabList)
        {
            Dictionary<string, List<VocabEntity>> fullDictionary = new Dictionary<string, List<VocabEntity>>();
            Dictionary<string, List<VocabEntity>> kanaDictionary = new Dictionary<string, List<VocabEntity>>();
            foreach (VocabEntity entity in vocabList)
            {
                string fullString = entity.KanjiWriting + "|" + entity.KanaWriting;
                if (fullDictionary.ContainsKey(fullString))
                {
                    fullDictionary[fullString].Add(entity);
                }
                else
                {
                    fullDictionary.Add(fullString, new List<VocabEntity>() { entity });
                }

                if (kanaDictionary.ContainsKey(entity.KanaWriting))
                {
                    kanaDictionary[entity.KanaWriting].Add(entity);
                }
                else
                {
                    kanaDictionary.Add(entity.KanaWriting, new List<VocabEntity>() { entity });
                }
            }

            AttachFurigana(vocabList, fullDictionary);
            AttachJlptLevel(vocabList, fullDictionary, kanaDictionary);
            AttachWordFrequency(vocabList, fullDictionary);
            AttachWkLevel(vocabList);
            AttachWikipediaRank(vocabList);
            await InsertData(vocabList);
        }

        private void AttachWordFrequency(List<VocabEntity> vocabList, Dictionary<string, List<VocabEntity>> fullDictionary)
        {
            DateTimeOffset before = DateTimeOffset.Now;
            foreach (string line in FileReadingHelper.ReadLineByLine(PathHelper.WordUsagePath, Encoding.UTF8))
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    string[] split = line.Trim().Split('|');
                    long? rank = ParsingHelper.ParseLong(split[0]);
                    if (split.Count() == 3 && rank.HasValue)
                    {
                        string kanjiReading = split[1];
                        string kanaReading = split[2];
                        string vocabString = kanjiReading + "|" + kanaReading;
                        if (fullDictionary.ContainsKey(vocabString))
                        {
                            foreach (VocabEntity match in fullDictionary[vocabString])
                            {
                                match.FrequencyRank = (int)rank.Value;
                            }
                        }
                    }
                }
            }
            TimeSpan duration = DateTimeOffset.Now - before;
            _log.LogInformation("Attaching word frequency took {0}ms.", (long)duration.TotalMilliseconds);
        }

        private void AttachFurigana(List<VocabEntity> vocabList, Dictionary<string, List<VocabEntity>> fullDictionary)
        {
            DateTimeOffset before = DateTimeOffset.Now;

            using (StreamReader reader = new StreamReader(PathHelper.JmDictFuriganaPath))
            {
                string line = string.Empty;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    string[] split = line.Split('|');
                    string vocabString = split[0] + "|" + split[1];
                    if (fullDictionary.ContainsKey(vocabString))
                    {
                        foreach (VocabEntity match in fullDictionary[vocabString])
                        {
                            match.Furigana = split[2];
                        }
                    }
                }
            }
            TimeSpan duration = DateTimeOffset.Now - before;
            _log.LogInformation("Attaching furigana took {0}ms.", (long)duration.TotalMilliseconds);
        }

        private void AttachJlptLevel(List<VocabEntity> vocabList, Dictionary<string, List<VocabEntity>> fullDictionary,
            Dictionary<string, List<VocabEntity>> kanaDictionary)
        {
            DateTimeOffset before = DateTimeOffset.Now;

            using (StreamReader reader = new StreamReader(PathHelper.JlptVocabListPath))
            {
                string line = string.Empty;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    string[] split = line.Split('|');

                    if (split.Length < 1)
                    {
                        continue;
                    }

                    short? level = ParsingHelper.ParseShort(split[0]);
                    if (!level.HasValue)
                    {
                        continue;
                    }

                    if (split.Length == 2)
                    {
                        // Level | kana
                        if (kanaDictionary.ContainsKey(split[1]))
                        {
                            foreach (VocabEntity match in kanaDictionary[split[1]])
                            {
                                match.JlptLevel = level.Value;
                            }
                        }
                    }
                    else if (split.Length == 3)
                    {
                        // Level | kanji | kana
                        string vocabString = split[1] + "|" + split[2];
                        if (fullDictionary.ContainsKey(vocabString))
                        {
                            foreach (VocabEntity match in fullDictionary[vocabString])
                            {
                                match.JlptLevel = level.Value;
                            }
                        }
                    }
                }
            }
            TimeSpan duration = DateTimeOffset.Now - before;
            _log.LogInformation("Attaching JLPT level took {0}ms.", (long)duration.TotalMilliseconds);
        }

        private void AttachWkLevel(List<VocabEntity> vocabList)
        {
            foreach (VocabEntity v in vocabList)
            {
                string vocabString = v.KanjiWriting + "|" + v.KanaWriting;
                if (_waniKaniDictionary.ContainsKey(vocabString))
                {
                    v.WaniKaniLevel = _waniKaniDictionary[vocabString];
                }
            }
        }

        private void AttachWikipediaRank(List<VocabEntity> vocabList)
        {
            foreach (VocabEntity v in vocabList)
            {
                // Find the word in the frequency list.
                // Only for main vocab.
                if (v.IsMain && !string.IsNullOrWhiteSpace(v.KanjiWriting) && _topFrequencyWords.ContainsKey(v.KanjiWriting))
                {
                    v.WikipediaRank = _topFrequencyWords[v.KanjiWriting];
                    v.IsCommon = true;
                }
            }
        }

        /// <summary>
        /// Reads the most used words and puts the result in the decicated field.
        /// </summary>
        private void GetTopFrequencyWords()
        {
            // For now, info is taken from http://en.wiktionary.org/wiki/Wiktionary:Frequency_lists/Japanese
            string[] words = File.ReadAllLines(PathHelper.TopVocabularyFrequencyPath);
            _topFrequencyWords = new Dictionary<string, int>();
            for (int i = 0; i < words.Count(); i++)
            {
                if (!_topFrequencyWords.ContainsKey(words[i]))
                {
                    _topFrequencyWords.Add(words[i], _topFrequencyWords.Count + 1);
                }
            }
        }

        /// <summary>
        /// Reads the WaniKani vocab list file and builds the dictionary that will be used
        /// during the execution of the ETL.
        /// </summary>
        private void LoadWkDictionary()
        {
            _waniKaniDictionary = new Dictionary<string, int>();
            foreach (string line in File.ReadLines(PathHelper.WaniKaniVocabListPath))
            {
                string[] split = line.Split('|');
                if (split.Count() != 3)
                {
                    continue;
                }

                int? level = ParsingHelper.ParseInt(split[2]);
                string vocabString = split[0] + "|" + split[1];
                if (!_waniKaniDictionary.ContainsKey(vocabString) && level.HasValue)
                {
                    _waniKaniDictionary.Add(vocabString, level.Value);
                }
            }
        }

        /// <summary>
        /// Writes the given list of vocab to the database.
        /// </summary>
        /// <param name="vocabList">Vocab to write to the database.</param>
        private async Task InsertData(List<VocabEntity> vocabList)
        {
            if (vocabList.Any())
            {
                _log.LogInformation("Inserting the entities for the {0} last vocab", vocabList.Count);

                // Insert vocab itself.
                await connection.InsertAllAsync(vocabList);
                _log.LogInformation("Inserted {0} vocab entities", vocabList.Count);
                VocabCount += vocabList.Count;

                // Insert meanings.
                VocabMeaning[] newMeanings = vocabList.SelectMany(v => v.Meanings)
                    .Distinct()
                    .Where(vm => vm.ID <= 0)
                    .ToArray();
                await connection.InsertAllAsync(newMeanings);
                _log.LogInformation("Inserted {0} vocab meaning entities", newMeanings.Length);
                VocabMeaningCount += newMeanings.Length;

                // Insert kanji-vocab join entities.
                int kanjiVocabCount = await connection.InsertAllAsync(vocabList.SelectMany(v => v.Kanji.Select(k =>
                    new KanjiVocabJoinEntity()
                    {
                        KanjiId = k.ID,
                        VocabId = v.ID
                    }
                )));
                _log.LogInformation("Inserted {0} kanji-vocab join entities", kanjiVocabCount);
                KanjiVocabCount += kanjiVocabCount;

                // Insert Vocab-VocabCategory join entities.
                int vocabVocabCategoryCount = await connection.InsertAllAsync(vocabList.SelectMany(v => 
                    v.Categories.Distinct().Select(c =>
                    new VocabCategoryVocabJoinEntity()
                    {
                        CategoryId = c.ID,
                        VocabId = v.ID
                    }
                )));
                _log.LogInformation("Inserted {0} Vocab-VocabCategory join entities", vocabVocabCategoryCount);
                VocabVocabCategoryCount += vocabVocabCategoryCount;

                // Insert Vocab-VocabMeaning join entities.
                int vocabVocabMeaningCount = await connection.InsertAllAsync(vocabList.SelectMany(v => v.Meanings.Select(m => 
                    new VocabVocabMeaningJoinEntity()
                    {
                        MeaningId = m.ID,
                        VocabId = v.ID
                    }
                )));
                _log.LogInformation("Inserted {0} Vocab-VocabMeaning join entities", vocabVocabMeaningCount);
                VocabVocabMeaningCount += vocabVocabMeaningCount;

                // Insert VocabMeaning-VocabCategory join entities.
                int vocabMeaningVocabCategoryCount = await connection.InsertAllAsync(newMeanings.SelectMany(m => m.Categories.Select(c =>
                    new VocabMeaningVocabCategoryJoinEntity()
                    {
                        MeaningId = m.ID,
                        CategoryId = c.ID
                    }
                )));
                _log.LogInformation("Inserted {0} VocabMeaning-VocabCategory join entities", vocabMeaningVocabCategoryCount);
                VocabMeaningVocabCategoryCount += vocabMeaningVocabCategoryCount;
            }
        }

        /// <summary>
        /// Reads the JMdict file.
        /// </summary>
        private async Task<IEnumerable<VocabEntity>> ReadJmDict()
        {
            // Load the file.
            var settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Parse;
            settings.MaxCharactersFromEntities = 0;
            XDocument xdoc = XDocument.Load(XmlReader.Create(PathHelper.JmDictPath, settings));

            // Load vocab categories.
            _log.LogInformation("Loading vocab categories");
            foreach (VocabCategory category in LoadVocabCategories(xdoc))
            {
                // Store vocab categories in the database.
                await connection.InsertAsync(category);
                VocabCategoryCount++;

                // Add them to the dictionary too.
                _categoryDictionary.Add(category.Label, category);
            }
            _log.LogInformation("Loaded {0} vocab categories", VocabCategoryCount);

            // Load and return vocab items.
            _log.LogInformation("Loading vocab");
            return LoadVocabItems(xdoc);
        }

        /// <summary>
        /// Loads and returns vocab categories from the JMDict XDocument.
        /// </summary>
        /// <param name="xdoc">Loaded JMDict XDocument.</param>
        /// <returns>Vocab categories loaded from the file.</returns>
        private IEnumerable<VocabCategory> LoadVocabCategories(XDocument xdoc)
        {
            // Get the DTD declarations as a string.
            string subset = xdoc.DocumentType.InternalSubset;

            // Use the entity regex to grab every entity term and value.
            foreach (Match match in XmlEntityRegex.Matches(subset))
            {
                string key = match.Groups[1].Value;
                string value = match.Groups[2].Value;

                yield return new VocabCategory() { ShortName = key, Label = value };
            }
        }

        /// <summary>
        /// Loads and returns vocab items from the JMDict XDocument.
        /// </summary>
        /// <param name="xdoc">Loaded JMDict XDocument.</param>
        /// <returns>Vocab loaded from the file.</returns>
        private IEnumerable<VocabEntity> LoadVocabItems(XDocument xdoc)
        {
            // Browse each vocab entry.
            foreach (XElement xentry in xdoc.Root.Elements(XmlNode_Entry))
            {
                List<VocabEntity> vocabList = new List<VocabEntity>();

                long seq = long.Parse(xentry.Element(XmlNode_EntSeq).Value);
                // For each kanji element node
                foreach (XElement xkanjiElement in xentry.Elements(XmlNode_KanjiElement))
                {
                    // Parse the kanji element. The list will be expanded with new elements.
                    ParseKanji(xkanjiElement, vocabList, seq);
                }

                // For each kanji reading node
                var xreadingElements = xentry.Elements(XmlNode_ReadingElement);
                foreach (XElement xreadingElement in xreadingElements)
                {
                    // Exclude the node if it contains the no kanji node, and is not the only reading.
                    // This is a behavior that seems to be implemented in Jisho (example word: 台詞).
                    if (xreadingElement.HasElement(XmlNode_NoKanji) && xreadingElements.Count() > 1)
                    {
                        continue;
                    }

                    // Parse the reading. The list will be expanded and/or its elements filled with
                    // the available info.
                    ParseReading(xreadingElement, vocabList, seq);
                }

                // For each kanji meaning node
                foreach (XElement xmeaningElement in xentry.Elements(XmlNode_Meaning))
                {
                    // Parse the meaning node. The list will be updated with the available info.
                    ParseMeaning(xmeaningElement, vocabList);
                }

                // Now we will define the vocab <-> kanji relationships.
                // For each vocab
                bool first = true;
                foreach (VocabEntity vocab in vocabList)
                {
                    // If the kanji vocab is defined
                    if (!string.IsNullOrEmpty(vocab.KanjiWriting))
                    {
                        // Browse each character
                        foreach (char c in vocab.KanjiWriting)
                        {
                            // Attempt to find a kanji matching the character.
                            string s = c.ToString();
                            KanjiEntity kanji = _kanjiDictionary.ContainsKey(s) ? _kanjiDictionary[s] : null;
                            if (kanji != null)
                            {
                                // If a kanji is found, create a relationship between the entities.
                                vocab.Kanji.Add(kanji);
                            }
                        }
                    }

                    if (first)
                    {
                        // Set the first vocab as the main variant.
                        first = false;
                        vocab.IsMain = true;
                    }
                    else
                    {
                        // For now, we are saying that non-main vocab cannot be common.
                        // Otherwise, too much variants are marked as common and it can
                        // be confusing.
                        vocab.IsCommon = false;
                    }

                    // Return the vocab and continue to the next one.
                    yield return vocab;
                }
            }
        }

        /// <summary>
        /// Parses a kanji element node.
        /// Updates the list with the available info.
        /// </summary>
        /// <param name="xkanjiElement">Element to parse.</param>
        /// <param name="vocabList">Vocab list to be updated.</param>
        /// <param name="seq">Sequence number of corresponding vocab item</param>
        private void ParseKanji(XElement xkanjiElement, List<VocabEntity> vocabList, long seq)
        {
            // Create a new vocab with the associated writing.
            VocabEntity vocab = new VocabEntity();
            vocab.Seq = seq;
            vocab.KanjiWriting = xkanjiElement.Element(XmlNode_KanjiReading).Value;
            vocab.IsCommon = IsCommonWord(xkanjiElement, XmlNode_KanjiVocabReference);
            
            // For each kanji info node
            foreach (XElement xkanjiInf in xkanjiElement.Elements(XmlNode_KanjiInfo))
            {
                // Associate the vocab with the category referred by the info.
                VocabCategory category = GetCategoryByLabel(xkanjiInf.Value);
                if (category != null && !vocab.Categories.Contains(category))
                {
                    vocab.Categories.Add(category);
                }
            }

            // Add the created vocab to the list.
            vocabList.Add(vocab);
        }

        /// <summary>
        /// Parses a reading element node.
        /// Updates the list with the available info.
        /// </summary>
        /// <param name="xreadingElement">Element to parse.</param>
        /// <param name="vocabList">Vocab list to be updated.</param>
        private void ParseReading(XElement xreadingElement, List<VocabEntity> vocabList, long seq)
        {
            // First, we have to determine the target of the reading node.
            // Two possible cases:
            // - Scenario 1: There were no kanji readings. In that case, the reading should
            //   add a new vocab element which has no kanji reading.
            // - Scenario 2: There was at least one kanji reading. In that case, the reading
            //   node targets a set of existing vocabs. They may be filtered by kanji reading
            //   with the reading constraint nodes.

            VocabEntity[] targets;
            if (!vocabList.Any())
            {
                // Scenario 1. Create a new kanji reading, add it to the list, and set it as target.
                VocabEntity newVocab = new VocabEntity();
                newVocab.Seq = seq;
                vocabList.Add(newVocab);
                targets = new VocabEntity[] { newVocab };
            }
            else
            {
                // Scenario 2. Check constraint nodes to filter the targets.

                // Get all reading constraints in an array.
                string[] readingConstraints = xreadingElement.Elements(XmlNode_ReadingConstraint)
                    .Select(x => x.Value).ToArray();

                // Filter from the vocab list.
                if (readingConstraints.Any())
                {
                    targets = vocabList.Where(v => readingConstraints.Contains(v.KanjiWriting)).ToArray();
                }
                else
                {
                    targets = vocabList.ToArray();
                }
            }

            // Now that we have the target vocabs, we can get the proper information from the node.
            string kanaReading = xreadingElement.Element(XmlNode_KanaReading).Value;
            bool isCommon = IsCommonWord(xreadingElement, XmlNode_ReadingVocabReference);

            // Get the optional categories defined by the "reading info" nodes.
            VocabCategory[] categories = xreadingElement.Elements(XmlNode_ReadingInfo)
                .Select(x => GetCategoryByLabel(x.Value))
                .Where(c => c != null).ToArray();

            // We have the info. Now we can apply it to the targets.
            // For each target
            foreach (VocabEntity target in targets)
            {
                // Set the kana reading if not already set.
                if (string.IsNullOrEmpty(target.KanaWriting))
                {
                    target.KanaWriting = kanaReading;

                    // Set the common flag to false only if both the kanji and the reading values are false.
                    target.IsCommon = target.IsCommon || isCommon;

                    // Append the categories to the existing collection.
                    target.Categories = target.Categories.Concat(categories).ToArray();
                }
                else if (!vocabList.Where(v => v.KanaWriting == kanaReading).Any() && target == targets.Last())
                {
                    // If a target already has a kana reading, we need to create a new vocab.
                    VocabEntity newVocab = new VocabEntity()
                    {
                        Seq = target.Seq,
                        KanjiWriting = target.KanjiWriting, // Assign the old kanji reading,
                        IsCommon = target.IsCommon || isCommon, // combined common flag,
                        FrequencyRank = target.FrequencyRank, // same frequency rank,
                        KanaWriting = kanaReading, // new kana reading,
                        Categories = target.Categories.Concat(categories).ToArray() // combined categories.
                    };
                    vocabList.Add(newVocab);
                }
            }
        }

        /// <summary>
        /// Parses a meaning element node.
        /// Updates the list with the available info.
        /// </summary>
        /// <param name="xmeaningElement">Element to parse.</param>
        /// <param name="vocabList">Vocab list to be updated.</param>
        private void ParseMeaning(XElement xmeaningElement, List<VocabEntity> vocabList)
        {
            // First, like in the reading nodes, we have to determine the vocabs the node targets.
            // There are two optional constraint nodes in a meaning node.

            VocabEntity[] targets;
            // Get all kanji reading constraints in an array.
            string[] kanjiConstraints = xmeaningElement.Elements(XmlNode_MeaningKanjiConstraint)
                .Select(x => x.Value).ToArray();
            // Get all kana reading constraints in an array.
            string[] kanaConstraints = xmeaningElement.Elements(XmlNode_MeaningReadingConstraint)
                .Select(x => x.Value).ToArray();

            // Filter from the vocab list.
            if (kanjiConstraints.Any() || kanaConstraints.Any())
            {
                targets = vocabList.Where(v => kanjiConstraints.Contains(v.KanjiWriting)
                    || kanaConstraints.Contains(v.KanaWriting)).ToArray();
            }
            else
            {
                targets = vocabList.ToArray();
            }

            // Targets acquired. Next, we will read the info of the node.

            // Get the categories by selecting on some nodes the DB categories matching the node values.
            List<VocabCategory> categories = new List<VocabCategory>();
            categories.AddRange(xmeaningElement.Elements(XmlNode_MeaningCategory)
                .Select(x => GetCategoryByLabel(x.Value))
                .Where(c => c != null));
            categories.AddRange(xmeaningElement.Elements(XmlNode_MeaningDialect)
                .Select(x => GetCategoryByLabel(x.Value))
                .Where(c => c != null));
            categories.AddRange(xmeaningElement.Elements(XmlNode_MeaningField)
                .Select(x => GetCategoryByLabel(x.Value))
                .Where(c => c != null));
            categories.AddRange(xmeaningElement.Elements(XmlNode_MeaningMisc)
                .Select(x => GetCategoryByLabel(x.Value))
                .Where(c => c != null));

            // Then, get the meanings.
            VocabMeaning meaning = new VocabMeaning() { Categories = categories };
            // For each meaning entry node
            foreach (XElement xmeaningEntry in xmeaningElement.Elements(XmlNode_MeaningEntry))
            {
                // Get the language and value.
                string language = null;
                if (xmeaningEntry.Attribute(XmlNs + XmlAttribute_Language) != null)
                {
                    language = _cultureDictionary[
                        xmeaningEntry.Attribute(XmlNs + XmlAttribute_Language).Value];

                    if (language.ToLower() == "en")
                    {
                        language = null;
                    }
                }
                string value = xmeaningEntry.Value;

                // Build a meaning entry and add it to the meaning.
                // Only take english meanings (for now?).
                if (language == null)
                {
                    meaning.Meaning += value + " ; ";
                }
            }
            meaning.Meaning = meaning.Meaning.TrimEnd(new char[] { ';', ' ' });

            // Value the targets.
            foreach (VocabEntity target in targets)
            {
                target.Meanings.Add(meaning);
            }
        }

        /// <summary>
        /// Determines if the word referred by the given reading or kanji element is considered a common word.
        /// </summary>
        /// <param name="xelement">Reading or kanji element representing the vocab.</param>
        /// <param name="refNodeName">Name of the nodes containing reference elements needed to determine
        /// if the vocab is a common word.</param>
        /// <returns>True if the word is common. False otherwise.</returns>
        private bool IsCommonWord(XElement xelement, string refNodeName)
        {
            // Acquire all references.
            string[] refs = xelement.Elements(refNodeName).Select(x => x.Value).ToArray();

            // Test for some references indicating a common word.
            if (refs.Intersect(new string[] { XmlValue_Gai1, XmlValue_Gai2, XmlValue_Ichi1,
                XmlValue_News1, XmlValue_Spec1, XmlValue_Spec2})
                .Any())
            {
                return true;
            }
            else
            {
                // If no common references are found, test the "nf" indicator.
                // To do that, use the nf regex to find a match in the references.
                Match nfMatch = refs.Select(r => XmlValueNfRegex.Match(r)).Where(match => match.Success).FirstOrDefault();
                if (nfMatch != null)
                {
                    // If a match is found, parse the numeric part of the nf reference.
                    int nf = int.Parse(nfMatch.Groups[1].Value);

                    // If the numeric part is below the common threshold, return true.
                    if (nf < MaxCommonNfRank)
                    {
                        return true;
                    }
                }
            }

            // Nothing indicates that the word is common. Return false.
            return false;
        }

        /// <summary>
        /// Gets the category with the given label (or null if not found).
        /// </summary>
        /// <param name="label">Label of the category to retrieve.</param>
        /// <returns>Category with the given label (or null if not found).</returns>
        private VocabCategory GetCategoryByLabel(string label)
        {
            return _categoryDictionary.ContainsKey(label) ? _categoryDictionary[label] : null;
        }

        /// <summary>
        /// Builds and returns a dictionary that matches ISO 639-2/B language codes
        /// (used in the XML file) to ISO 639-1 language codes (used in .net).
        /// </summary>
        /// <remarks>I love how standards for language codes are subdivided.</remarks>
        private Dictionary<string, string> GetXmlLangToCultureDictionary()
        {
            Dictionary<string, string> cultureDictionary = new Dictionary<string, string>();

            // For each culture...
            foreach (CultureInfo culture in CultureInfo.GetCultures(
                CultureTypes.SpecificCultures))
            {
                // Get the ISO 639-2/A language code.
                string threeLetter = culture.ThreeLetterISOLanguageName;

                // Convert it to ISO 639-2/B.
                if (threeLetter == "bod") threeLetter = "tib";
                else if (threeLetter == "ces") threeLetter = "cze";
                else if (threeLetter == "cym") threeLetter = "wel";
                else if (threeLetter == "deu") threeLetter = "ger";
                else if (threeLetter == "ell") threeLetter = "gre";
                else if (threeLetter == "eus") threeLetter = "baq";
                else if (threeLetter == "fas") threeLetter = "per";
                else if (threeLetter == "fra") threeLetter = "fre";
                else if (threeLetter == "hrv") threeLetter = "scr";
                else if (threeLetter == "hye") threeLetter = "arm";
                else if (threeLetter == "isl") threeLetter = "ice";
                else if (threeLetter == "jaw") threeLetter = "jav";
                else if (threeLetter == "kat") threeLetter = "geo";
                else if (threeLetter == "mkd") threeLetter = "mac";
                else if (threeLetter == "mri") threeLetter = "mao";
                else if (threeLetter == "msa") threeLetter = "may";
                else if (threeLetter == "mya") threeLetter = "bur";
                else if (threeLetter == "ron") threeLetter = "rum";
                else if (threeLetter == "slk") threeLetter = "slo";
                else if (threeLetter == "sqi") threeLetter = "alb";
                else if (threeLetter == "srp") threeLetter = "scc";
                else if (threeLetter == "nld") threeLetter = "dut";
                else if (threeLetter == "zho") threeLetter = "chi";

                // Get the ISO 639-1 language code.
                string twoLetter = culture.TwoLetterISOLanguageName;

                // Write the matching entry to the dictionary.
                if (!cultureDictionary.ContainsKey(threeLetter))
                {
                    cultureDictionary.Add(threeLetter, twoLetter);
                }
            }

            return cultureDictionary;
        }

        #endregion
    }
}
