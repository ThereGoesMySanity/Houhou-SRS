using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Kanji.Interface.Actors;

namespace Kanji.Interface;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        // Initialize the components.
        InitializeComponent();
        this.AttachDevTools();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        NavigationActor.Instance.SendMainWindowCloseEvent();
    }
}