using System.Threading.Tasks;
using AndroidX.AppCompat.App;
using Avalonia.Android;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Kanji.Android.Fragments;
using Kanji.Database.Entities;
using Kanji.Interface;
using Kanji.Interface.Actors;
using Kanji.Interface.Models;
using Kanji.Interface.ViewModels;
using Kanji.Interface.Views;

namespace Kanji.Android;
public class AndroidNavigationActor : NavigationActor
{
    internal KanjiActivity Activity { get; set; }

    public override ContentControl ActiveWindow => (Activity.SupportFragmentManager.FindFragmentById(Activity.FragmentContentId).View as AvaloniaView)?.Content as ContentControl;

    public AndroidNavigationActor(KanjiActivity activity)
    {
        CurrentPage = NavigationPageEnum.Home;
        Activity = activity;
    }

    public override Task<SrsEntryEditedEventArgs> OpenSrsEditWindow(SrsEntry entry)
    {
        TaskCompletionSource<SrsEntryEditedEventArgs> task = new();

        var transaction = Activity.SupportFragmentManager.BeginTransaction().SetReorderingAllowed(true)
            .Replace(Activity.FragmentContentId, new SrsEditFragment(entry, task));
        
        if (Activity is MainActivity)
            transaction = transaction.AddToBackStack(null);

        transaction.Commit();

        return task.Task;
    }

    public override Task<SrsReviewViewModel> OpenReviewSession()
    {
        SrsReviewViewModel viewModel = new();

        Activity.SupportFragmentManager.BeginTransaction().SetReorderingAllowed(true)
            .Replace(Activity.FragmentContentId, new SrsReviewNativeFragment(viewModel))
            .AddToBackStack(null).Commit();
        
        return Task.FromResult(viewModel);
    }

    public override void SetMainWindow(ContentControl window)
    {
        MainWindow = window;
        ActiveWindow = window;
    }

    public override void CreateMainWindow()
    {
        SetMainWindow(new MainView());
        (App.Current.ApplicationLifetime as ISingleViewApplicationLifetime).MainView = MainWindow;
    }

    public override void OpenMainWindow()
    {
        throw new System.NotImplementedException();
    }

    public override void OpenOrFocus()
    {
        throw new System.NotImplementedException();
    }
}