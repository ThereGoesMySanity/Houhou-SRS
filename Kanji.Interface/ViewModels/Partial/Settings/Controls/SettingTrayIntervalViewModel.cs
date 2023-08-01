using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kanji.Interface.Business;

namespace Kanji.Interface.ViewModels
{
    public class SettingTrayIntervalViewModel : SettingControlViewModel
    {
        #region Fields

        private int _interval;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the current interval in minutes.
        /// </summary>
        public int Interval
        {
            get { return _interval; }
            set
            {
                if (_interval != value)
                {
                    _interval = value;
                    RaisePropertyChanged();
                    RaiseSettingValueChanged();
                }
            }
        }

        #endregion

        #region Constructors

        public override void InitializeSettings()
        {
            Interval = Math.Max(1, (int)Properties.Settings.Default.TrayCheckInterval.TotalMinutes);
        }

        #endregion

        #region Methods

        #region Overriden

        protected override void DoSaveSetting()
        {
            Properties.Settings.Default.TrayCheckInterval = TimeSpan.FromMinutes(Interval);
            //TODO
            //TrayBusiness.Instance.ReloadConfiguration();
        }

        public override bool IsSettingChanged()
        {
            return Interval != Math.Max(1, (int)Properties.Settings.Default.TrayCheckInterval.TotalMinutes);
        }

        #endregion

        #endregion
    }
}
