﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Kanji.Common.Helpers;
using Kanji.Common.Extensions;
using Kanji.Database.Entities;
using Kanji.Database.Entities.Joins;
using Kanji.Database.Helpers;
using System.IO;
using System.IO.Compression;
using SQLite;
using Kanji.Database.Dao;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Kanji.DatabaseMaker
{
    class KanjiEtl : EtlBase
    {
        #region Constants

        private static readonly string XmlNode_Character = "character";
        private static readonly string XmlNode_Literal = "literal";
        private static readonly string XmlNode_Misc = "misc";
        private static readonly string XmlNode_Grade = "grade";
        private static readonly string XmlNode_StrokeCount = "stroke_count";
        private static readonly string XmlNode_Frequency = "freq";
        private static readonly string XmlNode_JlptLevel = "jlpt";
        private static readonly string XmlNode_ReadingMeaning = "reading_meaning";
        private static readonly string XmlNode_Nanori = "nanori";
        private static readonly string XmlNode_ReadingMeaningGroup = "rmgroup";
        private static readonly string XmlNode_Reading = "reading";
        private static readonly string XmlNode_Meaning = "meaning";
        private static readonly string XmlNode_CodePoint = "codepoint";
        private static readonly string XmlNode_CodePointValue = "cp_value";

        private static readonly string XmlAttribute_ReadingType = "r_type";
        private static readonly string XmlAttribute_MeaningLanguage = "m_lang";
        private static readonly string XmlAttribute_CodePointType = "cp_type";

        private static readonly string XmlAttributeValue_KunYomiReading = "ja_kun";
        private static readonly string XmlAttributeValue_OnYomiReading = "ja_on";
        private static readonly string XmlAttributeValue_CodePointUnicode = "ucs";

        #endregion

        #region Fields

        private RadicalDictionary _radicalDictionary;
        private Dictionary<string, short> _jlptDictionary;
        private Dictionary<string, int> _frequencyRankDictionary;
        private Dictionary<string, int> _waniKaniDictionary;
        private ILogger<KanjiEtl> _log;

        private ZipArchive _svgZipArchive;

        private SQLiteAsyncConnection connection = DaoConnection.Instance[DaoConnectionEnum.KanjiDatabase];

        #endregion

        #region Properties

        /// <summary>
        /// Gets the number of kanji added to the database.
        /// </summary>
        public long KanjiCount { get; private set; }

        /// <summary>
        /// Gets the number of kanji meanings added to the database.
        /// </summary>
        public long KanjiMeaningCount { get; private set; }

        /// <summary>
        /// Gets the number of kanji-radical join entities added to the database.
        /// </summary>
        public long KanjiRadicalCount { get; private set; }

        #endregion

        #region Constructors

        public KanjiEtl(RadicalDictionary radicalDictionary, ILogger<KanjiEtl> logger)
            : base()
        {
            _radicalDictionary = radicalDictionary;
            _log = logger;
            CreateJlptDictionary();
            CreateFrequencyRankDictionary();
            CreateWkDictionary();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Reads the JLPT kanji list file and fills a dictionary that will be used
        /// to look up the info during the execution of the ETL.
        /// </summary>
        private void CreateJlptDictionary()
        {
            _jlptDictionary = new Dictionary<string, short>();
            foreach (string line in File.ReadLines(PathHelper.JlptKanjiListPath))
            {
                string[] split = line.Split('|');
                if (split.Count() != 2)
                {
                    continue;
                }

                if (!_jlptDictionary.ContainsKey(split[1]))
                {
                    short? value = ParsingHelper.ParseShort(split[0]);
                    if (value.HasValue)
                    {
                        _jlptDictionary.Add(split[1], value.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Reads the kanji frequency file and fills a dictionary that will be used to look up the info
        /// during the execution of the ETL.
        /// </summary>
        private void CreateFrequencyRankDictionary()
        {
            _frequencyRankDictionary = new Dictionary<string, int>();
            int i = 1;
            foreach (string line in File.ReadLines(PathHelper.KanjiUsagePath))
            {
                string[] split = line.Split('|');
                if (split.Count() != 2)
                {
                    continue;
                }

                if (!_frequencyRankDictionary.ContainsKey(split[0]))
                {
                    _frequencyRankDictionary.Add(split[0], i++);
                }
            }
        }

        /// <summary>
        /// Reads the WaniKani kanji list file and fills a dictionary that will be used to look up the info
        /// during the execution of the ETL.
        /// </summary>
        private void CreateWkDictionary()
        {
            _waniKaniDictionary = new Dictionary<string, int>();
            foreach (string line in File.ReadLines(PathHelper.WaniKaniKanjiListPath))
            {
                string[] split = line.Split('|');
                if (split.Count() != 2)
                {
                    continue;
                }

                int? level = ParsingHelper.ParseInt(split[1]);
                if (!_waniKaniDictionary.ContainsKey(split[0]) && level.HasValue)
                {
                    _waniKaniDictionary.Add(split[0], level.Value);
                }
            }
        }

        /// <summary>
        /// Reads kanji and stores them in the database.
        /// </summary>
        public override async Task ExecuteAsync()
        {
            List<KanjiRadicalJoinEntity> kanjiRadicalList = new List<KanjiRadicalJoinEntity>();
            List<KanjiMeaning> kanjiMeaningList = new List<KanjiMeaning>();
            List<KanjiStrokes> kanjiStrokes = new List<KanjiStrokes>();

            // Parse the file.
            foreach (KanjiEntity kanji in ReadKanjiDic2())
            {
                // For each kanji read:
                string addedRadicalsString = string.Empty; // Log

                // Try to find the matching composition.
                if (_radicalDictionary.ContainsKey(kanji.Character))
                {
                    RadicalValue[] matchingRadicals = _radicalDictionary[kanji.Character];
                    // If the composition is found:
                    foreach (RadicalValue radicalValue in matchingRadicals)
                    {
                        // Retrieve each radical from the database and add it in the kanji.
                        kanji.Radicals.Add(radicalValue.Radical);
                        addedRadicalsString += radicalValue.Character + " "; // Log
                    }
                }

                // Search for a matching SVG.
                kanjiStrokes.Add(RetrieveSvg(kanji));

                // Add the finalized kanji to the database.
                await connection.InsertAsync(kanji);

                // Add the kanji meaning entities.
                kanjiMeaningList.AddRange(kanji.Meanings);

                // Add the kanji-radical join entities.
                foreach (RadicalEntity radical in kanji.Radicals)
                {
                    kanjiRadicalList.Add(new KanjiRadicalJoinEntity()
                        {
                            KanjiId = kanji.ID,
                            RadicalId = radical.ID
                        });
                }

                // Increment counter
                KanjiCount++;

                // Log
                _log.LogInformation("Inserted kanji {char}  ({id}) with radicals {radicals}", kanji.Character, kanji.ID, addedRadicalsString);
            }
            CloseZipArchive();

            // Insert the strokes.
            await connection.InsertAllAsync(kanjiStrokes);

            // Insert the kanji meaning entities.
            KanjiMeaningCount = kanjiMeaningList.Count;
            _log.LogInformation("Inserting {count} kanji meaning entities", KanjiMeaningCount);
            await connection.InsertAllAsync(kanjiMeaningList);

            // Insert the kanji-radical join entities
            KanjiRadicalCount = kanjiRadicalList.Count;
            _log.LogInformation("Inserting {count} kanji-radical join entities", KanjiRadicalCount);
            await connection.InsertAllAsync(kanjiRadicalList);
        }

        /// <summary>
        /// Reads the KanjiDic2 file and outputs kanji entities parsed from the file.
        /// </summary>
        /// <returns>Kanji entities parsed from the file.</returns>
        private IEnumerable<KanjiEntity> ReadKanjiDic2()
        {
            // Load the KanjiDic2 file.
            XDocument xdoc = XDocument.Load(PathHelper.KanjiDic2Path);

            // Browse kanji nodes.
            foreach (XElement xkanji in xdoc.Root.Elements(XmlNode_Character))
            {
                // For each kanji node, read values.
                KanjiEntity kanji = new KanjiEntity();

                // Read the kanji character.
                kanji.Character = xkanji.Element(XmlNode_Literal).Value;

                // In the code point node...
                XElement xcodePoint = xkanji.Element(XmlNode_CodePoint);
                if (xcodePoint != null)
                {
                    // Try to read the unicode character value.
                    XElement xunicode = xcodePoint.Elements(XmlNode_CodePointValue)
                        .Where(x => x.ReadAttributeString(XmlAttribute_CodePointType) == XmlAttributeValue_CodePointUnicode)
                        .FirstOrDefault();

                    if (xunicode != null)
                    {
                        string unicodeValueString = xunicode.Value;
                        int intValue = 0;
                        if (int.TryParse(unicodeValueString, System.Globalization.NumberStyles.HexNumber, ParsingHelper.DefaultCulture, out intValue))
                        {
                            kanji.UnicodeValue = intValue;
                        }
                    }
                }

                // In the misc node...
                XElement xmisc = xkanji.Element(XmlNode_Misc);
                if (xmisc != null)
                {
                    // Try to read the grade, stroke count, frequency and JLPT level.
                    // Update: JLPT level is outdated in this file. Now using the JLPTKanjiList.
                    XElement xgrade = xmisc.Element(XmlNode_Grade);
                    XElement xstrokeCount = xmisc.Element(XmlNode_StrokeCount);
                    XElement xfrequency = xmisc.Element(XmlNode_Frequency);
                    //XElement xjlpt = xmisc.Element(XmlNode_JlptLevel);

                    if (xgrade != null) kanji.Grade = ParsingHelper.ParseShort(xgrade.Value);
                    if (xstrokeCount != null) kanji.StrokeCount = ParsingHelper.ParseShort(xstrokeCount.Value);
                    if (xfrequency != null) kanji.NewspaperRank = ParsingHelper.ParseInt(xfrequency.Value);
                    //if (xjlpt != null) kanji.JlptLevel = ParsingHelper.ParseShort(xjlpt.Value);
                }

                // Find the JLPT level using the dictionary.
                if (_jlptDictionary.ContainsKey(kanji.Character))
                {
                    kanji.JlptLevel = _jlptDictionary[kanji.Character];
                }

                // Find the frequency rank using the dictionary.
                if (_frequencyRankDictionary.ContainsKey(kanji.Character))
                {
                    kanji.MostUsedRank = _frequencyRankDictionary[kanji.Character];
                }

                // Find the WaniKani level using the dictionary.
                if (_waniKaniDictionary.ContainsKey(kanji.Character))
                {
                    kanji.WaniKaniLevel = _waniKaniDictionary[kanji.Character];
                }

                // In the reading/meaning node...
                XElement xreadingMeaning = xkanji.Element(XmlNode_ReadingMeaning);
                if (xreadingMeaning != null)
                {
                    // Read the nanori readings.
                    kanji.Nanori = string.Empty;
                    foreach (XElement xnanori in xreadingMeaning.Elements(XmlNode_Nanori))
                    {
                        kanji.Nanori += xnanori.Value + MultiValueFieldHelper.ValueSeparator;
                    }
                    kanji.Nanori = kanji.Nanori.Trim(MultiValueFieldHelper.ValueSeparator);

                    // Browse the reading group...
                    XElement xrmGroup = xreadingMeaning.Element(XmlNode_ReadingMeaningGroup);
                    if (xrmGroup != null)
                    {
                        // Read the on'yomi readings.
                        kanji.OnYomi = string.Empty;
                        foreach (XElement xonYomi in xrmGroup.Elements(XmlNode_Reading)
                            .Where(x => x.Attribute(XmlAttribute_ReadingType).Value == XmlAttributeValue_OnYomiReading))
                        {
                            kanji.OnYomi += xonYomi.Value + MultiValueFieldHelper.ValueSeparator;
                        }
                        kanji.OnYomi = KanaHelper.ToHiragana(kanji.OnYomi.Trim(MultiValueFieldHelper.ValueSeparator));

                        // Read the kun'yomi readings.
                        kanji.KunYomi = string.Empty;
                        foreach (XElement xkunYomi in xrmGroup.Elements(XmlNode_Reading)
                            .Where(x => x.Attribute(XmlAttribute_ReadingType).Value == XmlAttributeValue_KunYomiReading))
                        {
                            kanji.KunYomi += xkunYomi.Value + MultiValueFieldHelper.ValueSeparator;
                        }
                        kanji.KunYomi = kanji.KunYomi.Trim(MultiValueFieldHelper.ValueSeparator);

                        // Browse the meanings...
                        foreach (XElement xmeaning in xrmGroup.Elements(XmlNode_Meaning))
                        {
                            // Get the language and meaning.
                            XAttribute xlanguage = xmeaning.Attribute(XmlAttribute_MeaningLanguage);
                            string language = xlanguage != null ? xlanguage.Value.ToLower() : null;
                            string meaning = xmeaning.Value;

                            if (xlanguage == null || language.ToLower() == "en")
                            {
                                // Build a meaning.
                                KanjiMeaning kanjiMeaning = new KanjiMeaning() { Kanji = kanji, Language = language, Meaning = meaning };

                                // Add the meaning to the kanji.
                                kanji.Meanings.Add(kanjiMeaning);
                            }
                        }
                    }
                }

                // Return the kanji read and go to the next kanji node.
                yield return kanji;

                xkanji.RemoveAll();
            }
        }

        /// <summary>
        /// Attempts to retrieve the kanji strokes SVG matching the given kanji inside the zip file.
        /// </summary>
        /// <param name="k">Target kanji.</param>
        /// <returns>A kanji strokes entity, never null, that contains either the retrieved data or an
        /// empty byte array when the entry was not found.</returns>
        private KanjiStrokes RetrieveSvg(KanjiEntity k)
        {
            KanjiStrokes strokes = new KanjiStrokes();
            strokes.FramesSvg = new byte[0];

            if (!k.UnicodeValue.HasValue)
            {
                return strokes;
            }

            ZipArchive svgZip = GetSvgZipArchive();
            string entryName = string.Format("{0}_frames.svg", k.UnicodeValue.Value);
            ZipArchiveEntry entry = svgZip.GetEntry(entryName);
            if (entry != null)
            {
                using (Stream stream = entry.Open())
                {
                    strokes.FramesSvg = StringCompressionHelper.Zip(StreamHelper.ReadToEnd(stream));
                    StringCompressionHelper.Unzip(strokes.FramesSvg);
                }
            }

            return strokes;
        }

        private ZipArchive GetSvgZipArchive()
        {
            if (_svgZipArchive == null)
            {
                _svgZipArchive = ZipFile.OpenRead(PathHelper.SvgZipPath);
            }

            return _svgZipArchive;
        }

        private void CloseZipArchive()
        {
            _svgZipArchive.Dispose();
            _svgZipArchive = null;
        }

        #endregion
    }
}
