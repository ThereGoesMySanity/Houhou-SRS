﻿using System.Collections.Generic;
using Kanji.Database.Dao;

namespace Kanji.Database.Models
{
    public class SingleFieldIntegerFilterClause : SingleFieldFilterClause
    {
        #region Properties

        public int? Value { get; set; }
        
		/// <summary>
		/// Gets or sets a boolean defining whether the filter should include
		/// or exclude results.
		/// </summary>
		public bool IsInclude { get; set; }

        #endregion

        #region Constructors

        public SingleFieldIntegerFilterClause(string fieldName)
            : base(fieldName)
        {
            IsInclude = true;
        }

        #endregion

        #region Methods

        protected override string DoGetSqlWhereClause(List<object> parameters)
        {
            if (!Value.HasValue)
			{
				return null;
			}

			string clause = string.Empty;

			parameters.Add(Value.Value);

			clause += _fieldName + "= ?";
			return (IsInclude ? string.Empty : "NOT ") + "(" + clause + ")";
        }

        #endregion
    }
}