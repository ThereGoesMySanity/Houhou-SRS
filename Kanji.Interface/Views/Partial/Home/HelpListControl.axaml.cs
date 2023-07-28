using Avalonia.Controls;
using Avalonia.Input;

namespace Kanji.Interface.Controls;

public partial class HelpListControl : UserControl
{
    public HelpListControl()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Redistribute the mouse wheel event on the parent.
    /// We don't want the sublist to catch the mouse wheel events because it would
    /// prevent scrolling in the parent list.
    /// </summary>
    private void OnSubListBoxPreviewMouseWheel(object sender, PointerWheelEventArgs e)
    {
        if (!e.Handled)
        {
            //TODO
            e.Handled = true;
            var parent = ((Control)sender).Parent;
            //parent.RaiseEvent(e);
        }
    }
}