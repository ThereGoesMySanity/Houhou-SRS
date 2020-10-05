using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;





using Avalonia.Controls;
using Avalonia.Markup.Xaml;




using Kanji.Interface.ViewModels;
using Kanji.Interface.Models;

namespace Kanji.Interface.Controls
{
    public partial class SrsEntryList : UserControl
    {
        public SrsEntryList()
        {
            InitializeComponent();
            SrsList.SelectionChanged += SrsList_SelectionChanged;
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            SrsList = this.FindControl<DataGrid>("SrsList");
        }
        public DataGrid SrsList { get; private set; }


        void SrsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            SrsEntryListViewModel vm = (SrsEntryListViewModel)DataContext;
            vm.SetSelection(SrsList.SelectedItems);
            vm.RefreshSelection();
        }
    }
}
