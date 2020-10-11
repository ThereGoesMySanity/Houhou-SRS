using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kanji.Interface.Actors;
using Kanji.Interface.Models;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;

namespace Kanji.Interface.ViewModels
{
    class SettingSrsLevelSetViewModel : SettingUserResourceViewModel
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
            var result = await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams
            {
                ContentTitle = "SRS level set change warning",
                ContentMessage = "Please be aware that modifying the SRS level set may block "
                + "some existing SRS items in the case where the new level set has less "
                + $"levels than the previous one.{Environment.NewLine}Also, please note that the current "
                + "scheduled review dates will not be affected.",
                ButtonDefinitions = ButtonEnum.OkCancel,
                Icon = Icon.Warning,
            }).ShowDialog(NavigationActor.Instance.MainWindow);
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
