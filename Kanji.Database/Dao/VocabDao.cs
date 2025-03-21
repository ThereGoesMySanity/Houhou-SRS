using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kanji.Common.Utility;
using Kanji.Database.Entities;
using Kanji.Database.Helpers;
using Kanji.Database.Models;
using SQLite;

namespace Kanji.Database.Dao;

public class VocabDao : Dao
{
    private static SQLiteAsyncConnection connection => DaoConnection.Instance[DaoConnectionEnum.KanjiDatabase];

    #region Properties
    
    private static string joinString_VocabEntity_VocabMeaning
    {
        get
        {
            return string.Format("JOIN {0} vvm ON (vvm.{1}=v.{2}) ",
                SqlHelper.Table_Vocab_VocabMeaning,
                SqlHelper.Field_Vocab_VocabMeaning_VocabId,
                SqlHelper.Field_Vocab_Id);
        }
    }
    
    private static string joinString_VocabMeaningSet
    {
        get
        {
            return string.Format("JOIN {0} vm ON (vm.{1}=vvm.{2}) ",
                SqlHelper.Table_VocabMeaning,
                SqlHelper.Field_VocabMeaning_Id,
                SqlHelper.Field_Vocab_VocabMeaning_VocabMeaningId);
        }
    }

    #endregion

    #region Methods

    public async Task<IEnumerable<VocabEntity>> GetAllVocab()
    {
        return await connection.Table<VocabEntity>().ToListAsync();
    }

    public async IAsyncEnumerable<VocabEntity> GetVocabByReadings(string kanjiReading, string kanaReading)
    {
        if (kanjiReading == kanaReading)
        {
            List<VocabEntity> vocabs = await connection.QueryAsync<VocabEntity>(
                string.Format("SELECT * FROM {0} WHERE {1}=?",
                SqlHelper.Table_Vocab,
                SqlHelper.Field_Vocab_KanaWriting),
                kanaReading);

            if (vocabs.Count() == 1)
            {
                await IncludeMeanings(vocabs[0]);
                yield return vocabs.First();
            }
            yield break;
        }
        List<VocabEntity> fullMatch = await connection.QueryAsync<VocabEntity>(
                string.Format("SELECT * FROM {0} WHERE {1}=? AND {2}=?",
                SqlHelper.Table_Vocab,
                SqlHelper.Field_Vocab_KanaWriting,
                SqlHelper.Field_Vocab_KanjiWriting),
                kanaReading, kanjiReading);

        foreach (var result in fullMatch)
        {
            await IncludeMeanings(result);
            yield return result;
        }
    }

    public async Task<VocabEntity> GetSingleVocabByKanaReading(string kanaReading)
    {
        long count = await connection.ExecuteScalarAsync<long>(
            string.Format("SELECT COUNT(1) FROM {0} WHERE {1}=?",
            SqlHelper.Table_Vocab,
            SqlHelper.Field_Vocab_KanaWriting),
            kanaReading);

        if (count == 1)
        {
            return await connection.FindWithQueryAsync<VocabEntity>(
                string.Format("SELECT * FROM {0} WHERE {1}=?",
                SqlHelper.Table_Vocab,
                SqlHelper.Field_Vocab_KanaWriting),
                kanaReading);
        }
        return null;
    }

    public async Task<bool> UpdateFrequencyRankOnSingleKanaMatch(string kanaReading, int rank)
    {
        long count = await connection.ExecuteScalarAsync<long>(
            string.Format("SELECT COUNT(1) FROM {0} WHERE {1}=?",
            SqlHelper.Table_Vocab,
            SqlHelper.Field_Vocab_KanaWriting),
            kanaReading);

        if (count == 1)
        {
            return (await connection.ExecuteAsync(
                string.Format("UPDATE {0} SET {1}={1}+? WHERE {2}=?",
                    SqlHelper.Table_Vocab,
                    SqlHelper.Field_Vocab_FrequencyRank,
                    SqlHelper.Field_Vocab_KanaWriting),
                rank, kanaReading)) == 1;
        }
        return false;
    }

