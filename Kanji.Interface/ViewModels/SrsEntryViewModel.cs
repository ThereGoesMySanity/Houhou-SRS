using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using Kanji.Common.Helpers;
using Kanji.Database.Dao;
using Kanji.Database.Entities;
using Kanji.Interface.Actors;
using Kanji.Interface.Business;
using Kanji.Interface.Internationalization;
using Kanji.Interface.Models;
using Kanji.Interface.Utilities;
using Kanji.Database.Helpers;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using Avalonia.Threading;

namespace Kanji.Interface.ViewModels
{
    public class SrsEntryViewModel : ViewModel
    {
        #region Internal enum

        private enum SrsEntryOperationEnum
        {
            Add = 0,
            Update = 1,
            Delete = 2
        }

        #endregion

        #region Fields

        private ExtendedSrsEntry _entry;

        private int _originalLevelValue;

        private DateTime? _originalNextReviewDate;

        private string _errorMessage;

        private bool _isSending;

        private bool _isDone;

        private SrsEntryDao _srsEntryDao;

        private KanjiDao _kanjiDao;

        private VocabDao _vocabDao;

        private KanjiEntity _associatedKanji;

        private VocabEntity _associatedVocab;

        private string _associatedKanjiString;

        private string _associatedVocabString;

        private bool _isEditingDate;

        private bool _isFirstSrsLevelSelect;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the SRS level picker ViewModel.
        /// </summary>
        public SrsLevelPickerViewModel SrsLevelPickerVm { get; private set; }

