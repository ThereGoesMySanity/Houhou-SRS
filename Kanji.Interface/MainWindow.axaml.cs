using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Kanji.Interface;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        // Initialize the components.
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}