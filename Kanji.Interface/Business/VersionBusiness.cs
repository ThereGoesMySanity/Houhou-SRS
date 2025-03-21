﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using GalaSoft.MvvmLight.Command;
using Kanji.Common.Helpers;
using Kanji.Interface.Helpers;
using Kanji.Interface.Models;
using Kanji.Interface.Utilities;
using Microsoft.Extensions.Logging;

namespace Kanji.Interface.Business
{
    public class VersionBusiness : NotifyPropertyChanged
    {
        #region Constants

        // Delay before the first update check once the instance is initialized.
        private static readonly TimeSpan BeforeFirstCheckDelay = TimeSpan.FromMinutes(2);
        //private static readonly TimeSpan BeforeFirstCheckDelay = TimeSpan.FromSeconds(10);

        // Delay before another update check attempt is issued when an error occurs during a check.
        private static readonly TimeSpan CheckAttemptErrorDelay = TimeSpan.FromSeconds(5);

        // Maximal number of attempts to check available updates.
        private static readonly int MaxCheckAttemptsCount = 3;

        private static readonly string HouhouUserAgent = "Houhou SRS";

        #endregion

        #region Static

        /// <summary>
        /// Gets the loaded instance.
        /// </summary>
        public static VersionBusiness Instance { get; private set; }

        /// <summary>
        /// Initializes the instance.
        /// </summary>
        public static void Initialize()
        {
            if (Instance == null)
            {
                Instance = new VersionBusiness();
            }
            else
            {
                throw new InvalidOperationException(
                    "The instance is already loaded.");
            }
        }

        #endregion

        #region Fields

        // Stores the last check date.
        private DateTimeOffset _lastCheckDate;

        private Update _lastUpdate;

        private UpdateCheckStatusEnum _checkStatus;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current check status.
        /// </summary>
        public UpdateCheckStatusEnum CheckStatus
        {
            get { return _checkStatus; }
            private set
            {
                if (_checkStatus != value)
                {
                    _checkStatus = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the update model retrieved from the last update check.
        /// </summary>
        public Update LastUpdate
        {
            get { return _lastUpdate; }
            private set
            {
                if (_lastUpdate != value)
                {
                    _lastUpdate = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the current version string.
        /// </summary>
        public string CurrentVersion
        {
            get
            {
                Version v = Assembly.GetExecutingAssembly().GetName().Version;
                return v.Major + "." + v.Minor + "." + v.Build;
            }
        }

        /// <summary>
        /// Gets the last update check date.
        /// </summary>
        public DateTimeOffset LastCheckDate
        {
            get { return _lastCheckDate; }
            private set
            {
                if (_lastCheckDate != value)
                {
                    _lastCheckDate = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command used to perform an update check.
        /// </summary>
        public RelayCommand CheckCommand { get; private set; }

        /// <summary>
        /// Command used to perform an update action.
        /// </summary>
        public RelayCommand UpdateCommand { get; private set; }

        #endregion

        #region Constructors

        public VersionBusiness()
        {
            CheckStatus = UpdateCheckStatusEnum.NotYetChecked;
            CheckCommand = new RelayCommand(Check);
            UpdateCommand = new RelayCommand(Update);

            // Run the first update check.
            if (Properties.UserSettings.Instance.IsAutoUpdateCheckEnabled)
            {
                Task.Run(() =>
                {
                    // This first nap is used to delay the update check, because
                    // the app may be launched at Windows startup and we don't
                    // want to be too heavy.
                    Thread.Sleep(BeforeFirstCheckDelay);
                    if (Properties.UserSettings.Instance.IsAutoUpdateCheckEnabled)
                    {
                        Check();
                    }
                });
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Checks for reviews.
        /// </summary>
        public void Check()
        {
            LastCheckDate = DateTimeOffset.Now;
            CheckStatus = UpdateCheckStatusEnum.Checking;
            LastUpdate = DoCheck();

            // An update is available if an Update model was successfuly retrieved,
            // and the version described is above the current version.
            bool isSuccessful = _lastUpdate != null;
            Version v = null;
            bool isMoreRecent = isSuccessful && Version.TryParse(_lastUpdate.Version, out v)
                && v > Assembly.GetExecutingAssembly().GetName().Version;

            if (isMoreRecent)
            {
                // The last update is more recent. So there's an update available.
                CheckStatus = UpdateCheckStatusEnum.UpdateAvailable;
            }
            else if (isSuccessful)
            {
                // The last update has been successfuly retrieved but is not
                // more recent. So we are using the latest version.
                CheckStatus = UpdateCheckStatusEnum.MostRecent;
            }
            else
            {
                // The last update has not been successfuly retrieved.
                CheckStatus = UpdateCheckStatusEnum.Error;
            }
        }

        /// <summary>
        /// Triggers the action needed to update Houhou.
        /// Though that is only calling an URL for now.
        /// </summary>
        private void Update()
        {
            // Check that an update is indeed available.
            if (_checkStatus != UpdateCheckStatusEnum.UpdateAvailable
                || _lastUpdate == null)
            {
                LogHelper.Factory.CreateLogger(GetType()).LogWarning(
                    "Tried to Update when no update available. Ignoring.");
                return;
            }

            // Validate the URI.
            if (string.IsNullOrEmpty(_lastUpdate.Uri)
                || (!_lastUpdate.Uri.StartsWith("http://")
                   && !_lastUpdate.Uri.StartsWith("https://")))
            {
                LogHelper.Factory.CreateLogger(GetType()).LogWarning(
                    "Tried to Update with an invalid uri: \"{uri}\". Ignoring.",
                    _lastUpdate.Uri);
                return;
            }

            // Call the URI.
            ProcessHelper.OpenUri(_lastUpdate.Uri);
        }

        /// <summary>
        /// Performs the actual check.
        /// </summary>
        /// <param name="attempt">Attempt number.</param>
        /// <returns>Current reviews count.</returns>
        private Update DoCheck(int attempt = 1)
        {
            try
            {
                // Prepare the request.
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(
                    ConfigurationManager.AppSettings["UpdateCheckUri"]);
                request.UserAgent = HouhouUserAgent;

                // Execute the request.
                using (WebResponse response = request.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        // Deserialize the Update model from the response.
                        System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(Update));
                        return x.Deserialize(stream) as Update;
                    }
                }
            }
            catch (Exception ex)
            {
                // In the failure case, try again until the MaxCheckAttemptsCount is reached.
                var log = LogHelper.Factory.CreateLogger(GetType());
                log.LogError(ex, "Update check attempt #{attempt} failed.",
                    attempt);

                if (attempt < MaxCheckAttemptsCount)
                {
                    log.LogInformation("Attempting update check again.");
                    // After a nap, recursively call back the same method,
                    // but with an incremented attempt #.
                    Thread.Sleep(CheckAttemptErrorDelay);
                    return DoCheck(attempt + 1);
                }
                else
                {
                    // MaxCheckAttemptsCount reached. Stop and return 0.
                    log.LogInformation("Update check abandoned.");
                    return null;
                }
            }
        }

        #endregion
    }
}