    public async Task UpdateFrequencyRank(VocabEntity vocab, int rank)
    {
        vocab.FrequencyRank = rank;
        await connection.UpdateAsync(vocab);
    }

    /// <summary>
    /// Gets the first vocab that exactly matches the given reading.
    /// </summary>
    /// <param name="reading">Reading to match.</param>
    /// <returns>First matching vocab, or null if not found.</returns>
    public async IAsyncEnumerable<VocabEntity> GetMatchingVocab(string reading)
    {
        List<VocabEntity> vocabs = await connection.QueryAsync<VocabEntity>(
                string.Format("SELECT v.* FROM {0} v WHERE v.{1}=? ORDER BY v.{2} DESC",
                SqlHelper.Table_Vocab,
                SqlHelper.Field_Vocab_KanjiWriting,
                SqlHelper.Field_Vocab_IsCommon),
            reading);

        if (!vocabs.Any())
        {
            vocabs = await connection.QueryAsync<VocabEntity>(
                    string.Format("SELECT v.* FROM {0} v WHERE v.{1}=? ORDER BY v.{2} DESC",
                    SqlHelper.Table_Vocab,
                    SqlHelper.Field_Vocab_KanaWriting,
                    SqlHelper.Field_Vocab_IsCommon),
                reading);
        }
        foreach (VocabEntity result in vocabs)
        {
            await IncludeMeanings(result);
            yield return result;
        }
    }

    /// <summary>
    /// Retrieves and returns the complete VocabEntity matching the given ID.
    /// </summary>
    /// <param name="id">Id to search.</param>
    /// <returns>The VocabEntity that matches the given ID, or null if not found.</returns>
    public async Task<VocabEntity> GetVocabById(long id)
    {
        var vocab = await connection.GetAsync<VocabEntity>(id);
        if (vocab != null)
        {
            await IncludeCategories(vocab);
            await IncludeMeanings(vocab);
            await IncludeKanji(vocab);
            await IncludeSrsEntries(vocab);
            await IncludeVariants(vocab);
        }
        return vocab;
    }

    /// <summary>
    /// Retrieves and returns the collection of vocab matching the
    /// given filters.
    /// </summary>
    /// <param name="kanji">Kanji filter. Only vocab containing this
    /// kanji will be filtered in.</param>
    /// <param name="readingFilter">Reading filter. Only vocab containing
    /// this string in their kana or kanji reading will be filtered in.</param>
    /// <param name="meaningFilter">Meaning filter. Only vocab containing
    /// this string as part of at least one of their meaning entries will
    /// be filtered in.</param>
    /// <param name="categoryFilter">If not null, this category is used as the filter.</param>
    /// <param name="jlptLevel">The JLPT level to filter
    /// (1-5, where a lower value means it is not covered on the JLPT
    /// and a higher value means that this filter will be ignored).</param>
    /// <param name="wkLevel">The WaniKani level to filter
    /// (1-60, where a higher value means it is not taught by WaniKani
    /// and a lower value means that this filter will be ignored).</param>
    /// <param name="isCommonFirst">Indicates if common vocab should be
    /// presented first. If false, results are sorted only by the length
    /// of their writing (asc or desc depending on the parameter)</param>
    /// <param name="isShortWritingFirst">Indicates if results should
    /// be sorted by ascending or descending writing length.
    /// If True, short readings come first. If False, long readings
    /// come first.</param>
    /// <returns>Vocab entities matching the filters.</returns>
    public async IAsyncEnumerable<VocabEntity> GetFilteredVocab(KanjiEntity kanji, string[] vocab,
        string readingFilter, string meaningFilter, string searchFilter,
        VocabCategory categoryFilter,
        int jlptLevel, int wkLevel,
        bool isCommonFirst, bool isShortWritingFirst)
    {
        List<object> parameters = new List<object>();
        string sqlFilterClauses = BuildVocabFilterClauses(parameters, kanji, vocab,
            readingFilter, meaningFilter, searchFilter, categoryFilter, jlptLevel, wkLevel);

        string sortClause = "ORDER BY ";
        if (isCommonFirst)
        {
            sortClause += string.Format("v.{0} DESC,", SqlHelper.Field_Vocab_IsCommon);
        }
        sortClause += string.Format("length(v.{0}) {1}",
            SqlHelper.Field_Vocab_KanaWriting,
            (isShortWritingFirst ? "ASC" : "DESC"));

        
        var vocabs = await connection.DeferredQueryAsync<VocabEntity>(
                string.Format("SELECT DISTINCT v.* FROM {0} v {1}{2}",
                SqlHelper.Table_Vocab,
                sqlFilterClauses,
                sortClause),
            parameters.ToArray());

        foreach (var result in vocabs)
        {
            await IncludeCategories(result);
            await IncludeMeanings(result);
            await IncludeKanji(result);
            await IncludeSrsEntries(result);
            await IncludeVariants(result);
            yield return result;
        }
    }

