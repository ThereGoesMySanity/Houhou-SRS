using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Kanji.Interface.Controls;
using Kanji.Interface.ViewModels;

namespace Kanji.Interface.Views;

public partial class VocabPage : UserControl
{
    public VocabPage()
    {
        InitializeComponent();
        DataContext = new VocabViewModel();
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
                case Key.R:
                    FilterControl.ReadingFilter.Focus();
                    e.Handled = true;
                    break;
                case Key.M:
                    FilterControl.MeaningFilter.Focus();
                    e.Handled = true;
                    break;
                case Key.W:
                    FilterControl.WkLevelFilter.LevelSlider.Focus();
                    e.Handled = true;
                    break;
                case Key.J:
                    FilterControl.JlptLevelFilter.LevelSlider.Focus();
                    e.Handled = true;
                    break;
                case Key.C:
                    // We can't just use CTRL+C here, because that would not work if a text box had focus.
                    if (e.KeyModifiers.HasFlag(KeyModifiers.Alt))
                    {
                        FilterControl.CategoryFilter.ComboBox.Focus();
                        e.Handled = true;
                    }
                    break;
            }
        }

        switch (e.Key)
        {
            case Key.Enter:
                FilterControl.ApplyFilterButton.Command.Execute(null);
                e.Handled = true;
                break;
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
