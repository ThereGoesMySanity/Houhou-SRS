using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kanji.Interface.Models;
using Kanji.Database.Entities;
using Kanji.Interface.Business;
using Kanji.Database.Dao;
using Kanji.Interface.Extensions;

namespace Kanji.Interface.ViewModels
{
    class SrsTimingViewModel : ViewModel
    {
        #region Fields

        private ImportTimingMode _timingMode = ImportTimingMode.UseSrsLevel;
        private int _spreadAmountPerInterval = 20;
        private int _spreadInterval;
        private ImportSpreadTimingMode _spreadMode = ImportSpreadTimingMode.Random;
        private bool _kanjiOrdered = false;
        private DateTime? _fixedDate;
        protected Random _random;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the timing mode.
        /// </summary>
        public ImportTimingMode TimingMode
        {
            get { return _timingMode; }
            set
            {
                if (_timingMode != value)
                {
                    _timingMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the amount of new reviews per 24 hours slice when using Spread mode.
        /// </summary>
        public int SpreadAmountPerInterval
        {
            get { return _spreadAmountPerInterval; }
            set
            {
                if (_spreadAmountPerInterval != value)
                {
                    _spreadAmountPerInterval = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int SpreadInterval
        {
            get { return _spreadInterval; }
            set
            {
                if (_spreadInterval != value)
                {
                    _spreadInterval = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the behavior of the timing when using Spread mode.
        /// </summary>
        public ImportSpreadTimingMode SpreadMode
        {
            get { return _spreadMode; }
            set
            {
                if (_spreadMode != value)
                {
                    _spreadMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool KanjiOrdered
        {
            get { return _kanjiOrdered; }
            set
            {
                if (_kanjiOrdered != value)
                {
                    _kanjiOrdered = value;
                    RaisePropertyChanged();
                }
            }
        }


        /// <summary>
        /// Gets or sets the fixed date to use when the mode is set to Fixed.
        /// </summary>
        public DateTime? FixedDate
        {
            get { return _fixedDate; }
            set
            {
                if (_fixedDate != value)
                {
                    _fixedDate = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Constructors

        public SrsTimingViewModel()
        {
            _random = new Random();
            FixedDate = DateTime.Now;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Applies the timing to the given entries.
        /// </summary>
        /// <param name="entries">Entries to reschedule.</param>
        public void ApplyTiming(List<SrsEntry> entries)
        {
            if (TimingMode == ImportTimingMode.Spread)
            {
                int i = 0;
                TimeSpan delay = TimeSpan.Zero;
                List<SrsEntry> pickList = new List<SrsEntry>(entries);
                HashSet<char> countedKanji = entries.Select(e => e.AssociatedKanji)
                        .Where(k => !string.IsNullOrEmpty(k))
                        .Select(k => k[0]).ToHashSet();
                HashSet<char> kanjiAdded = new HashSet<char>();
                HashSet<char> kanjiJustAdded = new HashSet<char>();
                List<SrsEntry> kanjiToAdd = new List<SrsEntry>();
                List<SrsEntry> vocabToAdd = new List<SrsEntry>();
                List<SrsEntry> vocabNext = new List<SrsEntry>();
                KanjiDao kanjiDao = new KanjiDao();
                while (pickList.Any() || kanjiToAdd.Any() || vocabToAdd.Any() || vocabNext.Any())
                {
                    SrsEntry next = null;
                    if (kanjiToAdd.Any())
                    {
                        next = kanjiToAdd[0];
                        kanjiToAdd.RemoveAt(0);
                    }
                    else if (vocabToAdd.Any())
                    {
                        next = vocabToAdd[0];
                        vocabToAdd.RemoveAt(0);
                    }
                    else if (pickList.Any())
                    {
                        // Pick an item and remove it.
                        int nextIndex = SpreadMode == ImportSpreadTimingMode.ListOrder ? 0 : _random.Next(pickList.Count);
                        next = pickList[nextIndex];
                        pickList.RemoveAt(nextIndex);
                        if (KanjiOrdered)
                        {
                            if (!string.IsNullOrEmpty(next.AssociatedVocab))
                            {
                                bool skip = false;
                                foreach (var kanji in next.AssociatedVocab.Where(c => c > '\u4e00' && c < '\u9fff' && !kanjiAdded.Contains(c)))
                                {
                                    if (!countedKanji.Contains(kanji))
                                    {
                                        kanjiAdded.Add(kanji);
                                    }
                                    else 
                                    {
                                        if (kanjiToAdd.All(k => k.AssociatedKanji[0] != kanji))
                                        {
                                            int index = pickList.FindIndex(k => !string.IsNullOrEmpty(k.AssociatedKanji) && k.AssociatedKanji[0] == kanji);
                                            if (index >= 0)
                                            {
                                                kanjiToAdd.Add(pickList[index]);
                                                pickList.RemoveAt(index);
                                            }
                                        }
                                        skip = true;
                                    }
                                }
                                if (skip || next.AssociatedVocab.Any(c => c > '\u4e00' && c < '\u9fff' && kanjiJustAdded.Contains(c)))
                                {
                                    vocabNext.Add(next);
                                    continue;
                                }
                            }
                        }
                    }

                    if (next != null)
                    {
                        // add kanji to set
                        if (!string.IsNullOrEmpty(next.AssociatedKanji))
                        {
                            kanjiAdded.Add(next.AssociatedKanji[0]);
                            kanjiJustAdded.Add(next.AssociatedKanji[0]);
                        }

                        // Apply spread
                        next.NextAnswerDate = DateTime.Now + delay;
                    }

                    // Increment i and add a day to the delay if i reaches the spread value.
                    if (++i >= SpreadAmountPerInterval || next == null)
                    {
                        i = 0;
                        delay += TimeSpan.FromDays(SpreadInterval);

                        if (KanjiOrdered)
                        {
                            kanjiJustAdded.Clear();
                            // add vocab to queue once all kanji have been added
                            vocabToAdd.AddRange(vocabNext.Where(v => !v.AssociatedVocab.Any(c => c > '\u4e00' && c < '\u9fff' && !kanjiAdded.Contains(c))));
                            vocabNext.RemoveAll(v => !v.AssociatedVocab.Any(c => c > '\u4e00' && c < '\u9fff' && !kanjiAdded.Contains(c)));
                        }
                    }
                }
            }
            else if (TimingMode == ImportTimingMode.Immediate)
            {
                foreach (SrsEntry entry in entries)
                {
                    entry.NextAnswerDate = DateTime.Now;
                }
            }
            else if (TimingMode == ImportTimingMode.Never)
            {
                foreach (SrsEntry entry in entries)
                {
                    entry.NextAnswerDate = null;
                }
            }
            else if (TimingMode == ImportTimingMode.Fixed)
            {
                foreach (SrsEntry entry in entries)
                {
                    entry.NextAnswerDate = FixedDate;
                }
            }
            else if (TimingMode == ImportTimingMode.UseSrsLevel)
            {
                foreach (SrsEntry entry in entries)
                {
                    entry.NextAnswerDate = SrsLevelStore.Instance.GetNextReviewDate(entry.CurrentGrade);
                }
            }
        }

        #endregion
    }
}