    /// <summary>
    /// See <see cref="Kanji.Database.Dao.VocabDao.GetFilteredVocab"/>.
    /// Returns the results count.
    /// </summary>
    public async Task<long> GetFilteredVocabCount(KanjiEntity kanji, string[] vocab,
        string readingFilter, string meaningFilter, string searchFilter, VocabCategory categoryFilter, int jlptLevel, int wkLevel)
    {
        List<object> parameters = new List<object>();
        string sqlFilterClauses = BuildVocabFilterClauses(parameters, kanji, vocab,
            readingFilter, meaningFilter, searchFilter, categoryFilter, jlptLevel, wkLevel);
        

        return await connection.ExecuteScalarAsync<long>(
            $"SELECT count(distinct v.{SqlHelper.Field_Vocab_Id}) FROM {SqlHelper.Table_Vocab} v {sqlFilterClauses}",
            parameters.ToArray());
    }

    /// <summary>
    /// Retrieves all vocab categories.
    /// </summary>
    /// <returns>All vocab categories.</returns>
    public async Task<IEnumerable<VocabCategory>> GetAllCategories()
    {
        return await connection.Table<VocabCategory>().ToArrayAsync();
    }

    /// <summary>
    /// Retrieves the category with the given label.
    /// </summary>
    /// <param name="label">Label of the category to retrieve.</param>
    /// <returns>Matching category if any. Null otherwise.</returns>
    public async Task<VocabCategory> GetCategoryByLabel(string label)
    {
        return await connection.FindWithQueryAsync<VocabCategory>(
                string.Format("SELECT * FROM {0} WHERE {1}=?",
                SqlHelper.Table_VocabCategory,
                SqlHelper.Field_VocabCategory_Label),
                label);
    }

    #region Query building

