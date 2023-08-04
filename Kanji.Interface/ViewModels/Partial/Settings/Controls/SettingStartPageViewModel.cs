using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kanji.Interface.Models;

namespace Kanji.Interface.ViewModels
{
    public class SettingStartPageViewModel : SettingControlViewModel
    {
        #region Fields

        private StartPageEnum _startPage;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the start page value to be applied.
        /// </summary>
        public StartPageEnum StartPage
        {
            get { return _startPage; }
            set
            {
                if (_startPage != value)
                {
                    _startPage = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Constructors

        public override void InitializeSettings()
        {
            StartPage = Properties.UserSettings.Instance.StartPage;
        }

        #endregion

        #region Methods

        #region Overriden

        protected override void DoSaveSetting()
        {
            Properties.UserSettings.Instance.StartPage = StartPage;
        }

        public override bool IsSettingChanged()
        {
            return StartPage != Properties.UserSettings.Instance.StartPage;
        }

        #endregion

        #endregion
    }
}
