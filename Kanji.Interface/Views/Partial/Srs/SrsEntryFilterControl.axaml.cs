using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;








using Avalonia.Controls;
using Avalonia.Markup.Xaml;


namespace Kanji.Interface.Controls
{
    public partial class SrsEntryFilterControl : UserControl
    {
        public SrsEntryFilterControl()
        {
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            MeaningFilter = this.FindControl<SrsEntryMeaningFilterControl>("MeaningFilter");
            ReadingFilter = this.FindControl<SrsEntryReadingFilterControl>("ReadingFilter");
            TagFilter = this.FindControl<SrsEntryTagsFilterControl>("TagFilter");
        }
        public SrsEntryMeaningFilterControl MeaningFilter { get; private set; }
        public SrsEntryReadingFilterControl ReadingFilter { get; private set; }
        public SrsEntryTagsFilterControl TagFilter { get; private set; }
    }
}
