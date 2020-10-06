using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;










namespace Kanji.Interface.Controls
{
    public partial class HelpListControl : UserControl
    {
        public HelpListControl()
        {
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
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
                parent.RaiseEvent(e);
            }
        }
    }
}
