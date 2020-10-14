using Kanji.Common.Helpers;
using Kanji.Database.Entities;
using Kanji.Interface.Business;
using Kanji.Interface.Models;
using Kanji.Interface.Models.Import;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WaniKaniApi;
using WaniKaniApi.Models;
using WaniKaniApi.Models.Filters;

namespace Kanji.Interface.ViewModels
{
    class WkImportRequestViewModel : ImportStepViewModel
    {
        #region Constants

        private const int VocabularyRequestLevelInterval = 10;

        #endregion

        #region Fields

        private WkImportViewModel _parent;

        private string _error;
        private bool _isComplete;
        private bool _isError;
        private bool _isWorking;
        private BackgroundWorker _worker;
        private WkImportResult _result;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating if the request was successfully completed.
        /// </summary>
        public bool IsComplete
        {
            get { return _isComplete; }
            set
            {
                if (_isComplete != value)
                {
                    _isComplete = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating if the request resulted in an error.
        /// </summary>
        public bool IsError
        {
            get { return _isError; }
            private set
            {
                if (_isError != value)
                {
                    _isError = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating if the worker is still operating.
        /// </summary>
        public bool IsWorking
        {
            get { return _isWorking; }
            private set
            {
                if (_isWorking != value)
                {
                    _isWorking = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets a string representing the error that occured during the process, if any.
        /// </summary>
        public string Error
        {
            get { return _error; }
            private set
            {
                if (_error != value)
                {
                    _error = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the results of the request.
        /// </summary>
        public WkImportResult Result
        {
            get { return _result; }
            private set
            {
                if (_result != value)
                {
                    _result = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Constructors

        public WkImportRequestViewModel(ImportModeViewModel parent)
            : base(parent)
        {
            _parent = (WkImportViewModel)parent;
            SkipOnPrevious = true;
            IsWorking = true;
        }

        #endregion

        #region Methods

        #region RequestApi

        /// <summary>
        /// Starts a background task to perform the request in the background.
        /// </summary>
        private void RequestApi()
        {
            // Check the worker
            if (_worker != null)
            {
                _worker.Dispose();
            }

            // Initialize values
            IsComplete = false;
            IsError = false;
            IsWorking = true;
            Error = string.Empty;
            Result = null;

            // Run the initialization in the background.
            _worker = new BackgroundWorker();
            _worker.DoWork += DoRequestApi;
            _worker.RunWorkerCompleted += DoneRequestApi;
            _worker.RunWorkerAsync();
        }

        /// <summary>
        /// Background task work method.
        /// Contacts the WaniKani API.
        /// </summary>
        private void DoRequestApi(object sender, DoWorkEventArgs e)
        {
            try
            {
                var client = new WaniKaniClient(_parent.ApiKey);
                AssignmentsFilter filter = null;

                if (_parent.ImportMode == WkImportMode.All)
                {
                    filter = new AssignmentsFilter() {SubjectTypes = new SubjectType[] {SubjectType.Kanji, SubjectType.Vocabulary}};
                }
                else if (_parent.ImportMode == WkImportMode.Kanji)
                {
                    filter = new AssignmentsFilter() { SubjectTypes = new SubjectType[] {SubjectType.Kanji}};
                }
                else if (_parent.ImportMode == WkImportMode.Vocab)
                {
                    filter = new AssignmentsFilter() { SubjectTypes = new SubjectType[] {SubjectType.Vocabulary}};
                }
                if (filter != null)
                {
                    List<(Subject, Assignment)> results = new List<(Subject, Assignment)>();
                    var response = client.Assignments.GetAllAsync(filter).Result;
                    var subRes = client.Subjects.GetAllAsync(new SubjectsFilter() { Ids = response.Data.Select(r => r.SubjectId).ToArray() }).Result;
                    results.AddRange(response.Data.Select(r => (subRes.Data.Where(s => s.Id == r.SubjectId).First(), r)));
                    while(response.NextPageUrl != null && results.Count < response.TotalCount)
                    {
                        response = client.Assignments.GetAllAsync(response.NextPageUrl).Result;
                        subRes = client.Subjects.GetAllAsync(new SubjectsFilter() { Ids = response.Data.Select(r => r.SubjectId).ToArray() }).Result;
                        results.AddRange(response.Data.Select(r => (subRes.Data.Where(s => s.Id == r.SubjectId).First(), r)));
                    }

                    WkImportResult result = new WkImportResult();
                    result.Items = results.Select(((Subject s, Assignment a) t) => new WkItem
                    {
                        IsKanji = t.a.SubjectType == "kanji",
                        KanjiReading = (t.s as WaniKaniApi.Models.Kanji)?.Characters ?? (t.s as Vocabulary)?.Characters,
                        MeaningNote = t.s.MeaningMnemonic,
                        Meanings = string.Join(',', t.s.Meanings.Where(m => m.AcceptedAnswer).Select(m => m.Text)),
                        NextReviewDate = t.a.AvailableAt?.UtcDateTime,
                        ReadingNote = (t.s as Vocabulary)?.ReadingMnemonic,
                        Readings = (t.s is Vocabulary v)? string.Join(',', v.Readings.Select(r => r.AcceptedAnswer)) : null,
                        SrsLevel = (short)t.a.SrsStageId,
                        WkLevel = t.s.Level,
                    }).ToList();

                    Result = result;
                    IsComplete = true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.GetLogger("WkImport").Error("An error occured during the request to the WaniKani API.", ex);
                Error = "An error occured while trying to request the data. Please consult your log file for more details.";
                IsError = true;
                return;
            }
        }

        /// <summary>
        /// Background task completed method. Unsubscribes to the events.
        /// </summary>
        private void DoneRequestApi(object sender, RunWorkerCompletedEventArgs e)
        {
            ((BackgroundWorker)sender).DoWork -= DoRequestApi;
            ((BackgroundWorker)sender).RunWorkerCompleted -= DoneRequestApi;
            IsWorking = false;
        }

        #endregion

        public override void OnEnterStep()
        {
            base.OnEnterStep();
            RequestApi();
        }

        public override bool OnNextStep()
        {
            List<SrsEntry> newEntries = new List<SrsEntry>();
            foreach (WkItem wkItem in Result.Items)
            {
                SrsEntry entry = new SrsEntry();
                if (wkItem.IsKanji)
                {
                    entry.AssociatedKanji = wkItem.KanjiReading;
                }
                else
                {
                    entry.AssociatedVocab = wkItem.KanjiReading;
                }

                entry.CurrentGrade = wkItem.SrsLevel;
                entry.MeaningNote = wkItem.MeaningNote;
                entry.Meanings = wkItem.Meanings;
                entry.NextAnswerDate = wkItem.NextReviewDate;
                entry.ReadingNote = wkItem.ReadingNote;
                entry.Readings = wkItem.Readings;
                entry.SuspensionDate = _parent.IsStartEnabled ? (DateTime?)null : DateTime.Now;
                entry.Tags = _parent.Tags.Replace(WkImportViewModel.LevelSpecialString, wkItem.WkLevel.ToString());
                newEntries.Add(entry);
            }

            _parent.NewEntries = newEntries;
            _parent.ApplyTiming();

            return true;
        }

        #endregion
    }
}
