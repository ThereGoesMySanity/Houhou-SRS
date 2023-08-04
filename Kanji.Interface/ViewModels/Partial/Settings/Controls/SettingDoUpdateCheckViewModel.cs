using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kanji.Interface.ViewModels
{
    public class SettingDoUpdateCheckViewModel : SettingControlViewModel
    {
        #region Fields

        private bool _doUpdateCheck;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the current status.
        /// </summary>
        public bool DoUpdateCheck
        {
            get { return _doUpdateCheck; }
            set
            {
                if (_doUpdateCheck != value)
                {
                    _doUpdateCheck = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Constructors

        public override void InitializeSettings()
        {
            DoUpdateCheck = Properties.UserSettings.Instance.IsAutoUpdateCheckEnabled;
        }

        #endregion

        #region Methods

        #region Overriden

        protected override void DoSaveSetting()
        {
            Properties.UserSettings.Instance.IsAutoUpdateCheckEnabled = DoUpdateCheck;
        }

        public override bool IsSettingChanged()
        {
            return DoUpdateCheck != Properties.UserSettings.Instance.IsAutoUpdateCheckEnabled;
        }

        #endregion

        #endregion
    }
}
