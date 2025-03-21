﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using Kanji.Database.Entities;
using Kanji.Interface.Models;
using Kanji.Interface.Business;
using Kanji.Database.Dao;

namespace Kanji.Interface.ViewModels
{
    public abstract class ImportModeViewModel : ViewModel
    {
        #region Fields

        protected List<ImportStepViewModel> _steps;
        protected ImportStepViewModel _currentStep;
        protected List<SrsEntry> _newEntries;
        protected string _importLog;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the current import step.
        /// </summary>
        public ImportStepViewModel CurrentStep
        {
            get { return _currentStep; }
            set
            {
                if (_currentStep != value)
                {
                    _currentStep = value;
                    RaisePropertyChanged();
                    if (StepChanged != null)
                    {
                        StepChanged(this, new EventArgs());
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets new SRS entries to be imported.
        /// </summary>
        public List<SrsEntry> NewEntries
        {
            get { return _newEntries; }
            set
            {
                if (_newEntries != value)
                {
                    _newEntries = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the import log string.
        /// </summary>
        public string ImportLog
        {
            get { return _importLog; }
            set
            {
                if (_importLog != value)
                {
                    _importLog = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the duplicate options view model.
        /// </summary>
        public ImportDuplicateOptionsViewModel DuplicateOptions { get; private set; }

        /// <summary>
        /// Gets the SRS timing options for the imported items.
        /// </summary>
        public SrsTimingViewModel Timing { get; private set; }

        #endregion

        #region Commands

        /// <summary>
        /// Gets the command used to go forward to the next step.
        /// </summary>
        public RelayCommand NextStepCommand { get; private set; }

        /// <summary>
        /// Gets the command used to go back to the previous step.
        /// </summary>
        public RelayCommand PreviousStepCommand { get; private set; }

        #endregion

        #region Events

        public delegate void CancelHandler(object sender, EventArgs e);
        /// <summary>
        /// Event called when the import process is cancelled.
        /// </summary>
        public event CancelHandler Cancel;

        public delegate void StepChangedHandler(object sender, EventArgs e);
        /// <summary>
        /// Event called when the current step changes.
        /// </summary>
        public event StepChangedHandler StepChanged;

        public delegate void FinishedHandler(object sender, EventArgs e);
        /// <summary>
        /// Event called when the import process is finished.
        /// </summary>
        public event FinishedHandler Finished;

        #endregion

        #region Constructors

        public ImportModeViewModel()
        {
            NextStepCommand = new RelayCommand(NextStep);
            PreviousStepCommand = new RelayCommand(PreviousStep);
            DuplicateOptions = new ImportDuplicateOptionsViewModel();
            Timing = new SrsTimingViewModel();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the view model.
        /// </summary>
        protected void Initialize()
        {
            _currentStep = _steps[0];
        }

        /// <summary>
        /// Applies the correct timing to the items, according to the configuration.
        /// </summary>
        public void ApplyTiming()
        {
            // Timing is applied only to entries that do not already have a next answer date
            // and that are not in a final SRS level.
            var srsDao = new SrsEntryDao();
            List<SrsEntry> eligibleEntries = NewEntries.Where(e => !e.NextAnswerDate.HasValue
                && !SrsLevelStore.Instance.IsFinalLevel(e.CurrentGrade, false)
                && (DuplicateOptions.DuplicateNewItemAction != ImportDuplicateNewItemAction.Ignore || srsDao.GetSimilarItem(e) == null)
                ).ToList();

            Timing.ApplyTiming(eligibleEntries);
        }

        /// <summary>
        /// Goes forward to the next import step.
        /// </summary>
        public async void NextStep()
        {
            if (!IsLastStep())
            {
                if (await CurrentStep.OnNextStep())
                {
                    CurrentStep = _steps[GetStepIndex() + 1];
                    await CurrentStep.OnEnterStep();
                }
            }
            else if (Finished != null)
            {
                Finished(this, new EventArgs());
            }
        }

        /// <summary>
        /// Goes back to the previous import step.
        /// </summary>
        public async void PreviousStep()
        {
            CurrentStep.OnPreviousStep();

            if (GetStepIndex() > 0)
            {
                while (GetStepIndex() > 0)
                {
                    CurrentStep = _steps[GetStepIndex() - 1];
                    if (!CurrentStep.SkipOnPrevious)
                    {
                        break;
                    }
                }
                
                await CurrentStep.OnEnterStep();
            }
            else if (Cancel != null)
            {
                Cancel(this, new EventArgs());
            }
        }

        /// <summary>
        /// Gets a boolean value indicating if the current step is the last import step of the process.
        /// </summary>
        public bool IsLastStep()
        {
            return GetStepIndex() >= _steps.Count - 1;
        }

        /// <summary>
        /// Gets the index of the current step in the step list.
        /// </summary>
        protected int GetStepIndex()
        {
            return _steps.IndexOf(CurrentStep);
        }

        #endregion
    }
}
