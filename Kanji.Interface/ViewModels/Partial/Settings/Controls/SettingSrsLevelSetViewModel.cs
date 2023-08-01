﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kanji.Interface.Actors;
using Kanji.Interface.Models;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;

namespace Kanji.Interface.ViewModels
{
    public class SettingSrsLevelSetViewModel : SettingUserResourceViewModel
    {
        #region Methods

        #region Override

        protected override string GetInitialSetName()
        {
            return Properties.Settings.Default.SrsLevelSetName;
        }

        protected override async Task<bool> CanChangeSet(UserResourceSetInfo setInfo)
        {
            // Show validation messagebox.
            var result = await MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams
            {
                ContentTitle = "SRS level set change warning",
                ContentMessage = "Please be aware that modifying the SRS level set may block "
                + "some existing SRS items in the case where the new level set has less "
                + $"levels than the previous one.{Environment.NewLine}Also, please note that the current "
                + "scheduled review dates will not be affected.",
                ButtonDefinitions = ButtonEnum.OkCancel,
                Icon = Icon.Warning,
            }).ShowAsPopupAsync(NavigationActor.Instance.ActiveWindow);
            return result == ButtonResult.Ok;
        }

        public override bool IsSettingChanged()
        {
            return Properties.Settings.Default.SrsLevelSetName != SelectedSetName;
        }

        protected override void DoSaveSetting()
        {
            Properties.Settings.Default.SrsLevelSetName = SelectedSetName;
        }

        #endregion

        #endregion
    }
}
