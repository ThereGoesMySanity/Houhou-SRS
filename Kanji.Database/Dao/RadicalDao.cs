using System.Collections.Generic;
using System.Threading.Tasks;
using Kanji.Database.Entities;
using Kanji.Database.Helpers;
using Kanji.Database.Models;
using SQLite;

namespace Kanji.Database.Dao;

public class RadicalDao : Dao
{
    private static SQLiteAsyncConnection connection => DaoConnection.Instance[DaoConnectionEnum.KanjiDatabase];

    #region Methods

    /// <summary>
    /// Retrieves all radicals.
    /// </summary>
    public async Task<RadicalEntity[]> GetAllRadicals()
    {
        var results = await connection.Table<RadicalEntity>().ToArrayAsync();

        foreach (RadicalEntity radical in results)
        {
            await IncludeKanji(radical);
        }
        return results;
    }

    /// <summary>
    /// Computes and returns which radicals can still be used in a kanji filter in complement to the
    /// given set of filters, and still return kanji results.
    /// </summary>
    public async Task<IEnumerable<RadicalEntity>> GetAvailableRadicals(RadicalGroup[] radicals, string textFilter,
        string meaningFilter, string anyReadingFilter, string onYomiFilter, string kunYomiFilter,
        string nanoriFilter, int jlptLevel, int wkLevel)
    {
        // Compute the filters.
        List<object> parameters = new List<object>();
        string sqlFilter = KanjiDao.BuildKanjiFilterClauses(parameters, radicals, textFilter,
            meaningFilter, anyReadingFilter, onYomiFilter, kunYomiFilter, nanoriFilter,
            jlptLevel, wkLevel);

        return await connection.QueryAsync<RadicalEntity>(
            string.Format(
                "SELECT DISTINCT ckr.{0} Id " + "FROM {1} k JOIN {2} ckr " + "ON (ckr.{3}=k.{4}) {5}",
                SqlHelper.Field_Kanji_Radical_RadicalId,
                SqlHelper.Table_Kanji,
                SqlHelper.Table_Kanji_Radical,
                SqlHelper.Field_Kanji_Radical_KanjiId,
                SqlHelper.Field_Kanji_Id, sqlFilter),
            parameters.ToArray());
    }

    #region Includes

    /// <summary>
    /// Retrieves and includes kanji entities in the given radical entity.
    /// </summary>
    private async Task IncludeKanji(RadicalEntity radical)
    {
        var results = await connection.QueryAsync<KanjiEntity>(
            string.Format(
                "SELECT kr.{0} {1} FROM {2} kr WHERE kr.{3}=?",
                SqlHelper.Field_Kanji_Radical_KanjiId,
                SqlHelper.Field_Kanji_Id,
                SqlHelper.Table_Kanji_Radical,
                SqlHelper.Field_Kanji_Radical_RadicalId),
            radical.ID);

        foreach (KanjiEntity kanji in results)
        {
            radical.Kanji.Add(kanji);
        }
    }

    #endregion

    #endregion
}
