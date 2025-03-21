﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kanji.Interface.Models;

namespace Kanji.Interface.ViewModels
{
    public class SettingReviewPlayAudio : SettingControlViewModel
    {
        #region Fields

        private AudioAutoplayModeEnum _autoPlayMode;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the audio autoplay mode.
        /// </summary>
        public AudioAutoplayModeEnum AutoPlayMode
        {
            get { return _autoPlayMode; }
            set
            {
                if (_autoPlayMode != value)
                {
                    _autoPlayMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Constructors

        public override void InitializeSettings()
        {
            AutoPlayMode = Properties.UserSettings.Instance.AudioAutoplayMode;
        }

        #endregion

        #region Methods

        public override bool IsSettingChanged()
        {
            return Properties.UserSettings.Instance.AudioAutoplayMode != AutoPlayMode;
        }

        protected override void DoSaveSetting()
        {
            Properties.UserSettings.Instance.AudioAutoplayMode = AutoPlayMode;
        }

        #endregion
    }
}
