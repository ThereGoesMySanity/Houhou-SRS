using System;
using System.Linq;
using System.ComponentModel;

using GalaSoft.MvvmLight.Command;
using Kanji.Database.Entities;
using Kanji.Interface.Models;
using Kanji.Interface.Views;
using Kanji.Interface.Extensions;
using Kanji.Interface.Helpers;
using Kanji.Database.Dao;
using Kanji.Common.Helpers;

using System.Text.RegularExpressions;

using Avalonia.Threading;
using Avalonia;
using Kanji.Interface.Actors;
using Avalonia.Input;
using Avalonia.Svg.Skia;
using SvgImage = Avalonia.Svg.Skia.SvgImage;
using System.Threading.Tasks;
using Svg.Skia;

namespace Kanji.Interface.ViewModels
{
    public class KanjiDetailsViewModel : ViewModel
    {
        #region Constants

        private static readonly int StrokeSquareWidth = 109;

        #endregion

        #region Fields

        private ExtendedKanji _kanjiEntity;

        private static bool _showDetails = true;

        private ExtendedSrsEntry _srsEntry;

        private object _updateLock = new object();

        private SvgImage _strokesImage;

        private Rect _strokesRect;

        private int _strokesCount;

        private int _currentStroke;

        private DispatcherTimer _strokeUpdateTimer;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the category filter view model.
        /// </summary>
        public CategoryFilterViewModel CategoryFilterVm { get; private set; }

        /// <summary>
        /// Gets the vocab list view model.
        /// </summary>
        public VocabListViewModel VocabListVm { get; private set; }

        /// <summary>
        /// Gets the vocab filter view model.
        /// </summary>
        public VocabFilterViewModel VocabFilterVm { get; private set; }

