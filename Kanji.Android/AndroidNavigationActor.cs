using System.Threading.Tasks;
using AndroidX.AppCompat.App;
using Avalonia.Controls;
using Kanji.Android.Fragments;
using Kanji.Database.Entities;
using Kanji.Interface.Actors;
using Kanji.Interface.Models;
using Kanji.Interface.ViewModels;

namespace Kanji.Android;
public class AndroidNavigationActor : NavigationActor
{
    internal AppCompatActivity Activity { get; set; }

    public AndroidNavigationActor(AppCompatActivity activity)
    {
        CurrentPage = NavigationPageEnum.Home;
        Activity = activity;
    }

    public override async Task<SrsEntryEditedEventArgs> OpenSrsEditWindow(SrsEntry entry)
    {
        TaskCompletionSource<SrsEntryEditedEventArgs> task = new();

        Activity.SupportFragmentManager.BeginTransaction().SetReorderingAllowed(true)
            .Replace(Resource.Id.main_content, new SrsEditFragment(entry, task))
            .AddToBackStack(null).Commit();
        
        var result = await task.Task;
        return result;
    }

    public override Task<SrsReviewViewModel> OpenReviewSession()
    {
        SrsReviewViewModel viewModel = new();

        Activity.SupportFragmentManager.BeginTransaction().SetReorderingAllowed(true)
            .Replace(Resource.Id.main_content, new SrsReviewNativeFragment(viewModel))
            .AddToBackStack(null).Commit();
        
        return Task.FromResult(viewModel);
    }

    public override void SendMainWindowCloseEvent()
    {
        throw new System.NotImplementedException();
    }

    public override void SetMainWindow(ContentControl window)
    {
        MainWindow = window;
        ActiveWindow = window;
    }
}