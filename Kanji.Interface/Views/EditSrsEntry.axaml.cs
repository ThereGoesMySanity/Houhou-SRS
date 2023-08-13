using Avalonia.Controls;
using Avalonia.Input;
using Kanji.Interface.ViewModels;

namespace Kanji.Interface.Views;

public partial class EditSrsEntry : UserControl
{
    public EditSrsEntry()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Since a <see cref="GalaSoft.MvvmLight.Command.RelayCommand"/> does not accept keyboard shortcuts,
    /// we have to manually invoke the commands on a keyboard event.
    /// </summary>
    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        /* 
            * - CTRL+Enter -> SubmitCommand
            * - CTRL+Delete -> DeleteCommand
            * - CTRL+R -> DateToNowCommand
            * - CTRL+N -> DateToNeverCommand
            * - CTRL+E -> ToggleDateEditCommand
            */


        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            SrsEntryViewModel viewModel = ((SrsEntryViewModel)DataContext);
            switch (e.Key)
            {
                case Key.Enter:
                    viewModel.SubmitCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.Delete:
                    viewModel.DeleteCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.R:
                    viewModel.DateToNowCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.N:
                    viewModel.DateToNeverCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.E:
                    viewModel.ToggleDateEditCommand.Execute(null);
                    if (viewModel.IsEditingDate)
                        ReviewDatePicker.Focus();
                    e.Handled = true;
                    break;
                case Key.S:
                    viewModel.ToggleSuspendCommand.Execute(null);
                    e.Handled = true;
                    break;
            }
        }
    }
}