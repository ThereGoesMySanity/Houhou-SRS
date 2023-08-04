using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kanji.Interface.ViewModels
{
    public class SettingAudioUriViewModel : SettingControlViewModel
    {
        #region Fields

        private string _audioUri;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the audio URI.
        /// </summary>
        public string AudioUri
        {
            get { return _audioUri; }
            set
            {
                if (_audioUri != value)
                {
                    _audioUri = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Constructors

        public override void InitializeSettings()
        {
            AudioUri = Properties.UserSettings.Instance.AudioUri;
        }

        #endregion

        #region Methods

        public override bool IsSettingChanged()
        {
            return Properties.UserSettings.Instance.AudioUri != _audioUri;
        }

        protected override void DoSaveSetting()
        {
            Properties.UserSettings.Instance.AudioUri = _audioUri;
        }

        #endregion
    }
}
