﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using Kanji.Common.Helpers;
using Kanji.Database.Dao;
using Kanji.Database.Entities;
using Kanji.Database.Helpers;
using Kanji.Interface.Business;
using Kanji.Interface.Models;
using Kanji.Interface.Actors;
using Avalonia.Threading;
using Kanji.Interface.Helpers;
using Microsoft.Extensions.Logging;

namespace Kanji.Interface.ViewModels
{
    public class SrsReviewViewModel : ViewModel
    {
        #region Constants

        private static readonly int BatchMaxSize = 10;

        private static readonly int[] MeaningDistanceLenience =
            [0, 4, 7, 12, 20];

        /// <summary>
        /// Delay before a submission can be issued after the state was set to
        /// the failure state.
        /// </summary>
        private static readonly TimeSpan FailureSubmitDelay = TimeSpan.FromMilliseconds(1000);

        #endregion

        #region Fields

        private Random _random;

        private FilteredSrsEntryIterator _iterator;

        private SrsEntryDao _srsEntryDao;

        /// <summary>
        /// Batch capped to a max size containing the
        /// current questions that can be answered.
        /// </summary>
        private List<SrsQuestionGroup> _currentBatch;

        /// <summary>
        /// Boolean defining if a submit operation can
        /// be processed.
        /// Used to block submission during the failure submit
        /// delay.
        /// </summary>
        private bool _canSubmit;

        /// <summary>
        /// Value indicating if there are still results
        /// to come from the iterator.
        /// False when all results have been fetched.
        /// </summary>
        private bool _isEntryAvailable;

        private bool _isReviewing;

        private bool _isWrappingUp;

        private SrsQuestion _currentQuestion;

        private SrsReviewStateEnum _reviewState;

        private string _currentAnswer;

        private int _totalReviewsCount;

        private int _answeredReviewsCount;

        private SrsLevel _previewNextLevel;

        private object _batchLock = new object();

        private object _submitLock = new object();

        private object _stopLock = new object();

