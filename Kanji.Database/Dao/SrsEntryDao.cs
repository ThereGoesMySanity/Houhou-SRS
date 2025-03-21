using System;
using System.Collections.Generic;
using System.Linq;
using Kanji.Common.Models;
using Kanji.Database.Entities;
using Kanji.Database.Helpers;
using Kanji.Database.Models;
using SQLite;
using System.Threading.Tasks;

namespace Kanji.Database.Dao;

public class SrsEntryDao : Dao
{
    private readonly SQLiteAsyncConnection connection; 
    
    public SrsEntryDao()
    {
        connection = DaoConnection.Instance[DaoConnectionEnum.SrsDatabase];
    }

    public SrsEntryDao(SQLiteAsyncConnection connection)
    {
        this.connection = connection;
    }

    #region Methods

    /// <summary>
    /// Gets all review information for the current date.
    /// </summary>
    /// <returns>Review info for the current date.</returns>
    public async Task<ReviewInfo> GetReviewInfo(bool updateCheckDate = true)
    {
        ReviewInfo info = new();

        // Query the review count for this date.
        info.AvailableReviewsCount = await connection.ExecuteScalarAsync<long>(
            "SELECT COUNT(1) FROM " + SqlHelper.Table_SrsEntry + " se WHERE se."
            + SqlHelper.Field_SrsEntry_SuspensionDate + " IS NULL AND se."
            + SqlHelper.Field_SrsEntry_NextAnswerDate + " <= ?",
            DateTimeOffset.Now.UtcTicks);

        // Query the new review count
        info.RecentReviewsCount = await connection.ExecuteScalarAsync<long>(@$"
            SELECT COUNT(1) FROM {SqlHelper.Table_SrsEntry} se
            CROSS JOIN (SELECT * FROM {SqlHelper.Table_SrsLastCheck} LIMIT 1) lc
            WHERE se.{SqlHelper.Field_SrsEntry_SuspensionDate} IS NULL
            AND se.{SqlHelper.Field_SrsEntry_NextAnswerDate} <= ?
            AND se.{SqlHelper.Field_SrsEntry_NextAnswerDate} >= lc.{SqlHelper.Field_SrsLastCheck_LastChecked}"
            , DateTimeOffset.Now.UtcTicks);

        //Update last checked time
        await connection.ExecuteAsync($"UPDATE {SqlHelper.Table_SrsLastCheck} SET {SqlHelper.Field_SrsLastCheck_LastChecked} = ?"
            , DateTimeOffset.Now.UtcTicks);

        // Query the review count for today.
        DateTimeOffset endOfToday = DateTimeOffset.Now.Date.AddDays(1);
        info.TodayReviewsCount = await connection.ExecuteScalarAsync<long>(
            "SELECT COUNT(1) FROM " + SqlHelper.Table_SrsEntry + " se WHERE se."
            + SqlHelper.Field_SrsEntry_SuspensionDate + " IS NULL AND se."
            + SqlHelper.Field_SrsEntry_NextAnswerDate + " <= ?",
            endOfToday.UtcTicks);

        DateTimeOffset endOfWeek = DateTimeOffset.Now.AddDays(7);
        info.UpcomingReviews = (await connection.QueryAsync<SrsEntry>($@"
            SELECT {SqlHelper.Field_SrsEntry_NextAnswerDate}, {SqlHelper.Field_SrsEntry_CurrentGrade}
            FROM {SqlHelper.Table_SrsEntry}
            WHERE {SqlHelper.Field_SrsEntry_SuspensionDate} IS NULL AND
            {SqlHelper.Field_SrsEntry_NextAnswerDate} <= ?",
            endOfWeek.UtcTicks))
            .GroupBy(e => e.CurrentGrade)
            .ToDictionary(g => g.Key, g => g.Select(e => e.NextAnswerDate.Value).ToList());

        // Query the first review date.
        long? nextAnswerDate = await connection.ExecuteScalarAsync<long?>(
            "SELECT MIN(" + SqlHelper.Field_SrsEntry_NextAnswerDate + ") FROM "
            + SqlHelper.Table_SrsEntry + " WHERE "
            + SqlHelper.Field_SrsEntry_SuspensionDate + " IS NULL AND "
            + SqlHelper.Field_SrsEntry_NextAnswerDate + " NOT NULL");

        if (nextAnswerDate.HasValue)
        {
            info.FirstReviewDate = new DateTimeOffset(nextAnswerDate.Value, TimeSpan.Zero);
        }

        // Query all counts/total info.
        var from = "FROM " + SqlHelper.Table_SrsEntry;
        info.KanjiItemsCount = await connection.ExecuteScalarAsync<long>($"SELECT COUNT({SqlHelper.Field_SrsEntry_AssociatedKanji}) {from}");
        info.VocabItemsCount = await connection.ExecuteScalarAsync<long>($"SELECT COUNT({SqlHelper.Field_SrsEntry_AssociatedVocab}) {from}");
        info.TotalSuccessCount = await connection.ExecuteScalarAsync<long>($"SELECT SUM({SqlHelper.Field_SrsEntry_SuccessCount}) {from}");
        info.TotalFailureCount = await connection.ExecuteScalarAsync<long>($"SELECT SUM({SqlHelper.Field_SrsEntry_FailureCount}) {from}");
        
        // Query item count by level.
        var results = await connection.QueryAsync<(short grade, long itemCount)>($@"
            SELECT {SqlHelper.Field_SrsEntry_CurrentGrade} grade
            ,   SUM(1) itemCount
            FROM {SqlHelper.Table_SrsEntry}
            GROUP BY {SqlHelper.Field_SrsEntry_CurrentGrade}
        ");

        foreach (var level in results)
        {
            info.ReviewsPerLevel.Add(level.grade, level.itemCount);
        }

        return info;
    }

    /// <summary>
    /// Gets all reviews due for the current date.
    /// </summary>
    /// <returns>Reviews due for the current date.</returns>
    public async Task<IEnumerable<SrsEntry>> GetReviews()
    {
        return await connection.QueryAsync<SrsEntry>(
            "SELECT * FROM " + SqlHelper.Table_SrsEntry + " se WHERE se."
            + SqlHelper.Field_SrsEntry_SuspensionDate + " IS NULL AND se."
            + SqlHelper.Field_SrsEntry_NextAnswerDate + " <= ?"
            + " ORDER BY RANDOM()",
            DateTimeOffset.Now.UtcTicks);
    }

    /// <summary>
    /// Gets the number of reviews due for the current date.
    /// </summary>
    /// <returns>Number of reviews due for the current date.</returns>
    public async Task<long> GetReviewsCount()
    {
        return await connection.ExecuteScalarAsync<long>(
                "SELECT COUNT(1) FROM " + SqlHelper.Table_SrsEntry + " se WHERE se."
                + SqlHelper.Field_SrsEntry_SuspensionDate + " IS NULL AND se."
                + SqlHelper.Field_SrsEntry_NextAnswerDate + " <= ?",
                DateTimeOffset.Now.UtcTicks);
    }

    public async Task<SrsEntry> GetItem(long id)
    {
        return await connection.GetAsync<SrsEntry>(id);
    }

    private string AssembleWhereClause(FilterClause[] filterClauses, List<object> outParams)
    {
        string whereClause = string.Empty;
        bool isFiltered = false;

        foreach (FilterClause clause in filterClauses)
        {
            if (clause != null)
            {
                string sqlClause = clause.GetSqlWhereClause(!isFiltered, outParams);
                if (!string.IsNullOrEmpty(sqlClause))
                {
                    whereClause += sqlClause + " ";
                    isFiltered = true;
                }
            }
        }
        return whereClause;
    }

    /// <summary>
    /// Gets a filtered set of SRS entries.
    /// </summary>
    /// <param name="filterClauses">Filter clauses.</param>
    /// <returns>Filtered SRS entries.</returns>
    public async Task<IEnumerable<SrsEntry>> GetFilteredItems(FilterClause[] filterClauses)
    {
        List<object> parameters = new List<object>();
        var whereClause = AssembleWhereClause(filterClauses, parameters);

        return await connection.DeferredQueryAsync<SrsEntry>(
            "SELECT * FROM " + SqlHelper.Table_SrsEntry + " se "
            + whereClause
            + "ORDER BY (se." + SqlHelper.Field_SrsEntry_CreationDate + ") DESC",
            parameters.ToArray());
    }

    public async Task<SrsEntry> GetFilteredItem(FilterClause[] filterClauses, int index)
    {
        List<object> parameters = new();
        var whereClause = AssembleWhereClause(filterClauses, parameters);
        parameters.Add(index);

        return await connection.FindWithQueryAsync<SrsEntry>(
            $@"SELECT * FROM {SqlHelper.Table_SrsEntry} se
            {whereClause}
            ORDER BY (se.{SqlHelper.Field_SrsEntry_CreationDate}) DESC
            LIMIT 1 OFFSET ?",
            parameters.ToArray());
    }

    /// <summary>
    /// Gets the number of items matching the given filter clauses.
    /// </summary>
    /// <param name="filterClauses">Filter clauses to match.</param>
    /// <returns>Number of items matching the filter clauses.</returns>
    public async Task<long> GetFilteredItemsCount(FilterClause[] filterClauses)
    {
        List<object> parameters = new List<object>();
        var whereClause = AssembleWhereClause(filterClauses, parameters);

        return await connection.ExecuteScalarAsync<long>(
            "SELECT COUNT(1) FROM " + SqlHelper.Table_SrsEntry + " se "
            + whereClause,
            parameters.ToArray());
    }

    /// <summary>
    /// Gets a similar item (same kanji reading and type) if found.
    /// </summary>
    /// <param name="entry">Reference entry.</param>
    /// <returns>The first matching item if found. Null otherwise.</returns>
    public async Task<SrsEntry> GetSimilarItem(SrsEntry entry)
    {
        List<object> parameters = new List<object>();
        string request = "SELECT * FROM " + SqlHelper.Table_SrsEntry + " se WHERE se.";
        if (!string.IsNullOrEmpty(entry.AssociatedKanji))
        {
            request += SqlHelper.Field_SrsEntry_AssociatedKanji;
            parameters.Add(entry.AssociatedKanji);
        }
        else
        {
            request += SqlHelper.Field_SrsEntry_AssociatedVocab;
            parameters.Add(entry.AssociatedVocab);
        }
        request += "=?";

        return await connection.FindWithQueryAsync<SrsEntry>(request, parameters.ToArray());
    }

    /// <summary>
    /// Inserts the given entity in the database.
    /// Overrides the ID property of the given entity.
    /// </summary>
    /// <param name="entity">Entity to insert.</param>
    public async Task Add(SrsEntry entity)
    {
        entity.LastUpdateDate = DateTimeOffset.Now;
        await connection.InsertAsync(entity);
    }

    /// <summary>
    /// Updates the given SRS entry.
    /// </summary>
    /// <param name="entity">Entity to update.</param>
    /// <returns>True if the operation was sucessful. False otherwise.</returns>
    public async Task<bool> Update(SrsEntry entity)
    {
        entity.LastUpdateDate = DateTimeOffset.Now;
        return (await connection.UpdateAsync(entity)) == 1;
    }

    /// <summary>
    /// Applies the given value to the meaning note field of the given entries.
    /// </summary>
    /// <param name="entities">Entries to edit.</param>
    /// <param name="value">Value to set to the meaning note field.</param>
    /// <returns>Number of entities edited.</returns>
    public async Task<long> BulkEditMeaningNote(IEnumerable<SrsEntry> entities,
        string value)
    {
        return await BulkEditStringField(entities,
            SqlHelper.Field_SrsEntry_MeaningNote, value);
    }

    /// <summary>
    /// Applies the given value to the reading note field of the given entries.
    /// </summary>
    /// <param name="entities">Entries to edit.</param>
    /// <param name="value">Value to set to the reading note field.</param>
    /// <returns>Number of entities edited.</returns>
    public async Task<long> BulkEditReadingNote(IEnumerable<SrsEntry> entities,
        string value)
    {
        return await BulkEditStringField(entities,
            SqlHelper.Field_SrsEntry_ReadingNote, value);
    }

    /// <summary>
    /// Applies the given value to the Tags field of the given entries.
    /// </summary>
    /// <param name="entities">Entries to edit.</param>
    /// <param name="value">Value to set to the tags field.</param>
    /// <returns>Number of entities edited.</returns>
    public async Task<long> BulkEditTags(IEnumerable<SrsEntry> entities, string value)
    {
        return await BulkEditStringField(entities,
            SqlHelper.Field_SrsEntry_Tags, value);
    }

    /// <summary>
    /// Applies the given value to the given field of the given entries.
    /// </summary>
    /// <param name="entities">Entries to edit.</param>
    /// <param name="fieldName">Name of the field to set.</param>
    /// <param name="value">Value to set for all entities.</param>
    /// <returns>Number of entities edited.</returns>
    private async Task<long> BulkEditStringField(IEnumerable<SrsEntry> entities,
        string fieldName, string value)
    {
        if (!entities.Any())
        {
            return 0;
        }
        var setter = typeof(SrsEntry).GetProperty(fieldName).SetMethod;
        foreach (var entity in entities)
        {
            setter.Invoke(entity, new[]{value});
            entity.LastUpdateDate = DateTimeOffset.Now;
        }
        return await connection.UpdateAllAsync(entities);
    }

    /// <summary>
    /// Applies the specified grade to all given entities. Also schedules the
    /// next review for the sum of now and the given time interval.
    /// </summary>
    /// <param name="entities">Entries ot edit.</param>
    /// <param name="value">Value to set as the new grade.</param>
    /// <param name="delay">Time interval from now to the next review date.
    /// If null, the next review date is set to a null value.</param>
    /// <returns>Number of items successfuly edited.</returns>
    public async Task<long> BulkEditGrade(IEnumerable<SrsEntry> entities, short value, TimeSpan? delay)
    {
        if (!entities.Any())
        {
            return 0;
        }
        foreach (var entity in entities)
        {
            entity.LastUpdateDate = DateTimeOffset.Now;
            entity.NextAnswerDate = delay.HasValue? DateTimeOffset.Now + delay.Value : null;
        }
        return await connection.UpdateAllAsync(entities);
    }

    /// <summary>
    /// Updates the review date for all given entities.
    /// The review date is assumed already modified in the entity.
    /// </summary>
    /// <param name="entities">Entities to update.</param>
    /// <returns>Number of entities updated.</returns>
    public async Task<long> BulkEditReviewDate(IEnumerable<SrsEntry> entities)
    {
        if (!entities.Any())
        {
            return 0;
        }
        foreach (var entity in entities)
        {
            entity.LastUpdateDate = DateTimeOffset.Now;
        }
        return await connection.UpdateAllAsync(entities);
    }

    /// <summary>
    /// Suspends all the given entities (i.e. sets their suspension dates to now).
    /// </summary>
    /// <param name="entities">Entities to suspend.</param>
    /// <returns>Number of items successfuly suspended.</returns>
    public async Task<long> BulkSuspend(IEnumerable<SrsEntry> entities)
    {
        if (!entities.Any())
        {
            return 0;
        }
        foreach (var entity in entities)
        {
            entity.LastUpdateDate = DateTimeOffset.Now;
            entity.SuspensionDate = DateTimeOffset.Now;
        }
        return await connection.UpdateAllAsync(entities);
    }

    /// <summary>
    /// Resumes all the given entities (i.e. deletes the suspension date
    /// and applies the right offset on the next review date).
    /// </summary>
    /// <param name="entities">Entities to resume.</param>
    /// <returns>Number of items successfuly resumed.</returns>
    public async Task<long> BulkResume(IEnumerable<SrsEntry> entities)
    {
        if (!entities.Any())
        {
            return 0;
        }
        foreach (var entity in entities)
        {
            entity.LastUpdateDate = DateTimeOffset.Now;
            entity.NextAnswerDate = entity.NextAnswerDate + (DateTimeOffset.Now - entity.SuspensionDate);
            entity.SuspensionDate = null;
        }
        return await connection.UpdateAllAsync(entities);
    }

    /// <summary>
    /// Removes all the given entities from the database.
    /// </summary>
    /// <param name="entities">Entities to delete.</param>
    /// <returns>Number of items successfuly deleted.</returns>
    public async Task<long> BulkDelete(IEnumerable<SrsEntry> entities)
    {
        if (!entities.Any())
        {
            return 0;
        }
        int count = 0;
        foreach (var entity in entities)
        {
            count += await connection.DeleteAsync<SrsEntry>(entity.ID);
        };
        return count;
    }

    /// <summary>
    /// Removes the entity from the database.
    /// </summary>
    /// <param name="entity">Entity to delete.</param>
    /// <returns>True if the operation was successful. False otherwise.</returns>
    public async Task<bool> Delete(SrsEntry entity)
    {
        return (await connection.DeleteAsync<SrsEntry>(entity.ID)) == 1;
    }

    #endregion
}
