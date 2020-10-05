using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;








using Avalonia.Controls;
using Avalonia.Markup.Xaml;


namespace Kanji.Interface.Controls
{
    public partial class VocabFilterControl : UserControl
    {
        public VocabFilterControl()
        {
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            ReadingFilter = this.FindControl<CommandTextBox>("ReadingFilter");
            MeaningFilter = this.FindControl<CommandTextBox>("MeaningFilter");
            CategoryFilter = this.FindControl<CategoryFilterControl>("CategoryFilter");
            WkLevelFilter = this.FindControl<WkLevelFilterControl>("WkLevelFilter");
            JlptLevelFilter = this.FindControl<JlptLevelFilterControl>("JlptLevelFilter");
            ApplyFilterButton = this.FindControl<Button>("ApplyFilterButton");
        }

        internal CommandTextBox ReadingFilter { get; private set; }
        internal CommandTextBox MeaningFilter { get; private set; }
        public CategoryFilterControl CategoryFilter { get; private set; }
        public WkLevelFilterControl WkLevelFilter { get; private set; }
        public JlptLevelFilterControl JlptLevelFilter { get; private set; }
        public Button ApplyFilterButton { get; private set; }

    }
}