        private int _currentTransactionCount;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a boolean indicating if the
        /// review is over.
        /// </summary>
        public bool IsReviewing
        {
            get { return _isReviewing; }
            set
            {
                if (_isReviewing != value)
                {
                    _isReviewing = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets a boolean indicating if the
        /// review session is being wrapped up.
        /// </summary>
        public bool IsWrappingUp
        {
            get { return _isWrappingUp; }
            set
            {
                if (_isWrappingUp != value)
                {
                    _isWrappingUp = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the current review state.
        /// </summary>
        public SrsReviewStateEnum ReviewState
        {
            get { return _reviewState; }
            set
            {
                if (value != _reviewState)
                {
                    _reviewState = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the current answer.
        /// </summary>
        public string CurrentAnswer
        {
            get { return _currentAnswer; }
            set
            {
                if (_currentAnswer != value)
                {
                    if (CurrentQuestion != null
                        && CurrentQuestion.Question == SrsQuestionEnum.Reading)
                    {
                        //Fixed: https://github.com/AvaloniaUI/Avalonia/issues/3603
                        _currentAnswer = KanaHelper.RomajiToKana(value, true);
                    }
                    else
                    {
                        _currentAnswer = value;
                    }

                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the current question.
        /// </summary>
        public SrsQuestion CurrentQuestion
        {
            get { return _currentQuestion; }
            set
            {
                if (value != _currentQuestion)
                {
                    _currentQuestion = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the parent question group of the current question.
        /// (Shortcut provided for convenience and clarity)
        /// </summary>
        public SrsQuestionGroup CurrentQuestionGroup
        {
            get { return _currentQuestion == null ? null : _currentQuestion.ParentGroup; }
        }

        /// <summary>
        /// Gets the total number of reviews for this session.
        /// </summary>
        public int TotalReviewsCount
        {
            get { return _totalReviewsCount; }
            set
            {
                if (_totalReviewsCount != value)
                {
                    _totalReviewsCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the number of answered reviews for this session.
        /// </summary>
        public int AnsweredReviewsCount
        {
            get { return _answeredReviewsCount; }
            set
            {
                if (_answeredReviewsCount != value)
                {
                    _answeredReviewsCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the preview of the level that will be assigned
        /// to the SRS entry referred by the last answered question.
        /// </summary>
        public SrsLevel PreviewNextLevel
        {
            get { return _previewNextLevel; }
            set
            {
                if (_previewNextLevel != value)
                {
                    _previewNextLevel = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Events

        public delegate void ReviewFinishedHandler(object sender, EventArgs e);
        /// <summary>
        /// Event triggered when the review session is over.
        /// </summary>
        public event ReviewFinishedHandler ReviewFinished;

        #endregion

        #region Commands

        /// <summary>
        /// Gets the command used to submit the answer.
        /// </summary>
        public RelayCommand SubmitCommand { get; private set; }

        /// <summary>
        /// Gets the command used to add the current answer
        /// as an accepted meaning or reading of the underlying entry.
        /// </summary>
        public RelayCommand AddAnswerCommand { get; private set; }

        /// <summary>
        /// Gets the command used wrap up the review session.
        /// </summary>
        public RelayCommand WrapUpCommand { get; private set; }

        /// <summary>
        /// Gets the command used to push the last question
        /// answered back to the pool as if it was never answered.
        /// </summary>
        public RelayCommand IgnoreAnswerCommand { get; private set; }

        /// <summary>
        /// Gets the command used to edit the SRS entry
        /// referred by the last question answered.
        /// </summary>
        public RelayCommand EditSrsEntryCommand { get; private set; }

        /// <summary>
        /// Gets the command used to close the current
        /// review session.
        /// </summary>
        public RelayCommand StopSessionCommand { get; private set; }

        /// <summary>
        /// Gets the command that uses a shorcut to ignore the currently submitted answer.
        /// </summary>
        public RelayCommand IgnoreAnswerShortcutCommand { get; private set; }

        /// <summary>
        /// Gets the command that uses a shortcut to add the currently submitted answer to the
        /// list of accepted answers.
        /// </summary>
        public RelayCommand AddAnswerShortcutCommand { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Builds the ViewModel.
        /// </summary>
        public SrsReviewViewModel()
        {
            _canSubmit = true;
            _random = new Random();
            _iterator = new FilteredSrsEntryIterator();
            _srsEntryDao = new SrsEntryDao();

            SubmitCommand = new RelayCommand(OnSubmit);
            AddAnswerCommand = new RelayCommand(OnAddAnswer);
            WrapUpCommand = new RelayCommand(OnWrapUp);
            IgnoreAnswerCommand = new RelayCommand(OnIgnoreAnswer);
            EditSrsEntryCommand = new RelayCommand(OnEditSrsEntry);
            StopSessionCommand = new RelayCommand(OnStopSession);
            IgnoreAnswerShortcutCommand = new RelayCommand(OnIgnoreAnswerShortcut);
            AddAnswerShortcutCommand = new RelayCommand(OnAddAnswerShortcut);
        }

        #endregion

        #region Methods

        #region Session methods

        /// <summary>
        /// Starts a new SRS session.
        /// </summary>
        public async Task StartSession()
        {
            await _iterator.ApplyFilter();
            IsReviewing = true;
            TotalReviewsCount = _iterator.ItemCount;
            AnsweredReviewsCount = 0;
            _isEntryAvailable = true;
            CurrentAnswer = string.Empty;
            _currentBatch = new List<SrsQuestionGroup>();
            await FillCurrentBatch();
            await ToNextQuestion();
        }

        /// <summary>
        /// Submits the current answer.
        /// </summary>
        private void SubmitAnswer()
        {
            if (string.IsNullOrEmpty(CurrentAnswer)
                || CurrentQuestion == null)
            {
                // Do not evaluate an empty answer.
                return;
            }

            // Convert the answer to kana if necessary.
            if (CurrentQuestion.Question == SrsQuestionEnum.Reading)
            {
                CurrentAnswer = KanaHelper.RomajiToKana(CurrentAnswer);
            }

            // Determine if the answer is correct.
            bool success = IsAnswerCorrect();

            // Set the review state accordingly.
            // Other operations will be executed when we move on to another question.
            ReviewState = success ? SrsReviewStateEnum.Success : SrsReviewStateEnum.Failure;

            // Play audio if auto play enabled.
            if (CurrentQuestion.Question == SrsQuestionEnum.Reading
                && CurrentQuestionGroup.IsVocab
                && ((success && Properties.UserSettings.Instance.AudioAutoplayMode.ShouldPlayOnSuccess())
                || (!success && Properties.UserSettings.Instance.AudioAutoplayMode.ShouldPlayOnFailure())))
            {
                AudioBusiness.PlayVocabAudio(CurrentQuestionGroup.Audio);
            }

            // If we have a failure: trigger the timer.
            if (ReviewState == SrsReviewStateEnum.Failure)
            {
                _canSubmit = false;
                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = FailureSubmitDelay;
                timer.Tick += OnFailureSubmitTimerTick;
                timer.Start();
            }
            // If it was a success on the last question of the group, allow level up/down preview.
            else if (CurrentQuestionGroup.GetUnansweredQuestions().Count() == 1)
            {
                SrsLevelStore.Instance.IssueWhenLoaded(
                    () => {
                        DispatcherHelper.Invoke(() => {
                            PreviewNextLevel = GetUpdatedLevel(CurrentQuestionGroup);
                        });
                    });
            }
        }

        /// <summary>
        /// Goes forward to the next question.
        /// </summary>
        private async Task ToNextQuestion()
        {
            if (CurrentQuestion != null)
            {
                // Execute the logic of success/failure for the last question.
                await ProcessLastSubmission();
            }

            PreviewNextLevel = null;

            // Try to pick a new question.
            CurrentQuestion = PickRandomQuestion();
            if (CurrentQuestion == null)
            {
                // No questions available.
                // End the review session.
                OnStopSession();
            }
            else
            {
                // Question successfuly picked.
                // Get ready to get an answer.

                CurrentAnswer = string.Empty;
                ReviewState = SrsReviewStateEnum.Input;
            }
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Attempts to fill the batch to its maximal capacity.
        /// </summary>
        private async Task FillCurrentBatch()
        {
            int remaining = BatchMaxSize - _currentBatch.Count;
            while (remaining-- > 0 && _isEntryAvailable)
            {
                SrsEntry next = await _iterator.GetNext(1).FirstOrDefaultAsync();
                if (next == null)
                {
                    _isEntryAvailable = false;
                }
                else
                {
                    _currentBatch.Add(ProcessEntry(next));
                }
            }
        }

        /// <summary>
        /// Gets a question group from an SRS entry.
        /// </summary>
        private SrsQuestionGroup ProcessEntry(SrsEntry entry)
        {
            return new SrsQuestionGroup(entry);
        }

        /// <summary>
        /// Picks and returns a random question from the batch.
        /// </summary>
        /// <returns>Randomly picked question, or null.</returns>
        private SrsQuestion PickRandomQuestion()
        {
            SrsQuestion[] eligibleQuestions = _currentBatch
                .SelectMany(g => g.GetUnansweredQuestions())
                .ToArray();

            if (eligibleQuestions.Any())
            {
                return eligibleQuestions[_random.Next(eligibleQuestions.Length)];
            }

            return null;
        }

        /// <summary>
        /// Gets a boolean indicating if the current answer is correct.
        /// </summary>
        private bool IsAnswerCorrect()
        {
            CurrentAnswer = CurrentAnswer.Trim();

            if (CurrentQuestion.Question == SrsQuestionEnum.Meaning)
            {
                // Look for an exact meaning answer.
                bool isExactAnswer = CurrentQuestionGroup.Reference.Meanings
                    .Split(MultiValueFieldHelper.ValueSeparator)
                    .Any(s => s.ToLower() == CurrentAnswer.ToLower());

                // If there wasn't an exact meaning, use the string distance
                // algorithm to approximate the answer.
                if (!isExactAnswer)
                {
                    // Compute all evaluation strings.
                    List<string> evaluations = new List<string>();
                    foreach (string meaning in CurrentQuestionGroup.Reference.Meanings.Split(MultiValueFieldHelper.ValueSeparator))
                    {
                        evaluations.Add(meaning);
                        string formattedMeaning = StringHelper.RemoveParenthesisExpressions(meaning);
                        if (formattedMeaning != meaning)
                        {
                            evaluations.Add(formattedMeaning);
                        }
                    }

                    // See if the answer at least approximately matches one of the evaluation strings.
                    string answer = evaluations.Where(s => StringDistanceHelper.DoDistributedLevenshteinDistance(
                        CurrentAnswer.ToLower(), s.ToLower(), MeaningDistanceLenience))
                        .FirstOrDefault();

                    if (answer != null)
                    {
                        // An approximate answer was found.
                        CurrentAnswer = answer;
                        return true;
                    }

                    // No approximate answer found either.
                    return false;
                }

                // Exact answer match.
                return true;
            }
            else if (CurrentQuestion.Question == SrsQuestionEnum.Reading)
            {
                // Look for an exact match for any reading.
                return CurrentQuestionGroup.Reference.Readings
                    .Split(MultiValueFieldHelper.ValueSeparator)
                    .Any(s => s.ToLower() == CurrentAnswer.ToLower());
            }

            // Should never happen.
            return false;
        }

        /// <summary>
        /// Processes the success/failure logic for the last answered question.
        /// Executed before getting to the next question.
        /// </summary>
        private async Task ProcessLastSubmission()
        {
            if (ReviewState == SrsReviewStateEnum.Success)
            {
                // Success. Set the question as answered.
                CurrentQuestion.IsAnswered = true;

                // If both questions of the group have been answered.
                if (!CurrentQuestionGroup.GetUnansweredQuestions().Any())
                {
                    SrsQuestionGroup group = CurrentQuestionGroup;

                    // Remove the group from the batch.
                    _currentBatch.Remove(group);

                    // Start the update group method after making sure the
                    // level store is finished loading.
                    SrsLevelStore.Instance.IssueWhenLoaded(
                        () => {
                            UpdateAnsweredGroup(group);
                        });

                    // Update the answered reviews count.
                    AnsweredReviewsCount++;

                    // Fill the batch to compensate for the removed group.
                    if (!IsWrappingUp)
                    {
                        await FillCurrentBatch();
                    }
                    else if (_currentBatch.Count == 0)
                    {
                        StopSessionCommand.Execute(null);
                    }
                }
            }
            else if (ReviewState == SrsReviewStateEnum.Failure)
            {
                // Wrong answer.
                // Don't set the question as answered.
                // Set the group as wrong.
                CurrentQuestion.ParentGroup.IsWrong = true;
            }
        }

        /// <summary>
        /// Called to update the SRS entry referred by a question group that was
        /// answered.
        /// </summary>
        /// <param name="group">Question group answered.</param>
        private void UpdateAnsweredGroup(SrsQuestionGroup group)
        {
            // Get the new level of the item.
            SrsLevel newLevel = GetUpdatedLevel(group);

            // Update its grade and next answer date.
            if (newLevel != null)
            {
                // Set the next answer date according to the delay of the new level.
                if (newLevel.Delay.HasValue)
                {
                    group.Reference.NextAnswerDate =
                        DateTimeOffset.Now
                        + newLevel.Delay.Value;
                }
                else
                {
                    group.Reference.NextAnswerDate = null;
                }

                group.Reference.CurrentGrade = newLevel.Value;
            }
            else
            {
                // If there is no new level and no current level, the delay is null.
                group.Reference.NextAnswerDate = null;
            }

            // Update the success/failure count.
            if (group.IsWrong)
            {
                group.Reference.FailureCount++;
            }
            else
            {
                group.Reference.SuccessCount++;
            }

            // Update the entry in the DB.
            UpdateEntry(group.Reference);
        }

        /// <summary>
        /// Gets the level that will be associated to the SRS entry matching the
        /// given group in the current state.
        /// </summary>
        /// <returns>New level for the entity.</returns>
        private SrsLevel GetUpdatedLevel(SrsQuestionGroup group)
        {
            // Take the upper/lower level depending on the success state.
            int modifier = group.IsWrong ? -1 : 1;
            SrsLevel newLevel = newLevel = SrsLevelStore.Instance.GetLevelByValue(
                    group.Reference.CurrentGrade + modifier);

            // If the upper/lower level could not be obtained, (as this may
            // be the case when at max/min level), use the current one.
            // Note that this still may return a null value if the levels
            // were not loaded correctly or if the configuration has changed.
            return newLevel ?? SrsLevelStore.Instance.GetLevelByValue(
                    group.Reference.CurrentGrade);
        }

        #endregion

        #region Background tasks

        #region UpdateEntry

        /// <summary>
        /// Updates the given entry.
        /// </summary>
        /// <param name="entry">Entry to update.</param>
        private void UpdateEntry(SrsEntry entry)
        {
            _currentTransactionCount++;
            Task.Run(() => DoUpdateEntry(entry).ContinueWith((t) => DoneUpdateEntry()));
        }

        /// <summary>
        /// Background task work method.
        /// Updates the given SRS item.
        /// </summary>
        private async Task DoUpdateEntry(SrsEntry entry)
        {
            if (!await _srsEntryDao.Update(entry))
            {
                LogHelper.Factory.CreateLogger(GetType()).LogWarning(
                    "The review update for the SRS entry \"{associated}\" ({id}) failed.",
                    entry.AssociatedKanji ?? entry.AssociatedVocab, entry.ID);
            }
        }

        /// <summary>
        /// Background task completed method. Unsubscribes to the events.
        /// </summary>
        private void DoneUpdateEntry()
        {
            // Trigger the event if the revision should be over.
            lock (_stopLock)
            {
                _currentTransactionCount--;
                if (!IsReviewing && ReviewFinished != null
                    && _currentTransactionCount <= 0)
                {
                    ReviewFinished(this, new EventArgs());
                }
            }
        }

        #endregion

        #endregion

        #region Command callbacks

        /// <summary>
        /// Command callback.
        /// Submits the current answer.
        /// </summary>
        private async void OnSubmit()
        {
            if (IsReviewing)
            {
                if (ReviewState == SrsReviewStateEnum.Input)
                {
                    SubmitAnswer();
                    if (ReviewState == SrsReviewStateEnum.Success
                        && Kanji.Interface.Properties.UserSettings.Instance.AutoSkipReviews)
                    {
                        await ToNextQuestion();
                    }
                }
                else if (_canSubmit)
                {
                    await ToNextQuestion();
                }
            }
        }

        /// <summary>
        /// Command callback.
        /// Adds the current answer to the list of readings or
        /// meanings of the entry referred by the given question.
        /// Also sets the state as successful and goes forward.
        /// </summary>
        private async void OnAddAnswer() => await AddAnswer();
        private async Task AddAnswer()
        {
            if (CurrentQuestion != null && ReviewState == SrsReviewStateEnum.Failure)
            {
                if (CurrentQuestion.Question == SrsQuestionEnum.Meaning)
                {
                    CurrentQuestionGroup.Reference.Meanings +=
                        MultiValueFieldHelper.ValueSeparator + CurrentAnswer;
                }
                else if (CurrentQuestion.Question == SrsQuestionEnum.Reading)
                {
                    CurrentQuestionGroup.Reference.Readings +=
                        MultiValueFieldHelper.ValueSeparator + CurrentAnswer;
                }
                RaisePropertyChanged(nameof(CurrentQuestion));

                ReviewState = SrsReviewStateEnum.Success;
                await ToNextQuestion();
            }
        }

        /// <summary>
        /// Command callback.
        /// Calls the AddAnswer command if shortcuts are enabled.
        /// </summary>
        private async void OnAddAnswerShortcut()
        {
            if (Properties.UserSettings.Instance.IsIgnoreAnswerShortcutEnabled)
            {
                await AddAnswer();
            }
        }

        /// <summary>
        /// Command callback.
        /// Wraps up the session by not introducing any more new items
        /// and offering only those of which either the meaning or the reading have appeared (but not both).
        /// </summary>
        private void OnWrapUp()
        {
            IsWrappingUp = true;
            TotalReviewsCount = AnsweredReviewsCount + _currentBatch.Count;
        }

        /// <summary>
        /// Command callback.
        /// Ignores the last answer and goes to the next question.
        /// </summary>
        private async void OnIgnoreAnswer()
        {
            if (ReviewState == SrsReviewStateEnum.Failure
                || ReviewState == SrsReviewStateEnum.Success)
            {
                ReviewState = SrsReviewStateEnum.Ignore;
                await ToNextQuestion();
            }
        }

        /// <summary>
        /// Command callback.
        /// Calls the IgnoreAnswer command if shortcuts are enabled.
        /// </summary>
        private void OnIgnoreAnswerShortcut()
        {
            if (Properties.UserSettings.Instance.IsIgnoreAnswerShortcutEnabled)
            {
                OnIgnoreAnswer();
            }
        }

        /// <summary>
        /// Command callback.
        /// Calls for the SRS item edition window to edit the SRS item
        /// referred by the current question.
        /// </summary>
        private async void OnEditSrsEntry()
        {
            if (CurrentQuestion != null)
            {
                SrsEntryEditedEventArgs e = await NavigationActor.Instance.OpenSrsEditWindow(CurrentQuestionGroup.Reference);
                if (e.IsSaved)
                {
                    if (e.SrsEntry != null
                        && e.SrsEntry.NextAnswerDate.HasValue
                        && e.SrsEntry.NextAnswerDate.Value.ToLocalTime() <= DateTimeOffset.Now
                        && !e.SrsEntry.SuspensionDate.HasValue)
                    {
                        // The result exists and is still due for review.
                        // Update the current group.
                        CurrentQuestionGroup.Reference = e.SrsEntry.Reference;
                        RaisePropertyChanged(nameof(CurrentQuestion));
                    }
                    else
                    {
                        // The result has been deleted or is no longer due
                        // for review. Remove the question group from the batch.
                        ReviewState = SrsReviewStateEnum.Ignore;
                        _currentBatch.Remove(CurrentQuestionGroup);
						if (!IsWrappingUp)
							await FillCurrentBatch();

                        AnsweredReviewsCount++;
                        await ToNextQuestion();
                    }
                }
            }
        }

        /// <summary>
        /// Command callback.
        /// Called to stop the current review session.
        /// </summary>
        private void OnStopSession()
        {
            lock (_stopLock)
            {
                if (IsReviewing)
                {
                    IsReviewing = false;
                }

                if (_currentTransactionCount <= 0 && ReviewFinished != null)
                {
                    ReviewFinished(this, new EventArgs());
                }
            }
        }

        #endregion

        #region Event callbacks

        /// <summary>
        /// Triggered when the failure submit timer ticks.
        /// Resets the failure submit boolean to allow submission.
        /// </summary>
        private void OnFailureSubmitTimerTick(object sender, EventArgs e)
        {
            DispatcherTimer timer = (DispatcherTimer)sender;
            timer.Stop();
            timer.Tick -= OnFailureSubmitTimerTick;

            _canSubmit = true;
        }

        #endregion

        #endregion
    }
}
