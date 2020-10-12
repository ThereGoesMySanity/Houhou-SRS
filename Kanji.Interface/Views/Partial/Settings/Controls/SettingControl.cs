using System;
using Avalonia.Controls;
using Kanji.Interface.ViewModels;

namespace Kanji.Interface.Controls
{
    public abstract class SettingControl : UserControl
    {
        protected override void OnDataContextChanged(EventArgs args)
        {
            if (DataContext is SettingControlViewModel scvm)
            {
                scvm.InitializeSettings();
            }
        }
    }
}