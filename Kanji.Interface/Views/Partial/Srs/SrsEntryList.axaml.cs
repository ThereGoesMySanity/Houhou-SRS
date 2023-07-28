using System;
using Avalonia.Controls;
using Kanji.Interface.ViewModels;

namespace Kanji.Interface.Controls;

public partial class SrsEntryList : UserControl
{
    public SrsEntryList()
    {
        InitializeComponent();
        this.DataContextChanged += (object o, EventArgs args) => (DataContext as SrsEntryListViewModel).SelectedItems = SrsList.SelectedItems;
        SrsList.SelectionChanged += SrsList_SelectionChanged;
    }
    void SrsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        e.Handled = true;
        SrsEntryListViewModel vm = (SrsEntryListViewModel)DataContext;
        vm.SelectedItems = SrsList.SelectedItems;
        vm.RefreshSelection();
    }
}
