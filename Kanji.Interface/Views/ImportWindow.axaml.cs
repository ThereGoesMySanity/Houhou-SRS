using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Kanji.Interface.ViewModels;

namespace Kanji.Interface.Views
{
    public partial class ImportWindow : Window
    {
        public ImportWindow()
        {
            InitializeComponent();
            ImportViewModel vm = new ImportViewModel();
            DataContext = vm;
            vm.Finished += OnImportFinished;
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnImportFinished(object sender, EventArgs e)
        {
            Close();
        }
    }
}
