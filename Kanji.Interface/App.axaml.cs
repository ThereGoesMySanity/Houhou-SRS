using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Kanji.Interface.Actors;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;


namespace Kanji.Interface
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                NavigationActor.Instance.SetMainWindow(new MainWindow());
                desktop.MainWindow = NavigationActor.Instance.MainWindow;
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}
