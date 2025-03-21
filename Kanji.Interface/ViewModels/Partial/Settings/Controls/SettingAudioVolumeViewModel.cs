﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kanji.Interface.ViewModels
{
    public class SettingAudioVolumeViewModel : SettingControlViewModel
    {
        #region Fields

        private int _audioVolume;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the volume of the audio.
        /// </summary>
        public int AudioVolume
        {
            get { return _audioVolume; }
            set
            {
                if (_audioVolume != value)
                {
                    _audioVolume = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Constructors

        public override void InitializeSettings()
        {
            AudioVolume = (int)Properties.UserSettings.Instance.AudioVolume;
        }

        #endregion

        #region Methods

        public override bool IsSettingChanged()
        {
            return Properties.UserSettings.Instance.AudioVolume != _audioVolume;
        }

        protected override void DoSaveSetting()
        {
            Properties.UserSettings.Instance.AudioVolume = _audioVolume;
        }

        #endregion
    }
}
