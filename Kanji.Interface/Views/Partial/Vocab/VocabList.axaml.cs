using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;






using Avalonia.Controls;
using Avalonia.Markup.Xaml;



using Kanji.Interface.ViewModels;

namespace Kanji.Interface.Controls
{
    public partial class VocabList : UserControl
    {
        public VocabList()
        {
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            VocabListBox = this.FindControl<ItemsControl>("VocabListBox");
        }
        public ItemsControl VocabListBox { get; private set; }
    }
}
