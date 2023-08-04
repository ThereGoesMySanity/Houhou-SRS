using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kanji.Interface.ViewModels
{
    public class SettingAutoSkipViewModel : SettingControlViewModel
    {
        #region Fields

        private bool _autoSkip;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the current status.
        /// </summary>
        public bool AutoSkip
        {
            get { return _autoSkip; }
            set
            {
                if (_autoSkip != value)
                {
                    _autoSkip = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Constructors

        public override void InitializeSettings()
        {
            AutoSkip = Properties.UserSettings.Instance.AutoSkipReviews;
        }

        #endregion

        #region Methods

        #region Overriden

        protected override void DoSaveSetting()
        {
            Properties.UserSettings.Instance.AutoSkipReviews = AutoSkip;
        }

        public override bool IsSettingChanged()
        {
            return AutoSkip != Properties.UserSettings.Instance.AutoSkipReviews;
        }

        #endregion

        #endregion
    }
}
