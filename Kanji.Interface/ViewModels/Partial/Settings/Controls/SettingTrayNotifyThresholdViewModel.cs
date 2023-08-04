using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kanji.Interface.ViewModels
{
    public class SettingTrayNotifyThresholdViewModel : SettingControlViewModel
    {
        #region Fields

        private long _threshold;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the current threshold.
        /// </summary>
        public long Threshold
        {
            get { return _threshold; }
            set
            {
                if (_threshold != value)
                {
                    _threshold = value;
                    RaisePropertyChanged();
                    RaiseSettingValueChanged();
                }
            }
        }

        #endregion

        #region Constructors

        public override void InitializeSettings()
        {
            Threshold = Math.Max(0, Properties.UserSettings.Instance.TrayNotificationCountThreshold);
        }

        #endregion

        #region Methods

        #region Overriden

        protected override void DoSaveSetting()
        {
            Properties.UserSettings.Instance.TrayNotificationCountThreshold = Threshold;
        }

        public override bool IsSettingChanged()
        {
            return Threshold != Math.Max(1, (int)Properties.UserSettings.Instance.TrayNotificationCountThreshold);
        }

        #endregion

        #endregion
    }
}
