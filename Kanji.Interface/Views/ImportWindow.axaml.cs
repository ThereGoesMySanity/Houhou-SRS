using System;
using Avalonia.Controls;
using Kanji.Interface.ViewModels;

namespace Kanji.Interface.Views;

public partial class ImportWindow : Window
{
    public ImportWindow()
    {
        InitializeComponent();
        ImportViewModel vm = new ImportViewModel();
        DataContext = vm;
        vm.Finished += OnImportFinished;
    }
    private void OnImportFinished(object sender, EventArgs e)
    {
        Close();
    }
}
