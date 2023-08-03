using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using Kanji.Database.Entities;
using Kanji.Database.Helpers;
using Kanji.Database.Models;
using Kanji.Common.Utility;
using System;

namespace Kanji.Database.Dao
{
    public class KanjiDao : Dao
    {
        private static SQLiteAsyncConnection connection => DaoConnection.Instance[DaoConnectionEnum.KanjiDatabase];
        #region Methods

        /// <summary>
        /// Gets all kanji with minimal info.
        /// </summary>
        /// <returns>All kanji with minimal info.</returns>
        public async Task<List<KanjiEntity>> GetAllKanji()
        {
            return await connection.Table<KanjiEntity>().ToListAsync();
        }

        public async Task<List<KanjiEntity>> CustomQueryAsync(string query)
        {
            return await connection.QueryAsync<KanjiEntity>(query);
        }


        /// <summary>
        /// Gets the first kanji that matches the given character.
        /// </summary>
        /// <param name="character">Character to match.</param>
        /// <returns>First kanji matching the given character.
        /// Null if nothing was found.</returns>
        public async Task<KanjiEntity> GetFirstMatchingKanji(string character)
        {
            KanjiEntity result = await connection.FindWithQueryAsync<KanjiEntity>(string.Format(
                "SELECT * FROM {0} k WHERE k.{1}=? ORDER BY (k.{2} IS NULL),(k.{2});",
                SqlHelper.Table_Kanji,
                SqlHelper.Field_Kanji_Character,
                SqlHelper.Field_Kanji_MostUsedRank),
            character);

            if (result != null)
            {
                await IncludeKanjiMeanings(result);
            }

            return result;
        }

        /// <summary>
        /// Gets a set of kanji matching the given filters.
        /// </summary>
        /// <param name="radicals">Filters out kanji which do not contain all
        /// of the contained radicals.</param>
        /// <param name="textFilter">If set, filters out all kanji that are not
        /// contained in the string.</param>
        /// <param name="meaningFilter">Filter for the meaning of the kanji.</param>
        /// <param name="anyReadingFilter">Filter matching any reading of the kanji.
        /// <remarks>If set, this parameter will override the three reading filters.
        /// </remarks></param>
        /// <param name="onYomiFilter">Filter for the on'yomi reading of the kanji.
        /// <remarks>This parameter will be ignored if
        /// <paramref name="anyReadingFilter"/> is set.</remarks></param>
        /// <param name="kunYomiFilter">Filter for the kun'yomi reading of the kanji.
        /// <remarks>This parameter will be ignored if
        /// <paramref name="anyReadingFilter"/> is set.</remarks></param>
        /// <param name="nanoriFilter">Filter for the nanori reading of the kanji.
        /// <remarks>This parameter will be ignored if
        /// <paramref name="anyReadingFilter"/> is set.</remarks></param>
        /// <returns>Kanji matching the given filters.</returns>
        public async IAsyncEnumerable<KanjiEntity> GetFilteredKanji(RadicalGroup[] radicals, string textFilter,
            string meaningFilter, string anyReadingFilter, string onYomiFilter, string kunYomiFilter,
            string nanoriFilter, int jlptLevel, int wkLevel)
        {
            List<object> parameters = new List<object>();
            string sqlFilter = BuildKanjiFilterClauses(parameters, radicals, textFilter,
                meaningFilter, anyReadingFilter, onYomiFilter, kunYomiFilter, nanoriFilter,
                jlptLevel, wkLevel);


            // FILTERS COMPUTED.
            // Execute the final request.
            List<KanjiEntity> results = await connection.QueryAsync<KanjiEntity>(string.Format(
                "SELECT * FROM {0} k {1}ORDER BY (k.{2} IS NULL),(k.{2});",
                SqlHelper.Table_Kanji,
                sqlFilter,
                SqlHelper.Field_Kanji_MostUsedRank),
            parameters.ToArray());

            foreach (KanjiEntity kanji in results)
            {
                await IncludeKanjiMeanings(kanji);
                await IncludeRadicals(kanji);
                await IncludeSrsEntries(kanji);
                yield return kanji;
            }
        }

