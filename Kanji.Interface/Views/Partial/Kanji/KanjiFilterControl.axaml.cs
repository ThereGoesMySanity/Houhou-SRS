using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;






using Avalonia.Controls;
using Avalonia.Markup.Xaml;




namespace Kanji.Interface.Controls
{
    public partial class KanjiFilterControl : UserControl
    {
        public KanjiFilterControl()
        {
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            TextFilter = this.FindControl<CommandTextBox>("TextFilter");
            Filter = this.FindControl<CommandTextBox>("Filter");
            FilterModeCombobox = this.FindControl<ComboBox>("FilterModeCombobox");
            WkLevelFilter = this.FindControl<WkLevelFilterControl>("WkLevelFilter");
            JlptLevelFilter = this.FindControl<JlptLevelFilterControl>("JlptLevelFilter");
            RadicalNameFilter = this.FindControl<TextBox>("RadicalNameFilter");
        }
        internal CommandTextBox TextFilter { get; private set; }
        internal CommandTextBox Filter { get; private set; }
        internal ComboBox FilterModeCombobox { get; private set; }
        public WkLevelFilterControl WkLevelFilter { get; private set; }
        public JlptLevelFilterControl JlptLevelFilter { get; private set; }
        public TextBox RadicalNameFilter { get; private set; }
    }
}
