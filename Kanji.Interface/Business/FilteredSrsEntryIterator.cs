﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kanji.Database.Dao;
using Kanji.Database.Entities;
using Kanji.Interface.Models;

namespace Kanji.Interface.Business
{
    public class FilteredSrsEntryIterator : FilteredItemIterator<SrsEntry>
    {
        #region Fields

        private SrsEntryDao _srsEntryDao;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the behavior of the iterator.
        /// If True, the iterator will iterate over items available
        /// for review.
        /// If False, the iterator will use the filter to return the
        /// matching items.
        /// </summary>
        public bool IsReviewIterator { get; set; }

        #endregion

        #region Constructors

        public FilteredSrsEntryIterator()
            : this(new SrsEntryFilter())
        {
            IsReviewIterator = true;
        }

        public FilteredSrsEntryIterator(SrsEntryFilter filter)
            : base(filter)
        {
            _srsEntryDao = new SrsEntryDao();
        }

        #endregion

        #region Methods

        protected override async IAsyncEnumerable<SrsEntry> DoFilter()
        {
            if (IsReviewIterator)
            {
                foreach(var r in await _srsEntryDao.GetReviews()) yield return r;
            }
            else if (!_currentFilter.IsEmpty() || _currentFilter.ForceFilter)
            {
                foreach(var r in await _srsEntryDao.GetFilteredItems(((SrsEntryFilter)_currentFilter).FilterClauses))
                {
                    yield return r;
                }
            }
        }

        protected override async Task<int> GetItemCount()
        {
            if (IsReviewIterator)
            {
                return (int)await _srsEntryDao.GetReviewsCount();
            }
            else if (!_currentFilter.IsEmpty() || _currentFilter.ForceFilter)
            {
                return (int)await _srsEntryDao.GetFilteredItemsCount(
                    ((SrsEntryFilter)_currentFilter).FilterClauses);
            }
            
            return 0;
        }

        #endregion
    }
}