    /// <summary>
    /// Builds and returns the vocab filter SQL clauses from the given
    /// filters.
    /// </summary>
    internal string BuildVocabFilterClauses(List<object> parameters, 
        KanjiEntity kanji, string[] vocab,
        string readingFilter, string meaningFilter, string searchFilter,
        VocabCategory categoryFilter,
        int jlptLevel, int wkLevel)
    {
        const int minJlptLevel = Levels.MinJlptLevel;
        const int maxJlptLevel = Levels.MaxJlptLevel;
        const int minWkLevel = Levels.MinWkLevel;
        const int maxWkLevel = Levels.MaxWkLevel;

        string sqlJlptFilter = string.Empty;
        if (jlptLevel >= minJlptLevel && jlptLevel <= maxJlptLevel)
        {
            sqlJlptFilter = string.Format("v.{0}=? ",
                SqlHelper.Field_Vocab_JlptLevel);

            parameters.Add(jlptLevel);
        }
        else if (jlptLevel < minJlptLevel)
        {
            sqlJlptFilter = string.Format("v.{0} IS NULL ",
                SqlHelper.Field_Vocab_JlptLevel);
        }

        string sqlWkFilter = string.Empty;
        if (wkLevel >= minWkLevel && wkLevel <= maxWkLevel)
        {
            sqlWkFilter = string.Format("v.{0}=? ",
                SqlHelper.Field_Vocab_WaniKaniLevel);

            parameters.Add(wkLevel);
        }
        else if (wkLevel > maxWkLevel)
        {
            sqlWkFilter = string.Format("v.{0} IS NULL ",
                SqlHelper.Field_Vocab_WaniKaniLevel);
        }

        string sqlVocabFilter = string.Empty;
        if (vocab != null)
        {
            //WHERE v.KanjiWriting IN ( '漢字' , ... )
            parameters.AddRange(vocab);
            string list = $"( {string.Join(" , ", Enumerable.Repeat("?", vocab.Length))} )";
            sqlVocabFilter =$@"(v.{SqlHelper.Field_Vocab_KanjiWriting} IN {list} 
                OR (v.{SqlHelper.Field_Vocab_KanjiWriting} IS NULL AND v.{SqlHelper.Field_Vocab_KanaWriting} IN {list}))";
        }

        string sqlKanjiFilter = string.Empty;
        if (kanji != null)
        {
            // Build the sql kanji filter clause.
            // Example with the kanji '達' :
            //
            // WHERE v.KanjiWriting LIKE '%達%'

            sqlKanjiFilter = string.Format("v.{0} LIKE ? ",
                SqlHelper.Field_Vocab_KanjiWriting);

            parameters.Add($"%{kanji.Character}%");
        }

        string sqlReadingFilter = string.Empty;
        if (!string.IsNullOrWhiteSpace(readingFilter))
        {
            // Build the sql reading filter clause.
            // Example with readingFilter="かな" :
            // 
            // WHERE v.KanaWriting LIKE '%かな%' OR
            // v.KanjiWriting LIKE '%かな%'

            sqlReadingFilter = string.Format("(v.{0} LIKE ? OR v.{1} LIKE ?) ",
                SqlHelper.Field_Vocab_KanaWriting,
                SqlHelper.Field_Vocab_KanjiWriting);

            parameters.Add($"%{readingFilter}%");
            parameters.Add($"%{readingFilter}%");
        }

        string sqlSharedJoins = string.Empty;

        string sqlMeaningFilterJoins = string.Empty;
        string sqlMeaningFilter = string.Empty;
        if (!string.IsNullOrWhiteSpace(meaningFilter))
        {
            // Build the sql meaning filter clause and join clauses.
            // Example of filter clause with meaningFilter="test" :
            //
            // WHERE vm.Meaning LIKE '%test%'
            
            // First, build the join clause if it does not already exist. This will be included before the filters.
            if (string.IsNullOrEmpty(sqlSharedJoins))
            {
                sqlSharedJoins = joinString_VocabEntity_VocabMeaning;
            }
            sqlMeaningFilterJoins = joinString_VocabMeaningSet;

            // Once the join clauses are done, build the filter itself.
            sqlMeaningFilter = string.Format("vm.{0} LIKE ? ",
                SqlHelper.Field_VocabMeaning_Meaning);

            parameters.Add($"%{meaningFilter}%");
        }

        if (!string.IsNullOrWhiteSpace(searchFilter))
        {
            if (string.IsNullOrEmpty(sqlSharedJoins))
            {
                sqlSharedJoins = joinString_VocabEntity_VocabMeaning;
            }
            sqlMeaningFilterJoins = joinString_VocabMeaningSet;

            sqlKanjiFilter = $@"(v.{SqlHelper.Field_Vocab_KanjiWriting} LIKE ? 
                    OR v.{SqlHelper.Field_Vocab_KanaWriting} LIKE ? 
                    OR vm.{SqlHelper.Field_VocabMeaning_Meaning} LIKE ?)";

