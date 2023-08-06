using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kanji.Database.Dao;
using Kanji.Database.Entities;
using Kanji.Interface.Models;

namespace Kanji.Interface.Business
{
    public class FilteredVocabIterator : FilteredItemIterator<VocabEntity>
    {
        #region Fields

        private VocabDao _vocabDao;

        #endregion

        #region Constructors

        /// <summary>
        /// Builds a filtered vocab list using the provided filter.
        /// </summary>
        /// <param name="filter">Vocab filter.</param>
        public FilteredVocabIterator(VocabFilter filter)
            : base(filter)
        {
            _vocabDao = new VocabDao();
        }

        #endregion

        #region Methods

        protected override IAsyncEnumerable<VocabEntity> DoFilter()
        {
            if (!Filter.IsEmpty())
            {
                VocabFilter filter = (VocabFilter)_currentFilter;

                return _vocabDao.GetFilteredVocab(
                    filter.Kanji.FirstOrDefault(), filter.Vocab,
                    filter.ReadingString, filter.MeaningString,
                    filter.Category, filter.JlptLevel, filter.WkLevel,
                    filter.IsCommonFirst, filter.IsShortReadingFirst);
            }
            return AsyncEnumerable.Empty<VocabEntity>();
        }

        protected override async Task<int> GetItemCount()
        {
            if (!Filter.IsEmpty())
            {
                VocabFilter filter = (VocabFilter)_currentFilter;

                return (int)await _vocabDao.GetFilteredVocabCount(filter.Kanji.FirstOrDefault(), filter.Vocab,
                    filter.ReadingString, filter.MeaningString,
                    filter.Category, filter.JlptLevel, filter.WkLevel);
            }

            return 0;
        }

        #endregion
    }
}