        /// <summary>
        /// See <see cref="Kanji.Database.Dao.KanjiDao.GetFilteredKanji"/>.
        /// Returns the result count.
        /// </summary>
        public async Task<long> GetFilteredKanjiCount(RadicalGroup[] radicals, string textFilter,
            string meaningFilter, string anyReadingFilter, string onYomiFilter, string kunYomiFilter,
            string nanoriFilter, int jlptLevel, int wkLevel)
        {
            List<object> parameters = new List<object>();
            string sqlFilter = BuildKanjiFilterClauses(parameters, radicals, textFilter,
                meaningFilter, anyReadingFilter, onYomiFilter, kunYomiFilter, nanoriFilter,
                jlptLevel, wkLevel);

            return await connection.ExecuteScalarAsync<long>(
                string.Format("SELECT COUNT(1) FROM {0} k {1}",
                SqlHelper.Table_Kanji,
                sqlFilter),
                parameters.ToArray());
        }

        //public KanjiStrokes GetKanjiStrokes(long id)
        //{
        //    KanjiStrokes result = null;

        //    DaoConnection connection = null;
        //    try
        //    {
        //        // Create and open synchronously the primary Kanji connection.
        //        connection = DaoConnection.Open(DaoConnectionEnum.KanjiDatabase);

        //        // FILTERS COMPUTED.
        //        // Execute the final request.
        //        IEnumerable<NameValueCollection> results = connection.Query(
        //            "SELECT * "
        //            + "FROM " + SqlHelper.Table_KanjiStrokes + " ks "
        //            + "WHERE ks." + SqlHelper.Field_KanjiStrokes_Id + "=@ks;",
        //        new DaoParameter("@ks", id));

        //        if (results.Any())
        //        {
        //            KanjiStrokesBuilder builder = new KanjiStrokesBuilder();
        //            result = builder.BuildEntity(results.First(), null);
        //        }
        //    }
        //    finally
        //    {
        //        if (connection != null)
        //        {
        //            connection.Dispose();
        //        }
        //    }

        //    return result;
        //}

        public async Task<KanjiStrokes> GetKanjiStrokes(long id)
        {
            return await connection.FindAsync<KanjiStrokes>(id);
        }

        #region Query building

