using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Kanji.Interface.ViewModels;

namespace Kanji.Interface.Views;

public partial class SettingsPage : UserControl
{
    public SettingsPage()
    {
        InitializeComponent();
        DataContext = new SettingsViewModel();
        this.GetObservable(IsVisibleProperty).Subscribe(OnIsVisibleChanged);
    }
    
    /// <summary>
    /// Since a <see cref="GalaSoft.MvvmLight.Command.RelayCommand"/> does not accept keyboard shortcuts,
    /// we have to manually invoke the commands on a keyboard event.
    /// </summary>
    private void OnKeyDown(object sender, KeyEventArgs e)
    {

        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            switch (e.Key)
            {

            }
        }
    }

    private void OnIsVisibleChanged(bool newValue)
    {
        // Focus the page once it becomes visible.
        // This is so that the navigation bar does not keep the focus, which would prevent shortcut keys from working.
        if (newValue)
        {
            Focus();
        }
    }
}
