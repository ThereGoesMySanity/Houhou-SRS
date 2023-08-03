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
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Kanji.Interface.ViewModels
{
    public class WkImportRequestViewModel : ImportStepViewModel
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
        private async Task RequestApi()
        {
            //// Check the worker
            //if (_worker != null)
            //{
                //_worker.Dispose();
            //}

            // Initialize values
            IsComplete = false;
            IsError = false;
            IsWorking = true;
            Error = string.Empty;
            Result = null;

            await DoRequestApi();
            IsWorking = false;
            //// Run the initialization in the background.
            //_worker = new BackgroundWorker();
            //_worker.DoWork += DoRequestApi;
            //_worker.RunWorkerCompleted += DoneRequestApi;
            //_worker.RunWorkerAsync();
            //((BackgroundWorker)sender).DoWork -= DoRequestApi;
            //((BackgroundWorker)sender).RunWorkerCompleted -= DoneRequestApi;
        }

        /// <summary>
        /// Background task work method.
        /// Contacts the WaniKani API.
        /// </summary>
        private async Task DoRequestApi()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var baseUrl = "https://api.wanikani.com/v2/";
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_parent.ApiKey}");
                    List<string> subjectTypes = new List<string>();
                    if (_parent.ImportMode == WkImportMode.All || _parent.ImportMode == WkImportMode.Kanji)
                    {
                        subjectTypes.Add("kanji");
                    }
                    if (_parent.ImportMode == WkImportMode.All || _parent.ImportMode == WkImportMode.Vocab)
                    {
                        subjectTypes.Add("vocabulary");
                    }
                    if (subjectTypes.Count > 0)
                    {
                        List<(JToken, JToken)> results = new List<(JToken, JToken)>();
                        JObject response = JObject.Parse(await client.GetStringAsync(baseUrl + $"assignments?subject_types={String.Join(",", subjectTypes)}"));
                        JObject subRes = JObject.Parse(await client.GetStringAsync(baseUrl + $"subjects?ids={String.Join(",", response["data"].Select(t => t["data"]["subject_id"]))}"));
                        results.AddRange(response["data"].Select(r => (subRes["data"].First(s => (int)s["id"] == (int)r["data"]["subject_id"])["data"], r["data"])));
                        while (response["pages"]["next_url"] != null && results.Count < (int)response["total_count"])
                        {
                            response = JObject.Parse(await client.GetStringAsync((string)response["pages"]["next_url"]));
                            subRes = JObject.Parse(await client.GetStringAsync(baseUrl + $"subjects?ids={String.Join(",", response["data"].Select(t => t["data"]["subject_id"]))}"));
                            results.AddRange(response["data"].Select(r => (subRes["data"].First(s => (int)s["id"] == (int)r["data"]["subject_id"])["data"], r["data"])));
                        }

                        WkImportResult result = new WkImportResult();

                        //TODO: Level 0 is "haven't completed the lesson" - what do with those?
                        //TODO: Hint vs Mnemonic: Mnemonic is the one that comes up in lessons, hint comes up in reviews (and only exists for some objects?).
                        //      Which does the user want?
                        result.Items = results.Where(t => (int)t.Item2["srs_stage"] != 0).Select(((JToken s, JToken a) t) => new WkItem
                        {
                            IsKanji = (string)t.a["subject_type"] == "kanji",
                            KanjiReading = (string)t.s["characters"],
                            MeaningNote = (string)t.s["meaning_hint"] ?? (string)t.s["meaning_mnemonic"],
                            ReadingNote = (string)t.s["reading_hint"] ?? (string)t.s["reading_mnemonic"],
                            Meanings = string.Join(',', t.s["meanings"].Where(m => (bool)m["accepted_answer"]).Select(m => (string)m["meaning"])),
                            NextReviewDate = (DateTime?)t.a["available_at"],
                            Readings = string.Join(',', t.s["readings"].Where(r => (bool)r["accepted_answer"]).Select(r => (string)r["reading"])),
                            SrsLevel = (short)((short)t.a["srs_stage"] - 1),
                            WkLevel = (int)t.s["level"],
                        }).ToList();

                        Result = result;
                        IsComplete = true;
                    }
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

        #endregion

        public override async Task OnEnterStep()
        {
            await RequestApi();
        }

        public override Task<bool> OnNextStep()
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

            return Task.FromResult(true);
        }

        #endregion
    }
}