        /// <summary>
        /// Builds the SQL filter clauses to retrieve filtered kanji.
        /// </summary>
        internal static string BuildKanjiFilterClauses(List<object> parameters, RadicalGroup[] radicalGroups,
            string textFilter, string meaningFilter, string anyReadingFilter, string onYomiFilter,
            string kunYomiFilter, string nanoriFilter, int jlptLevel, int wkLevel)
        {
            const int minJlptLevel = Levels.MinJlptLevel;
            const int maxJlptLevel = Levels.MaxJlptLevel;
            const int minWkLevel = Levels.MinWkLevel;
            const int maxWkLevel = Levels.MaxWkLevel;

            string sqlJlptFilter = string.Empty;
            if (jlptLevel >= minJlptLevel && jlptLevel <= maxJlptLevel)
            {
                sqlJlptFilter = string.Format("k.{0}=? ",
                    SqlHelper.Field_Vocab_JlptLevel);

                parameters.Add(jlptLevel);
            }
            else if (jlptLevel < minJlptLevel)
            {
                sqlJlptFilter = string.Format("k.{0} IS NULL ",
                    SqlHelper.Field_Vocab_JlptLevel);
            }

            string sqlWkFilter = string.Empty;
            if (wkLevel >= minWkLevel && wkLevel <= maxWkLevel)
            {
                sqlWkFilter = string.Format("k.{0}=? ",
                    SqlHelper.Field_Vocab_WaniKaniLevel);

                parameters.Add(wkLevel);
            }
            else if (wkLevel > maxWkLevel)
            {
                sqlWkFilter = string.Format("k.{0} IS NULL ",
                    SqlHelper.Field_Vocab_WaniKaniLevel);
            }

            string sqlTextFilter = string.Empty;
            if (!string.IsNullOrWhiteSpace(textFilter))
            {
                // Build the text filter.
                // Example with textFilter="年生まれ" :
                // 
                // WHERE '1959年生まれ' LIKE '%' || k.Character || '%'

                sqlTextFilter = string.Format("? LIKE '%' || k.{0} || '%' ",
                    SqlHelper.Field_Kanji_Character);

                // And add the parameter.
                parameters.Add(textFilter);
            }

            string sqlAnyReadingFilter = string.Empty;
            string sqlOnYomiFilter = string.Empty;
            string sqlKunYomiFilter = string.Empty;
            string sqlNanoriFilter = string.Empty;
            if (!string.IsNullOrWhiteSpace(anyReadingFilter))
            {
                // Build the any reading filter.
                // Example with anyReadingFilter="test" :
                //
                // WHERE (k.KunYomi LIKE '%test%' OR k.OnYomi LIKE '%test%'
                // OR k.Nanori LIKE '%test%')

                sqlAnyReadingFilter = string.Format(
                    "(k.{0} LIKE ? OR k.{1} LIKE ? OR k.{2} LIKE ?) ",
                    SqlHelper.Field_Kanji_KunYomi,
                    SqlHelper.Field_Kanji_OnYomi,
                    SqlHelper.Field_Kanji_Nanori);

                // And add the parameter.
                var param = string.Format("%{0}%", anyReadingFilter);
                parameters.AddRange(Enumerable.Repeat(param, 3));
            }
            else
            {
                // Any reading filter is not set. Browse the other reading filters.
                if (!string.IsNullOrWhiteSpace(onYomiFilter))
                {
                    sqlOnYomiFilter = string.Format("k.{0} LIKE ? ", SqlHelper.Field_Kanji_OnYomi);

                    parameters.Add(string.Format("%{0}%", onYomiFilter));
                }
                if (!string.IsNullOrWhiteSpace(kunYomiFilter))
                {
                    sqlKunYomiFilter = string.Format("k.{0} LIKE ? ", SqlHelper.Field_Kanji_KunYomi);

                    parameters.Add(string.Format("%{0}%", kunYomiFilter));
                }
                if (!string.IsNullOrWhiteSpace(nanoriFilter))
                {
                    sqlNanoriFilter = string.Format("k.{0} LIKE ? ", SqlHelper.Field_Kanji_Nanori);

                    parameters.Add(string.Format("%{0}%", nanoriFilter));
                }
            }

            StringBuilder sqlRadicalFilter = new StringBuilder();
            if (radicalGroups.Any())
            {
                // Build the radical sql filter. For example with:
                // [0] = { 7974, 7975 }
                // [1] = { 7976 }
                // [2] = { 7977 }
                // ... we would want something like:
                //
                //WHERE (SELECT COUNT(*)
                //     FROM
                //     (
                //         SELECT 7976 UNION SELECT 7977
                //         INTERSECT
                //         SELECT kr.Radicals_Id
                //         FROM KanjiRadical kr
                //         WHERE kr.Kanji_Id = k.Id
                //     ))=2
                //     AND (SELECT COUNT(*)
                //     FROM
                //     (
                //         SELECT 7974 UNION SELECT 7975
                //         INTERSECT
                //         SELECT kr.Radicals_Id
                //         FROM KanjiRadical kr
                //         WHERE kr.Kanji_Id = k.Id
                //     )) >= 1

                // Get the mandatory radicals. In our example, these would be {7976,7977}.
                RadicalEntity[] mandatoryRadicals = radicalGroups
                    .Where(g => g.Radicals.Count() == 1)
                    .SelectMany(g => g.Radicals).ToArray();

                // Get the other radical groups. In our example, this would be {{7974,7975}}.
                RadicalGroup[] optionGroups = radicalGroups.Where(g => g.Radicals.Length > 1).ToArray();

                // We need to build one request per option group,
                // and one request for all mandatory radicals.
                int idParamIndex = 0;

                // Start with the request for all mandatory radicals.
                bool hasMandatoryRadicals = mandatoryRadicals.Any();
                if (hasMandatoryRadicals)
                {
                    sqlRadicalFilter.Append("(SELECT COUNT(*) FROM (");
                    for (int i = 0; i < mandatoryRadicals.Length; i++)
                    {
                        RadicalEntity radical = mandatoryRadicals[i];
                        sqlRadicalFilter.AppendFormat("SELECT ? ", idParamIndex);
                        if (i < mandatoryRadicals.Length - 1)
                        {
                            sqlRadicalFilter.Append("UNION ");
                        }
                        parameters.Add(radical.ID);
                    }
                    sqlRadicalFilter.AppendFormat(
                        "INTERSECT SELECT kr.{0} FROM {1} kr WHERE kr.{2}=k.{3}))=? ",
                        SqlHelper.Field_Kanji_Radical_RadicalId,
                        SqlHelper.Table_Kanji_Radical,
                        SqlHelper.Field_Kanji_Radical_KanjiId,
                        SqlHelper.Field_Kanji_Id);
                    parameters.Add(mandatoryRadicals.Count());
                }

                // Now build the requests for the option groups.
                for (int i = 0; i < optionGroups.Length; i++)
                {
                    RadicalGroup optionGroup = optionGroups[i];
                    if (hasMandatoryRadicals || i > 0)
                        sqlRadicalFilter.Append("AND ");

                    sqlRadicalFilter.Append("(SELECT COUNT(*) FROM (");
                    foreach (RadicalEntity radical in optionGroup.Radicals)
                    {
                        sqlRadicalFilter.Append("SELECT ? ");
                        if (optionGroup.Radicals.Last() != radical)
                        {
                            sqlRadicalFilter.Append("UNION ");
                        }
                        parameters.Add(radical.ID);
                    }
                    sqlRadicalFilter.AppendFormat(
                        "INTERSECT SELECT kr.{0} FROM {1} kr WHERE kr.{2}=k.{3}))>=1 ",
                        SqlHelper.Field_Kanji_Radical_RadicalId,
                        SqlHelper.Table_Kanji_Radical,
                        SqlHelper.Field_Kanji_Radical_KanjiId,
                        SqlHelper.Field_Kanji_Id);
                }
            }

            string sqlMeaningFilter = string.Empty;
            if (!string.IsNullOrWhiteSpace(meaningFilter))
            {
                // Build the meaning filter.
                // Example with meaningFilter="test" :
                //
                // WHERE EXISTS (SELECT * FROM KanjiMeaningSet km
                // WHERE km.Kanji_Id=k.Id AND km.Language IS NULL
                // AND km.Meaning LIKE '%test%') 

                sqlMeaningFilter = string.Format(
                    "k.{0} IN (SELECT km.{1} FROM {2} km WHERE km.{3} IS NULL AND km.{4} LIKE ?) ",
                    SqlHelper.Field_Kanji_Id,
                    SqlHelper.Field_KanjiMeaning_KanjiId,
                    SqlHelper.Table_KanjiMeaning,
                    SqlHelper.Field_KanjiMeaning_Language,
                    SqlHelper.Field_KanjiMeaning_Meaning);

                // And add the parameter.
                parameters.Add("%" + meaningFilter + "%");
            }
            
            string[] sqlArgs =
            {
                sqlJlptFilter,
                sqlWkFilter,
                sqlTextFilter,
                sqlAnyReadingFilter,
                sqlOnYomiFilter,
                sqlKunYomiFilter,
                sqlNanoriFilter,
                sqlRadicalFilter.ToString(),
                sqlMeaningFilter
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
        /// Retrieves and includes the meanings of the given kanji in the entity.
        /// </summary>
        internal static async Task IncludeKanjiMeanings(KanjiEntity kanji)
        {
            var meanings = await connection.QueryAsync<KanjiMeaning>(
                string.Format("SELECT * FROM {0} km WHERE km.{1} = ? AND km.{2} IS NULL;",
                SqlHelper.Table_KanjiMeaning,
                SqlHelper.Field_KanjiMeaning_KanjiId,
                SqlHelper.Field_KanjiMeaning_Language),
                kanji.ID);

            foreach (KanjiMeaning meaning in meanings)
            {
                meaning.Kanji = kanji;
                kanji.Meanings.Add(meaning);
            }
        }

        /// <summary>
        /// Retrieves and includes the radicals of the given kanji in the entity.
        /// </summary>
        internal static async Task IncludeRadicals(KanjiEntity kanji)
        {
            var radicals = await connection.QueryAsync<RadicalEntity>(
                string.Format("SELECT * FROM {0} r JOIN {1} kr ON (kr.{2}=r.{3}) WHERE kr.{4}= ?;",
                SqlHelper.Table_Radical,
                SqlHelper.Table_Kanji_Radical,
                SqlHelper.Field_Kanji_Radical_RadicalId,
                SqlHelper.Field_Radical_Id,
                SqlHelper.Field_Kanji_Radical_KanjiId),
                kanji.ID);
            
            foreach (RadicalEntity radical in radicals)
            {
                kanji.Radicals.Add(radical);
            }
        }

        /// <summary>
        /// Retrieves and includes the SRS entries matching the given kanji and includes
        /// them in the entity.
        /// </summary>
        internal static async Task IncludeSrsEntries(KanjiEntity kanji)
        {
            var srsConnection = DaoConnection.Instance[DaoConnectionEnum.SrsDatabase];
            var srsEntries = await srsConnection.QueryAsync<SrsEntry>(
                string.Format("SELECT * FROM {0} srs WHERE srs.{1}=?",
                SqlHelper.Table_SrsEntry,
                SqlHelper.Field_SrsEntry_AssociatedKanji),
                kanji.Character);

            foreach (SrsEntry srsEntry in srsEntries)
            {
                kanji.SrsEntries.Add(srsEntry);
            }
        }

        #endregion

        #endregion
    }
}
