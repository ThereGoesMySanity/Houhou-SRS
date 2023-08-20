using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Kanji.Interface.Models;

namespace Kanji.Interface.Business
{
    public abstract class FilteredItemIterator<T>
    {
        #region Fields

        private IAsyncEnumerable<T> _itemSet;
        private IAsyncEnumerator<T> _iterator;

        /// <summary>
        /// Last filter applied or filter being applied.
        /// </summary>
        protected Filter<T> _currentFilter;

        private CancellationTokenSource _iteratorCancellation = new CancellationTokenSource();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the filter associated to the item iterator.
        /// </summary>
        public Filter<T> Filter { get; private set; }

        /// <summary>
        /// Gets the total number of items in the list.
        /// </summary>
        public int ItemCount { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Builds a filtered item list using the provided filter.
        /// </summary>
        /// <param name="filter">Item filter.</param>
        public FilteredItemIterator(Filter<T> filter)
        {
            Filter = filter;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Changes the filter. This will trigger a subsequent filter operation,
        /// and as the filtered item list will be changed, the iterator will be
        /// reset as well.
        /// </summary>
        /// <param name="newFilter">New filter to apply.</param>
        public void SetFilter(Filter<T> newFilter)
        {
            if (Filter != newFilter)
            {
                Filter = newFilter;
                Dispatcher.UIThread.Post(async () => await ApplyFilter(), DispatcherPriority.Background);
            }
        }

        /// <summary>
        /// Iterates over the filtered item set and returns the given amount of results,
        /// starting from the element following the last one returned by this method.
        /// </summary>
        /// <param name="count">Max number of items to return.</param>
        /// <returns>A list of items containing 0 to <paramref name="count"/> elements.
        /// If the list contains less than <paramref name="count"/> elements, the set
        /// has been iterated until the end.</returns>
        public IAsyncEnumerable<T> GetNext(int count)
        {
            return _itemSet.TakeWhile(t => --count >= 0 && !_iteratorCancellation.IsCancellationRequested);
        }

        /// <summary>
        /// Applies the filter and sets the iterator.
        /// </summary>
        public async Task ApplyFilter()
        {
            // Clone the filter.
            _currentFilter = Filter.Clone();

            // Apply the filter.
            _itemSet = DoFilter();

            // Get the total item count.
            ItemCount = await GetItemCount();
        }

        /// <summary>
        /// In child classes, implements the filter application and returns the
        /// resulting set.
        /// </summary>
        protected abstract IAsyncEnumerable<T> DoFilter();

        /// <summary>
        /// In child classes, returns the total number of items to be iterated.
        /// </summary>
        protected abstract Task<int> GetItemCount();

        #endregion
    }
}
