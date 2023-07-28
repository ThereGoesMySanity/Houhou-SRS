using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Kanji.Interface.Business;
using Kanji.Interface.ViewModels;

namespace Kanji.Interface.Views
{
    public partial class ImportWindow : Window
    {
        public ImportWindow()
        {
            InitializeComponent();
            ImportViewModel vm = new ImportViewModel();
            this.AttachDevTools();
            DataContext = vm;
            vm.Finished += OnImportFinished;
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.FindControl<ContentControl>("ImportContent").DataTemplates.AddRange(PluginsBusiness.Instance.PluginTemplates);
        }

        private void OnImportFinished(object sender, EventArgs e)
        {
            Close();
        }
    }
}
