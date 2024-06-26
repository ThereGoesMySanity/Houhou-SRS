﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kanji.Interface.ViewModels
{
    public class ApplicationSettingsViewModel : SettingsPageViewModel
    {
        #region Fields



        #endregion

        #region Properties

        /// <summary>
        /// Gets the start page setting view model.
        /// </summary>
        public SettingStartPageViewModel StartPageVm { get; private set; }

        /// <summary>
        /// Gets the auto update check toggle View Model.
        /// </summary>
        public SettingDoUpdateCheckViewModel DoUpdateCheckVm { get; private set; }

        /// <summary>
        /// Gets the SRS Tray Autostart view model.
        /// </summary>
        public SettingTrayAutoStartViewModel SrsTrayAutostartVm { get; private set; }

        /// <summary>
        /// Gets the SRS Tray Interval view model.
        /// </summary>
        public SettingTrayIntervalViewModel SrsTrayIntervalVm { get; private set; }

        /// <summary>
        /// Gets the SRS Tray Notification status view model.
        /// </summary>
        public SettingTrayDoNotifyViewModel SrsTrayDoNotifyVm { get; private set; }

        /// <summary>
        /// Gets the SRS Tray Notification Threshold view model.
        /// </summary>
        public SettingTrayNotifyThresholdViewModel SrsTrayThresholdVm { get; private set; }

        /// <summary>
        /// Gets the User Directory view model.
        /// </summary>
        public SettingUserDirectoryViewModel UserDirectoryVm { get; private set; }

        /// <summary>
        /// Gets the window close action view model.
        /// </summary>
        public SettingWindowCloseActionViewModel WindowCloseActionVm { get; private set; }

        #endregion

        #region Constructor



        #endregion

        #region Methods

        #region Override

        protected override SettingControlViewModel[] InitializeSettingViewModels()
        {
            StartPageVm = new SettingStartPageViewModel();
            SrsTrayAutostartVm = new SettingTrayAutoStartViewModel();
            SrsTrayIntervalVm = new SettingTrayIntervalViewModel();
            SrsTrayDoNotifyVm = new SettingTrayDoNotifyViewModel();
            SrsTrayThresholdVm = new SettingTrayNotifyThresholdViewModel();
            DoUpdateCheckVm = new SettingDoUpdateCheckViewModel();
            UserDirectoryVm = new SettingUserDirectoryViewModel();
            WindowCloseActionVm = new SettingWindowCloseActionViewModel();

            return new SettingControlViewModel[]
            {
                StartPageVm,
                SrsTrayAutostartVm,
                SrsTrayIntervalVm,
                SrsTrayDoNotifyVm,
                SrsTrayThresholdVm,
                DoUpdateCheckVm,
                UserDirectoryVm,
                WindowCloseActionVm
            };
        }

        #endregion

        #endregion
    }
}