        /// <summary>
        /// Gets or sets the kanji entity stored in the view model.
        /// </summary>
        public ExtendedKanji KanjiEntity
        {
            get { return _kanjiEntity; }
            set
            {
                if (_kanjiEntity != value)
                {
                    _kanjiEntity = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets a boolean value indicating if all details
        /// concerning the kanji should be shown.
        /// </summary>
        public bool ShowDetails
        {
            get { return _showDetails; }
            set
            {
                if (value != _showDetails)
                {
                    _showDetails = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the SRS entry associated with the kanji
        /// handled by this ViewModel.
        /// </summary>
        public ExtendedSrsEntry SrsEntry
        {
            get { return _srsEntry; }
            set
            {
                if (_srsEntry != value)
                {
                    _srsEntry = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the drawing group that represents the
        /// strokes diagram of the kanji.
        /// </summary>
        public SvgImage StrokesImage
        {
            get { return _strokesImage; }
            set
            {
                if (_strokesImage != value)
                {
                    _strokesImage = value;
                    RaisePropertyChanged();
                }
            }
        }

        public Rect StrokesRect
        {
            get { return _strokesRect; }
            set
            {
                if (_strokesRect != value)
                {
                    _strokesRect = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the number of strokes.
        /// </summary>
        public int StrokesCount
        {
            get { return _strokesCount; }
            set
            {
                if (_strokesCount != value)
                {
                    _strokesCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the current 1-based index of the stroke
        /// displayed by the StrokesDrawingGroup.
        /// </summary>
        public int CurrentStroke
        {
            get { return _currentStroke; }
            set
            {
                if (_currentStroke != value)
                {
                    _currentStroke = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Gets the command used to show/hide details.
        /// </summary>
        public RelayCommand ToggleDetailsCommand { get; private set; }

        /// <summary>
        /// Gets the command used to open the SRS entry edition window
        /// to add the kanji to the SRS.
        /// </summary>
        public RelayCommand AddToSrsCommand { get; private set; }

        /// <summary>
        /// Gets the command used to open the SRS entry edition window
        /// to edit the entry related to the kanji handled by this ViewModel.
        /// </summary>
        public RelayCommand EditSrsEntryCommand { get; private set; }

        /// <summary>
        /// Gets the command used to filter the vocab of the kanji represented in this view model by the given reading.
        /// </summary>
        public RelayCommand<string> FilterReadingCommand { get; private set; }

        /// <summary>
        /// Gets the command used to advance to the next stroke.
        /// </summary>
        public RelayCommand<PointerReleasedEventArgs> NextStrokeCommand { get; private set; }

        /// <summary>
        /// Gets the command used to advance to the last stroke.
        /// </summary>
        public RelayCommand LastStrokeCommand { get; private set; }

        /// <summary>
        /// Gets the command used to go back to the first stroke.
        /// </summary>
        public RelayCommand FirstStrokeCommand { get; private set; }

        /// <summary>
        /// Gets the command used to access the kanji on WaniKani.
        /// </summary>
        public RelayCommand WaniKaniCommand { get; private set; }

        #endregion

        #region Events

        public delegate void KanjiNavigatedHandler(object sender, KanjiNavigatedEventArgs e);
        /// <summary>
        /// Event triggered when a kanji has been selected in a vocab.
        /// </summary>
        public event KanjiNavigatedHandler KanjiNavigated;

        #endregion

        #region Constructors

        /// <summary>
        /// Builds a kanji details ViewModel handling the given kanji.
        /// </summary>
        /// <param name="kanjiEntity">Kanji to handle.</param>
        public KanjiDetailsViewModel(ExtendedKanji kanjiEntity)
        {
            KanjiEntity = kanjiEntity;

            if (KanjiEntity.DbKanji.SrsEntries.Any())
            {
                SrsEntry = new ExtendedSrsEntry(
                    KanjiEntity.DbKanji.SrsEntries.First());
            }

            VocabFilter filter = new VocabFilter() {
                Kanji = new KanjiEntity[] { _kanjiEntity.DbKanji } };

            CategoryFilterVm = new CategoryFilterViewModel();
            CategoryFilterVm.PropertyChanged += OnCategoryChanged;

            VocabListVm = new VocabListViewModel(filter);
            VocabListVm.KanjiNavigated += OnKanjiNavigated;
            VocabFilterVm = new VocabFilterViewModel(filter);
            VocabFilterVm.FilterChanged += OnVocabFilterChanged;
            VocabFilterVm.PropertyChanged += OnVocabPropertyChanged;

            ToggleDetailsCommand = new RelayCommand(OnToggleDetails);
            AddToSrsCommand = new RelayCommand(OnAddToSrs);
            EditSrsEntryCommand = new RelayCommand(OnEditSrsEntry);
            FilterReadingCommand = new RelayCommand<string>(OnFilterReading);

            NextStrokeCommand = new RelayCommand<PointerReleasedEventArgs>(OnNextStroke);
            LastStrokeCommand = new RelayCommand(OnLastStroke);
            FirstStrokeCommand = new RelayCommand(OnFirstStroke);
            WaniKaniCommand = new RelayCommand(OnWaniKani);

            PrepareSvg();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Advances to the next stroke.
        /// </summary>
        private void GoToNextStroke()
        {
            int next = CurrentStroke + 1;
            if (next > StrokesCount)
            {
                next = 1;
            }

            SetCurrentStroke(next);
        }

        /// <summary>
        /// Sets the current stroke to the given value.
        /// </summary>
        /// <param name="value">New value for the current stroke.</param>
        private void SetCurrentStroke(int value)
        {
            // Careful: CurrentStroke starts at 1, not 0.
            CurrentStroke = value;

            if (StrokesImage != null)
            {
                int startX = (CurrentStroke - 1) * StrokeSquareWidth;
                StrokesRect = new Rect(-startX, 0, StrokeSquareWidth, StrokesImage.Size.Height);
            }
        }

        #region PrepareSvg

        /// <summary>
        /// Starts a background task to retrieve and prepare the SVG of the kanji.
        /// </summary>
        private void PrepareSvg()
        {
            // Run the task in the background.
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += DoPrepareSvg;
            worker.RunWorkerCompleted += DonePrepareSvg;
            worker.RunWorkerAsync();
        }

        /// <summary>
        /// Background task work method.
        /// </summary>
        private async void DoPrepareSvg(object sender, DoWorkEventArgs e)
        {
            KanjiDao dao = new KanjiDao();
            // Get the kanji strokes.
            KanjiStrokes strokes = await dao.GetKanjiStrokes(_kanjiEntity.DbKanji.ID);
            // Lock to allow only one of these operations at a time.
            lock (_updateLock)
            {
                if (strokes != null && strokes.FramesSvg.Length > 0)
                {
                    // If the strokes was successfuly retrieved, we have to read the compressed SVG contained inside.
                    string svgString = StringCompressionHelper.Unzip(strokes.FramesSvg);
                    StrokesCount = Regex.Matches(svgString, "<circle").Count;
                    DispatcherHelper.Invoke(() => 
                        {
                            StrokesImage = new SvgImage
                            {
                                Source = SvgSource.LoadFromSvg(svgString)
                            };
                            SetCurrentStroke(1);
                        });
                }
            }
        }

        /// <summary>
        /// Background task completed method. Unsubscribes to the events.
        /// </summary>
        private void DonePrepareSvg(object sender, RunWorkerCompletedEventArgs e)
        {
            ((BackgroundWorker)sender).DoWork -= DoPrepareSvg;
            ((BackgroundWorker)sender).RunWorkerCompleted -= DonePrepareSvg;

            if (Properties.UserSettings.Instance.AnimateStrokes)
            {
                _strokeUpdateTimer = new DispatcherTimer();
                _strokeUpdateTimer.Interval = TimeSpan.FromMilliseconds(Properties.UserSettings.Instance.StrokeAnimationDelay);
                _strokeUpdateTimer.Tick += OnStrokeUpdateTimerTick;
                _strokeUpdateTimer.Start();
            }
        }

        #endregion

        #region Command callbacks

        /// <summary>
        /// Called when the ToggleDetailsCommand is fired.
        /// Toggles the ShowDetails boolean.
        /// </summary>
        private void OnToggleDetails()
        {
            ShowDetails = !ShowDetails;
        }

        /// <summary>
        /// Called when the AddToSrsCommand is fired.
        /// Opens the SRS entry window.
        /// </summary>
        private async void OnAddToSrs()
        {
            // Prepare the new entry.
            SrsEntry entry = new SrsEntry();
            entry.LoadFromKanji(_kanjiEntity.DbKanji);

            SrsEntryEditedEventArgs e = await NavigationActor.Instance.OpenSrsEditWindow(entry);
            if (e.IsSaved && e.SrsEntry != null
                && e.SrsEntry.AssociatedKanji == _kanjiEntity.DbKanji.Character)
            {
                // The result exists and is still associated with this kanji.
                // We can use it in this ViewModel.
                SrsEntry = e.SrsEntry;
            }
        }

        /// <summary>
        /// Called when the EditSrsEntryCommand is fired.
        /// Opens the SRS entry edition window.
        /// </summary>
        private async void OnEditSrsEntry()
        {
            if (SrsEntry != null)
            {
                // Show the modal entry edition window.
                SrsEntryEditedEventArgs e = await NavigationActor.Instance.OpenSrsEditWindow(SrsEntry.Reference);
                if (e.IsSaved)
                {
                    if (e.SrsEntry != null &&
                        e.SrsEntry.AssociatedKanji == _kanjiEntity.DbKanji.Character)
                    {
                        // The result exists and is still associated with this kanji.
                        // We can use it in this ViewModel.
                        SrsEntry = e.SrsEntry;
                    }
                    else
                    {
                        // The result has been saved but is no longer associated with
                        // this kanji. Set the value to null.
                        SrsEntry = null;
                    }
                }
            }
        }

        /// <summary>
        /// Called when the FilterReadingCommand is fired.
        /// Filters the vocab of the kanji by the specified reading.
        /// </summary>
        /// <param name="reading">Reading to use as a filter.</param>
        private void OnFilterReading(string reading)
        {
            // This command is not fired by typing, but rather from events like
            // clicking on a reading on the kanji page.
            // In those events, we *do* want to automatically re-apply the filter.
            string newReading = reading.Replace("ー", string.Empty).Replace(".", string.Empty);
            if (VocabFilterVm.ReadingFilter == newReading)
                return;
            VocabFilterVm.ReadingFilter = newReading;
            VocabListVm.ReapplyFilter();
        }

        /// <summary>
        /// Called when the NextStrokeCommand is fired.
        /// Advances to the next stroke. Stops the timer if it was active.
        /// </summary>
        private void OnNextStroke(PointerReleasedEventArgs args)
        {
            if ((args.InitialPressMouseButton == MouseButton.Left || args.InitialPressMouseButton == MouseButton.Right)
                &&_strokeUpdateTimer != null)
            {
                _strokeUpdateTimer.Stop();
            }

            switch(args.InitialPressMouseButton)
            {
                case MouseButton.Left:
                    GoToNextStroke();
                    break;
                case MouseButton.Right:
                    int next = CurrentStroke - 1;
                    if (next <= 0)
                    {
                        next = StrokesCount;
                    }
                    SetCurrentStroke(next);
                    break;
            }
        }

        /// <summary>
        /// Called when the LastStrokeCommand is fired.
        /// Advances to the last stroke. Stops the timer if it was active.
        /// </summary>
        private void OnLastStroke()
        {
            if (_strokeUpdateTimer != null)
            {
                _strokeUpdateTimer.Stop();
            }

            SetCurrentStroke(StrokesCount);
        }

        /// <summary>
        /// Called when the FirstStrokeCommand is fired.
        /// Goes back to the first stroke. Stops the timer if it was active.
        /// </summary>
        private void OnFirstStroke()
        {
            if (_strokeUpdateTimer != null)
            {
                _strokeUpdateTimer.Stop();
            }

            SetCurrentStroke(1);
        }

        /// <summary>
        /// Called when the WaniKaniCommand is fired.
        /// Navigates to the URL of the kanji on WaniKani.
        /// </summary>
        private void OnWaniKani()
        {
            ProcessHelper.OpenUri("https://www.wanikani.com/kanji/" + _kanjiEntity.DbKanji.Character);
        }

        #endregion

        #region Event callbacks

        /// <summary>
        /// Occurs when a kanji from the vocab list is selected.
        /// Forwards the kanji navigation event if the kanji is different
        /// from the one detailed in this ViewModel.
        /// </summary>
        private void OnKanjiNavigated(object sender, KanjiNavigatedEventArgs e)
        {
            // Check that the kanji is different from the one attached to this details VM.
            if (KanjiNavigated != null &&
                e.Character.Kanji.ID != _kanjiEntity.DbKanji.ID)
            {
                // If different, forward the event.
                KanjiNavigated(sender, e);
            }
        }
        
        /// <summary>
        /// Event callback.
        /// Called when the vocab category changes.
        /// Refreshes the vocab list.
        /// </summary>
        private void OnCategoryChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CategoryFilter")
                VocabListVm.Category = CategoryFilterVm.CategoryFilter;
        }

        /// <summary>
        /// Event callback.
        /// Called when the vocab filter changes.
        /// Refreshes the vocab list.
        /// </summary>
        private void OnVocabFilterChanged(object sender, EventArgs e)
        {
            VocabListVm.ReapplyFilter();
        }

        /// <summary>
        /// Event callback.
        /// Called when a vocab property changes.
        /// Refreshes the vocab list.
        /// </summary>
        private void OnVocabPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //if (e.PropertyName == "ReadingFilter")
            //    VocabListVm.ReapplyFilter();
        }

        /// <summary>
        /// Event callback.
        /// Called when the stroke update timer ticks.
        /// Goes to the next stroke.
        /// </summary>
        private void OnStrokeUpdateTimerTick(object sender, EventArgs e)
        {
            GoToNextStroke();
        }

        #endregion

        /// <summary>
        /// Disposes resources used by this object.
        /// </summary>
        public override void Dispose()
        {
            VocabListVm.KanjiNavigated -= OnKanjiNavigated;
            KanjiNavigated = null;
            VocabListVm.Dispose();
            VocabFilterVm.FilterChanged -= OnVocabFilterChanged;
            VocabFilterVm.Dispose();

            if (_strokeUpdateTimer != null)
            {
                _strokeUpdateTimer.Tick -= OnStrokeUpdateTimerTick;
            }

            base.Dispose();
        }

        #endregion
    }
}