        /// <summary>
        /// Gets or sets the entry handled by this view model.
        /// </summary>
        public ExtendedSrsEntry Entry
        {
            get { return _entry; }
            set
            {
                if (_entry != value)
                {
                    _entry = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the associated kanji.
        /// Null if not loaded.
        /// </summary>
        public KanjiEntity AssociatedKanji
        {
            get { return _associatedKanji; }
            set
            {
                if (_associatedKanji != value)
                {
                    _associatedKanji = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the string that defines the associated
        /// kanji for the SRS entry.
        /// </summary>
        public string AssociatedKanjiString
        {
            get { return _associatedKanjiString; }
            set
            {
                if (_associatedKanjiString != value)
                {
                    _associatedKanjiString = value;
                    _entry.AssociatedKanji = value;
                    RaisePropertyChanged();
                    AssociatedKanji = null;
                    Dispatcher.UIThread.Post(async () => await GetAssociatedKanji(), DispatcherPriority.Background);
                }
            }
        }

        /// <summary>
        /// Gets or sets the associated vocab.
        /// Null if not loaded.
        /// </summary>
        public VocabEntity AssociatedVocab
        {
            get { return _associatedVocab; }
            set
            {
                if (_associatedVocab != value)
                {
                    _associatedVocab = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the string that defines the associated
        /// vocab for the SRS entry.
        /// </summary>
        public string AssociatedVocabString
        {
            get { return _associatedVocabString; }
            set
            {
                if (_associatedVocabString != value)
                {
                    _associatedVocabString = value;
                    _entry.AssociatedVocab = value;
                    RaisePropertyChanged();
                    AssociatedVocab = null;
                    Dispatcher.UIThread.Post(async () => await GetAssociatedVocab(), DispatcherPriority.Background);
                }
            }
        }

        /// <summary>
        /// Gets or sets the data validation error message.
        /// </summary>
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets a boolean value indicating if the entry data
        /// is being sent.
        /// </summary>
        public bool IsSending
        {
            get { return _isSending; }
            set
            {
                if (_isSending != value)
                {
                    _isSending = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if the SrsEntry
        /// handled by this ViewModel is being added (True)
        /// or modified (False).
        /// </summary>
        public bool IsNew
        {
            get { return Entry.Reference.ID <= 0; }
        }

        /// <summary>
        /// Gets or sets a value indicating if the next review date is being edited.
        /// </summary>
        public bool IsEditingDate
        {
            get { return _isEditingDate; }
            set
            {
                if (_isEditingDate != value)
                {
                    _isEditingDate = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Events

        public delegate void FinishedEditingHandler(object sender, SrsEntryEditedEventArgs e);
        /// <summary>
        /// Triggered when the entry edition is done.
        /// </summary>
        public event FinishedEditingHandler FinishedEditing;

        #endregion

        #region Commands

        /// <summary>
        /// Gets or sets the command used to submit the entry.
        /// </summary>
        public RelayCommand SubmitCommand { get; set; }

        /// <summary>
        /// Gets or sets the command used to cancel the SRS entry editing.
        /// </summary>
        public RelayCommand CancelCommand { get; set; }

        /// <summary>
        /// Gets or sets the command used to reset the entry progress.
        /// </summary>
        public RelayCommand SrsProgressResetCommand { get; set; }

        /// <summary>
        /// Gets or sets the command used to update the entry to match the
        /// associated kanji.
        /// </summary>
        public RelayCommand ApplyAssociatedKanjiCommand { get; set; }

        /// <summary>
        /// Gets or sets the command used to update the entry to match the
        /// associated vocab.
        /// </summary>
        public RelayCommand ApplyAssociatedVocabCommand { get; set; }

        /// <summary>
        /// Gets or sets the command used to toggle the suspension state
        /// of the SRS entry.
        /// </summary>
        public RelayCommand ToggleSuspendCommand { get; set; }

        /// <summary>
        /// Gets or sets the command used to delete the SRS entry.
        /// </summary>
        public RelayCommand DeleteCommand { get; set; }

        /// <summary>
        /// Gets the command used to toggle next review date edition.
        /// </summary>
        public RelayCommand ToggleDateEditCommand { get; private set; }

        /// <summary>
        /// Gets the command used to set the next review date to Now.
        /// </summary>
        public RelayCommand DateToNowCommand { get; private set; }

        /// <summary>
        /// Gets the command used to set the next review date to Never.
        /// </summary>
        public RelayCommand DateToNeverCommand { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Builds a ViewModel aimed at creating a new SrsEntry.
        /// </summary>
        public SrsEntryViewModel()
            : this(new SrsEntry())
        {
            
        }

        /// <summary>
        /// Builds a ViewModel aimed at editing an existing SrsEntry,
        /// or adding a pre-composed SrsEntry.
        /// </summary>
        /// <param name="entity">Entity to edit.</param>
        public SrsEntryViewModel(SrsEntry entity)
        {
            // Initialize fields.
            _entry = new ExtendedSrsEntry(entity);
            _originalNextReviewDate = entity.NextAnswerDate;
            _originalLevelValue = entity.CurrentGrade;
            _associatedKanjiString = Entry.AssociatedKanji;
            _associatedVocabString = Entry.AssociatedVocab;
            _srsEntryDao = new SrsEntryDao();
            _kanjiDao = new KanjiDao();
            _vocabDao = new VocabDao();
            if (IsNew)
            {
                Entry.Tags = Properties.UserSettings.Instance.LastSrsTagsValue;
            }

            // Create the relay commands.
            SubmitCommand = new RelayCommand(OnSubmit);
            CancelCommand = new RelayCommand(OnCancel);
            SrsProgressResetCommand = new RelayCommand(OnSrsProgressReset);
            ApplyAssociatedKanjiCommand = new RelayCommand(OnApplyAssociatedKanji);
            ApplyAssociatedVocabCommand = new RelayCommand(OnApplyAssociatedVocab);
            ToggleSuspendCommand = new RelayCommand(OnToggleSuspend);
            DeleteCommand = new RelayCommand(OnDelete);
            ToggleDateEditCommand = new RelayCommand(OnToggleDateEdit);
            DateToNowCommand = new RelayCommand(OnDateToNow);
            DateToNeverCommand = new RelayCommand(OnDateToNever);

            // Get the associated kanji or vocab.
            Dispatcher.UIThread.Post(async () => {await GetAssociatedKanji(); await GetAssociatedVocab();},
                DispatcherPriority.Background);
            // Initialize the VM.
            _isFirstSrsLevelSelect = true;
            SrsLevelPickerVm = new SrsLevelPickerViewModel();
            SrsLevelPickerVm.SrsLevelSelected += OnSrsLevelSelected;
            SrsLevelPickerVm.Initialize(_entry.CurrentGrade);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Validates the current SrsEntry entity.
        /// Also values the ErrorMessage property.
        /// </summary>
        /// <returns>True if the SrsEntry is valid.
        /// False otherwise.</returns>
        private bool ValidateEntity()
        {
            string errorMessage = string.Empty;
            bool error = false;

            // Meanings AND readings cannot be null.
            if (string.IsNullOrWhiteSpace(_entry.Meanings) && string.IsNullOrWhiteSpace(_entry.Readings))
            {
                errorMessage += "The item must have at least one meaning or reading.";
                error = true;
            }

            ErrorMessage = errorMessage;
            return !error;
        }

        #region Background tasks

        #region SendEntity

        /// <summary>
        /// Starts a background task that will update, add or delete the
        /// SrsItem according to the given operation type.
        /// </summary>
        /// <param name="operationType">Operation to execute on the SRS
        /// item.</param>
        private async Task SendEntity(SrsEntryOperationEnum operationType)
        {
            if (!_isDone)
            {
                // The work has still not been done.
                // Set the IsSending value and start the job.
                IsSending = true;

                if (operationType == SrsEntryOperationEnum.Update)
                {
                    // Update
                    try
                    {
                        _isDone = await _srsEntryDao.Update(_entry.Reference);
                        if (!_isDone)
                        {
                            ErrorMessage = R.SrsItem_EditFailure;
                        }
                    }
                    catch (Exception ex)
                    {
                        // An exception occured.
                        // Log the exception and set the error message.
                        LogHelper.GetLogger(this.GetType().Name)
                            .Error("Could not update the entity.", ex);
                        ErrorMessage = R.SrsItem_EditFailure;
                    }
                }
                else if (operationType == SrsEntryOperationEnum.Add)
                {
                    // Add
                    try
                    {
                        // Sets some properties
                        Entry.Reference.CreationDate = DateTime.UtcNow;

                        // Add the entity to the database.
                        await _srsEntryDao.Add(_entry.Reference);
                        _isDone = true;

                        // Saves the value of the tags
                        Properties.UserSettings.Instance.LastSrsTagsValue = Entry.Tags;
                    }
                    catch (Exception ex)
                    {
                        // An exception occured.
                        // Log the exception and set the error message.
                        LogHelper.GetLogger(this.GetType().Name)
                            .Error("Could not add the entity.", ex);
                        ErrorMessage = R.SrsItem_EditFailure;
                    }
                }
                else
                {
                    // Delete
                    try
                    {
                        _isDone = await _srsEntryDao.Delete(_entry.Reference);
                        if (!_isDone)
                        {
                            ErrorMessage = R.SrsItem_EditFailure;
                        }
                    }
                    catch (Exception ex)
                    {
                        // An exception occured.
                        // Log the exception and set the error message.
                        LogHelper.GetLogger(this.GetType().Name)
                            .Error("Could not delete the entity.", ex);
                        ErrorMessage = R.SrsItem_EditFailure;
                    }
                }

                // After the job.
                if (_isDone && FinishedEditing != null)
                {
                    // If the job has been done, raise the event.
                    ExtendedSrsEntry result =
                        (operationType == SrsEntryOperationEnum.Delete ?
                        null : Entry);

                    FinishedEditing(this, new SrsEntryEditedEventArgs(result, true));
                }

                IsSending = false;
            }
        }

        #endregion

        #region GetAssociatedKanji

        /// <summary>
        /// Starts a background task that will obtain the associated
        /// kanji property.
        /// </summary>
        private async Task GetAssociatedKanji()
        {
            if (!string.IsNullOrWhiteSpace(AssociatedKanjiString))
            {
                AssociatedKanji = await _kanjiDao.GetFirstMatchingKanji(AssociatedKanjiString);
            }
        }

        #endregion

        #region GetAssociatedVocab

        /// <summary>
        /// Starts a background task that will obtain the associated
        /// vocab property.
        /// </summary>
        private async Task GetAssociatedVocab()
        {
            if (!string.IsNullOrWhiteSpace(AssociatedVocabString))
            {
                IAsyncEnumerable<VocabEntity> results = _vocabDao.GetMatchingVocab(AssociatedVocabString);
                if (await results.AnyAsync())
                {
                    VocabEntity entity = new VocabEntity();
                    StringBuilder meanings = new StringBuilder();
                    StringBuilder readings = new StringBuilder();
                    await foreach (VocabEntity result in results)
                    {
                        foreach (VocabMeaning meaning in result.Meanings)
                        {
                            meanings.Append(MultiValueFieldHelper.ReplaceSeparator(meaning.Meaning)
                                .Replace(';', MultiValueFieldHelper.ValueSeparator)
                                + MultiValueFieldHelper.ValueSeparator);
                        }
                        readings.Append(result.KanaWriting + MultiValueFieldHelper.ValueSeparator);
                    }
                    entity.Meanings.Add(new VocabMeaning() { Meaning = MultiValueFieldHelper.Expand(MultiValueFieldHelper.Distinct(meanings.ToString())) });
                    entity.KanaWriting = MultiValueFieldHelper.Expand(MultiValueFieldHelper.Distinct(readings.ToString()));
                    AssociatedVocab = entity;
                }
                else
                {
                    AssociatedVocab = null;
                }
            }
        }

        #endregion

        #endregion

        #region Command callbacks

        /// <summary>
        /// Command callback.
        /// Called when the entity is submitted.
        /// </summary>
        private async void OnSubmit()
        {
            // Validate and send.
            if (ValidateEntity())
            {
                await SendEntity(IsNew ? SrsEntryOperationEnum.Add :
                    SrsEntryOperationEnum.Update);
            }
        }

        /// <summary>
        /// Command callback.
        /// Called to cancel edition.
        /// </summary>
        private void OnCancel()
        {
            if (FinishedEditing != null)
            {
                FinishedEditing(this, new SrsEntryEditedEventArgs(Entry, false));
            }
        }

        /// <summary>
        /// Command callback.
        /// Called to reset the entity progress.
        /// </summary>
        private void OnSrsProgressReset()
        {
            Entry.FailureCount = 0;
            Entry.SuccessCount = 0;
            SrsLevelPickerVm.SelectLevel(0);
        }

        /// <summary>
        /// Command callback.
        /// Called to update the entry according to the associated kanji.
        /// </summary>
        private void OnApplyAssociatedKanji()
        {
            Entry.LoadFromKanji(AssociatedKanji);
        }

        /// <summary>
        /// Command callback.
        /// Called to update the entry according to the associated vocab.
        /// </summary>
        private void OnApplyAssociatedVocab()
        {
            Entry.Meanings = AssociatedVocab.Meanings.First().Meaning;
            Entry.Readings = AssociatedVocab.KanaWriting;
        }

        /// <summary>
        /// Command callback.
        /// Called to toggle the suspension state of the entry.
        /// </summary>
        private void OnToggleSuspend()
        {
            if (Entry.SuspensionDate.HasValue)
            {
                Entry.SuspensionDate = null;
            }
            else
            {
                Entry.SuspensionDate = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Command callback.
        /// Called to delete the SRS entry.
        /// </summary>
        private async void OnDelete()
        {
            var result = await MessageBoxActor.Instance.ShowMessageBox(new MessageBoxStandardParams
            {
                ContentTitle = "Delete the SRS item",
                ContentMessage = "Do you really want to delete the SRS item?",
                ButtonDefinitions = MsBox.Avalonia.Enums.ButtonEnum.YesNo,
            });
            if (result == ButtonResult.Yes)
            {
                await SendEntity(SrsEntryOperationEnum.Delete);
            }
        }

        /// <summary>
        /// Command callback.
        /// Called to toggle next review date edit.
        /// </summary>
        private void OnToggleDateEdit()
        {
            IsEditingDate = !IsEditingDate;
        }

        /// <summary>
        /// Command callback.
        /// Called to set the next review date to Now.
        /// </summary>
        private void OnDateToNow()
        {
            Entry.NextAnswerDate = DateTime.Now;
        }

        /// <summary>
        /// Command callback.
        /// Called to set the next review date to Never.
        /// </summary>
        private void OnDateToNever()
        {
            Entry.NextAnswerDate = null;
        }

        #endregion

        #region Event callbacks

        /// <summary>
        /// Event callback.
        /// Called when a SRS level is selected from the level picker.
        /// </summary>
        private void OnSrsLevelSelected(object sender, SrsLevelSelectedEventArgs e)
        {
            if (e.SelectedLevel != null)
            {
                Entry.CurrentGrade = (short)e.SelectedLevel.Value;

                if ((e.SelectedLevel.Value == _originalLevelValue
                    && _originalNextReviewDate.HasValue)
                    || (_isFirstSrsLevelSelect && !IsNew))
                {
                    // Reset the original Next Answer Date.
                    Entry.NextAnswerDate = _originalNextReviewDate;
                    _isFirstSrsLevelSelect = false;
                }
                else
                {
                    if (e.SelectedLevel.Delay.HasValue)
                    {
                        Entry.NextAnswerDate = DateTime.Now + e.SelectedLevel.Delay.Value;
                    }
                    else
                    {
                        Entry.NextAnswerDate = null;
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Disposes the resources used by this object.
        /// </summary>
        public override void Dispose()
        {
            SrsLevelPickerVm.SrsLevelSelected -= OnSrsLevelSelected;
            SrsLevelPickerVm.Dispose();
            base.Dispose();
        }

        #endregion
    }
}
