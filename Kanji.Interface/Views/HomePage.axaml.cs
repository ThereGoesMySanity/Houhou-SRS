using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Kanji.Interface.ViewModels;

namespace Kanji.Interface.Views;

public partial class HomePage : UserControl
{
    public HomePage()
    {
        InitializeComponent();
        DataContext = new HomeViewModel();
        this.GetObservable(IsVisibleProperty).Subscribe(OnIsVisibleChanged);
    }

    #region Methods
    
    //TODO: once hyperlinks are back
    // private void OnRequestNavigate(object sender, RequestNavigateEventArgs e)
    // {
    //    ProcessHelper.OpenUrl(e.Uri.ToString());
    // }

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

    private void OnIsVisibleChanged(bool obj)
    {
        // Focus the page once it becomes visible.
        // This is so that the navigation bar does not keep the focus, which would prevent shortcut keys from working.
        if (obj)
        {
            Focus();
        }
    }

    #endregion
}
