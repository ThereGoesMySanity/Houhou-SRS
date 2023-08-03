using System;
using System.Collections.Generic;

namespace Kanji.Database.Models
{
    public abstract class FilterClause
    {
        /// <summary>
        /// Returns a SQL clause matching this filter.
        /// </summary>
        /// <param name="isFirstClause">Indicates if the filter is the
        /// first to be applied. Used to determine if the clause should
        /// use a WHERE or an OR statement.</param>
        /// <returns>SQL condition clause.</returns>
        internal string GetSqlWhereClause(bool isFirstClause, List<object> parameters)
        {
            string clause = DoGetSqlWhereClause(parameters);
            if (!string.IsNullOrEmpty(clause))
            {
                return (isFirstClause ? "WHERE " : "AND ") + clause;
            }

            return string.Empty;
        }

        protected abstract string DoGetSqlWhereClause(List<object> parameters);
    }
}
