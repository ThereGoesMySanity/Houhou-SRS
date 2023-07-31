﻿using System;
using System.IO;
using GalaSoft.MvvmLight.Command;
using Kanji.Common.Helpers;
using Kanji.Interface.Actors;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;

namespace Kanji.Interface.ViewModels
{
    public class SettingUserDirectoryViewModel : SettingControlViewModel
    {
        #region Fields

        private string _userDirectoryPath;

        private bool _isEditMode;

        private string _errorMessage;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the user directory path.
        /// </summary>
        public string UserDirectoryPath
        {
            get { return _userDirectoryPath; }
            set
            {
                if (_userDirectoryPath != value)
                {
                    _userDirectoryPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the value indicating if the edit mode is active.
        /// </summary>
        public bool IsEditMode
        {
            get { return _isEditMode; }
            private set
            {
                if (_isEditMode != value)
                {
                    _isEditMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the error message matching the current situation.
        /// </summary>
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Gets the command used to enter edit mode.
        /// </summary>
        public RelayCommand EnterEditModeCommand { get; private set; }

        /// <summary>
        /// Gets the command used to set the directory.
        /// </summary>
        public RelayCommand SetDirectoryCommand { get; private set; }

        #endregion

        #region Constructors

        public override void InitializeSettings()
        {
            UserDirectoryPath = Properties.Settings.Default.UserDirectoryPath;
            EnterEditModeCommand = new RelayCommand(OnEnterEditMode);
            SetDirectoryCommand = new RelayCommand(OnSetDirectory);
        }

        #endregion

        #region Methods

        public override bool IsSettingChanged()
        {
            return false;
        }

        protected override void DoSaveSetting()
        {

        }

        #region Command callbacks

        private void OnEnterEditMode()
        {
            IsEditMode = true;
        }

        private async void OnSetDirectory()
        {
            // Check that the directory exists.
            if (!Directory.Exists(_userDirectoryPath))
            {
                ErrorMessage = "The destination directory does not exist.";
                return;
            }

            // Try to copy everything from the current user folder to the new one.
            try
            {
                FileHelper.CopyAllContent(Properties.Settings.Default.UserDirectoryPath, _userDirectoryPath);
            }
            catch (Exception ex)
            {
                ErrorMessage = string.Format("Copy failed with error: {0}", ex.Message);
                LogHelper.GetLogger("Configuration").Error("User directory path modification failed.", ex);
                return;
            }

            // Show dialog.
            await MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams
            {
                ButtonDefinitions = ButtonEnum.Ok,
                ContentTitle = "User directory changed",
                ContentMessage = $"Your user directory has been successfuly modified. Please restart Houhou completely now.{Environment.NewLine}Please note that for safety reasons, your old directory has not been deleted.",
                Icon = Icon.Info,
            }).ShowAsPopupAsync(NavigationActor.Instance.ActiveWindow);

            // Modify values.
            Properties.Settings.Default.UserDirectoryPath = _userDirectoryPath;
            Properties.Settings.Default.Save();
            ErrorMessage = string.Empty;
            IsEditMode = false;
        }

        #endregion

        #endregion
    }
}
