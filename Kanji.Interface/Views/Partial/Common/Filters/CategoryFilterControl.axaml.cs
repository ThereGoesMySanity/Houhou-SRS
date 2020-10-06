using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;










namespace Kanji.Interface.Controls
{
    public partial class CategoryFilterControl : UserControl
    {
        public CategoryFilterControl()
        {
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            ComboBox = this.FindControl<ComboBox>("ComboBox");
        }
        public ComboBox ComboBox { get; private set; }
    }
}