            parameters.Add($"%{searchFilter}%");
            parameters.Add($"%{searchFilter}%");
            parameters.Add($"%{searchFilter}%");
        }
        
        string sqlCategoryFilterJoins = string.Empty;
        string sqlCategoryFilter = string.Empty;
        if (categoryFilter != null)
        {
            // Build the filter clause for the vocab category.
            // Note that the category is actually associated either with the vocab itself or with a MEANING,
            // so we need to grab any vocab which itself has said category, or of which ONE OF THE MEANINGS
            // is of said category.
            // Example of filter clause with category.ID=42 :
            //
            // WHERE vc.Categories_ID=42 OR mc.Categories_ID=42
            
            if (string.IsNullOrEmpty(sqlSharedJoins))
            {
                sqlSharedJoins = joinString_VocabEntity_VocabMeaning;
            }
            
            /* TODO: Currently, only the meanings are checked for category matches, not vocab items themselves.
                * This is because of several reasons:
                * 
                * 1) In the current database, not a single vocab has a category attached.
                *    Only meanings do.
                * 2) Saving some performance by not doing a check that does not do anything.
                * 
                * This does mean that this query must be changed if vocab items ever
                * get a category attached to them.
                * 
                * Actually, scratch that. Ateji DO have a category attached to the vocab themselves.
                */

            string subQuery = string.Format("(SELECT m.{0} FROM {1} m WHERE m.{2}=?)",
                SqlHelper.Field_VocabMeaning_VocabCategory_VocabMeaningId,
                SqlHelper.Table_VocabMeaning_VocabCategory,
                SqlHelper.Field_VocabMeaning_VocabCategory_VocabCategoryId);
            
            // First, build the join clause if it does not already exist. This will be included before the filters.
            sqlCategoryFilterJoins = string.IsNullOrEmpty(sqlMeaningFilterJoins) ? joinString_VocabMeaningSet : null;
            
            // Once the join clauses are done, build the filter itself.
            sqlCategoryFilter = string.Format("vm.{0} IN {1} ",
                SqlHelper.Field_VocabMeaning_Id, subQuery);

            parameters.Add(categoryFilter.ID);
        }

        string[] sqlArgs =
        {
            sqlSharedJoins,
            sqlMeaningFilterJoins,
            sqlCategoryFilterJoins,
            sqlJlptFilter,
            sqlWkFilter,
            sqlKanjiFilter,
            sqlVocabFilter,
            sqlReadingFilter,
            sqlMeaningFilter,
            sqlCategoryFilter
        };
        
        bool isFiltered = false;
        for (int i = 0; i < sqlArgs.Length; i++)
        {
            string arg = sqlArgs[i];
            if (string.IsNullOrEmpty(arg) || arg.StartsWith("JOIN"))
                continue;

            sqlArgs[i] = (isFiltered ? "AND " : "WHERE ") + arg;
            isFiltered = true;
        }

        return string.Concat(sqlArgs);
    }

    #endregion

    #region Includes

    /// <summary>
    /// Includes the kanji of the given vocab in the entity.
    /// </summary>
    private async Task IncludeKanji(VocabEntity vocab)
    {
        var results = await connection.QueryAsync<KanjiEntity>(
            string.Format("SELECT k.* FROM {0} kv JOIN {1} k ON (k.{2}=kv.{3}) WHERE kv.{4}=?",
            SqlHelper.Table_Kanji_Vocab,
            SqlHelper.Table_Kanji,
            SqlHelper.Field_Kanji_Id,
            SqlHelper.Field_Kanji_Vocab_KanjiId,
            SqlHelper.Field_Kanji_Vocab_VocabId),
            vocab.ID);

        foreach (var kanji in results)
        {
            await KanjiDao.IncludeKanjiMeanings(kanji);
            await KanjiDao.IncludeRadicals(kanji);
            await KanjiDao.IncludeSrsEntries(kanji);
            vocab.Kanji.Add(kanji);
        }
    }

    /// <summary>
    /// Includes the vocab variants in the entity.
    /// </summary>
    private async Task IncludeVariants(VocabEntity vocab)
    {
        var results = await connection.QueryAsync<VocabEntity>(
            string.Format("SELECT * FROM {0} WHERE {1}=? AND {2}!=?",
            SqlHelper.Table_Vocab,
            SqlHelper.Field_Vocab_Seq,
            SqlHelper.Field_Vocab_Id),
            vocab.Seq,
            vocab.ID);

        foreach(var variant in results)
        {
            vocab.Variants.Add(variant);
        }
    }

    /// <summary>
    /// Include the categories of the given vocab in the entity.
    /// </summary>
    private async Task IncludeCategories(VocabEntity vocab)
    {
        var categories = await connection.QueryAsync<VocabCategory>(
                string.Format("SELECT vc.* FROM {0} vcv JOIN {1} vc ON (vcv.{2}=vc.{3}) WHERE vcv.{4}=?",
                SqlHelper.Table_VocabCategory_Vocab,
                SqlHelper.Table_VocabCategory,
                SqlHelper.Field_VocabCategory_Vocab_VocabCategoryId,
                SqlHelper.Field_VocabCategory_Id,
                SqlHelper.Field_VocabCategory_Vocab_VocabId),
            vocab.ID);

        foreach (var category in categories)
        {
            vocab.Categories.Add(category);
        }
    }

    /// <summary>
    /// Includes the meanings of the given vocab in the entity.
    /// </summary>
    private async Task IncludeMeanings(VocabEntity vocab)
    {
        var meanings = await connection.QueryAsync<VocabMeaning>(
                string.Format("SELECT vm.* FROM {0} vvm JOIN {1} vm ON (vvm.{2}=vm.{3}) WHERE vvm.{4}=?",
                SqlHelper.Table_Vocab_VocabMeaning,
                SqlHelper.Table_VocabMeaning,
                SqlHelper.Field_Vocab_VocabMeaning_VocabMeaningId,
                SqlHelper.Field_VocabMeaning_Id,
                SqlHelper.Field_Vocab_VocabMeaning_VocabId),
            vocab.ID);

        foreach (var meaning in meanings)
        {
            await IncludeMeaningCategories(meaning);
            vocab.Meanings.Add(meaning);
        }
    }

    /// <summary>
    /// Includes the categories of the given meaning in the entity.
    /// </summary>
    private async Task IncludeMeaningCategories(VocabMeaning meaning)
    {
        var categories = await connection.QueryAsync<VocabCategory>(
                string.Format("SELECT vc.* FROM {0} vmvc JOIN {1} vc ON (vmvc.{2}=vc.{3}) WHERE vmvc.{4}=?",
                SqlHelper.Table_VocabMeaning_VocabCategory,
                SqlHelper.Table_VocabCategory,
                SqlHelper.Field_VocabMeaning_VocabCategory_VocabCategoryId,
                SqlHelper.Field_VocabCategory_Id,
                SqlHelper.Field_VocabMeaning_VocabCategory_VocabMeaningId),
            meaning.ID);

        foreach(var category in categories)
        {
            meaning.Categories.Add(category);
        }
    }

    /// <summary>
    /// Retrieves and includes the SRS entries matching the given vocab and includes
    /// them in the entity.
    /// </summary>
    private async Task IncludeSrsEntries(VocabEntity vocab)
    {
        var srsConnection = DaoConnection.Instance[DaoConnectionEnum.SrsDatabase];
        string value = string.IsNullOrEmpty(vocab.KanjiWriting) ?
            vocab.KanaWriting
            : vocab.KanjiWriting;

        var entries = await srsConnection.QueryAsync<SrsEntry>(
            string.Format("SELECT * FROM {0} srs WHERE srs.{1}=?",
            SqlHelper.Table_SrsEntry,
            SqlHelper.Field_SrsEntry_AssociatedVocab),
            value);

        foreach(var entry in entries)
        {
            vocab.SrsEntries.Add(entry);
        }
    }

    #endregion

    #endregion
}
