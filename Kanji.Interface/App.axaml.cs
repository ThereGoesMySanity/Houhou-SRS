using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Kanji.Interface.Actors;
using Kanji.Interface.Business;
using Kanji.Interface.Views;
using Microsoft.VisualBasic;
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
            NavigationActor.Instance.CreateMainWindow();
            base.OnFrameworkInitializationCompleted();
        }
        public void OnExit(object? source, EventArgs args)
        {
            (Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown();
        }

        public void OnOpenOrFocus(object? source, EventArgs args)
        {
            NavigationActor.Instance.OpenOrFocus();
        }
        public void OnOpenReviews(object? source, EventArgs args)
        {
            NavigationActor.Instance.OpenOrFocus();
            NavigationActor.Instance.NavigateToReviewSession();
        }
        public void OnCheckReviews(object? source, EventArgs args)
        {
            SrsBusiness.Instance?.UpdateReviewInfoAsync();
        }
    }
}
