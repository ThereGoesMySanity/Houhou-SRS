using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Kanji.Database.Dao;
using Microsoft.Extensions.Logging;

namespace Kanji.DatabaseMaker
{
    /// <summary>
    /// The goal of this project is to generate a database file from the following files:
    /// - kanjidic2.xml (Contains a number of kanji with info)
    /// - kradfile (Contains radicals by kanji for a number of common kanji)
    /// - kradfile2 (Contains radicals by kanji for a number of less common kanji)
    /// - JMdict.xml (Contains vocab)
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            // Add extra codepages
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // Initialize log4net.
            var logFactory = LoggerFactory.Create(b => b.AddConsole());

            // Create a logger.
            var log = logFactory.CreateLogger<Program>();
            log.LogInformation("Starting.");

            DaoConnection.Instance = new DaoConnection(Path.Combine(AppContext.BaseDirectory, "KanjiDatabase.sqlite"), null);
            // Get and store radicals.
            log.LogInformation("Getting radicals.");
            RadicalEtl radicalEtl = new RadicalEtl(logFactory.CreateLogger<RadicalEtl>());
            await radicalEtl.ExecuteAsync();
            
            log.LogInformation("Retrieved and stored {0} radicals from {1} compositions.",
                radicalEtl.RadicalCount, radicalEtl.RadicalDictionary.Count());

            // Get and store kanji.
            log.LogInformation("Getting kanji.");
            KanjiEtl kanjiEtl = new KanjiEtl(radicalEtl.RadicalDictionary, logFactory.CreateLogger<KanjiEtl>());
            await kanjiEtl.ExecuteAsync();
            log.LogInformation("Retrieved and stored {0} kanji.", kanjiEtl.KanjiCount);

            // Get and store vocab.
            log.LogInformation("Getting vocab.");
            VocabEtl vocabEtl = new VocabEtl(logFactory.CreateLogger<VocabEtl>());
            await vocabEtl.ExecuteAsync();
            log.LogInformation("Retrieved and stored {0} vocabs.", vocabEtl.VocabCount);

            // Log.
            log.LogInformation("{0}{0}*****{0}Process report{0}*****", Environment.NewLine, Environment.NewLine, Environment.NewLine, Environment.NewLine);
            log.LogInformation("+ {0} radicals", radicalEtl.RadicalCount);
            log.LogInformation("+ {0} kanji", kanjiEtl.KanjiCount);
            log.LogInformation("  + {0} kanji meanings", kanjiEtl.KanjiMeaningCount);
            log.LogInformation("  + {0} Kanji-Radical links", kanjiEtl.KanjiRadicalCount);
            log.LogInformation("+ {0} vocab categories", vocabEtl.VocabCategoryCount);
            log.LogInformation("+ {0} vocabs", vocabEtl.VocabCount);
            log.LogInformation("  + {0} vocab meanings", vocabEtl.VocabMeaningCount);
            log.LogInformation("    + {0} vocab meaning entries", vocabEtl.VocabMeaningEntryCount);
            log.LogInformation("  + {0} Kanji-Vocab links", vocabEtl.KanjiVocabCount);
            log.LogInformation("  + {0} Vocab-VocabCategory links", vocabEtl.VocabVocabCategoryCount);
            log.LogInformation("  + {0} Vocab-VocabMeaning links", vocabEtl.VocabVocabMeaningCount);
            log.LogInformation("  + {0} VocabMeaning-VocabCategory links", vocabEtl.VocabMeaningVocabCategoryCount);
            log.LogInformation("TOTAL: {0} items added.", radicalEtl.RadicalCount + kanjiEtl.KanjiCount
                + kanjiEtl.KanjiMeaningCount + kanjiEtl.KanjiRadicalCount + vocabEtl.KanjiVocabCount
                + vocabEtl.VocabCategoryCount + vocabEtl.VocabCount + vocabEtl.VocabMeaningCount
                + vocabEtl.VocabMeaningEntryCount + vocabEtl.VocabMeaningVocabCategoryCount
                + vocabEtl.VocabVocabCategoryCount + vocabEtl.VocabVocabMeaningCount);

            log.LogInformation("Ending process.");
        }
    }
}
