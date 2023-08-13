using System.Threading.Tasks;
using Android.App;
using Android.Content;
using AndroidX.AppCompat.App;
using Avalonia.Controls;
using Kanji.Database.Entities;
using Kanji.Interface.Actors;
using Kanji.Interface.Models;
using Kanji.Interface.Utilities;
using Kanji.Interface.ViewModels;

namespace Kanji.Android;
public class AndroidNavigationActor : NavigationActor
{
    internal AppCompatActivity Activity { get; set; }

    public AndroidNavigationActor()
    {
        CurrentPage = NavigationPageEnum.Home;
    }

    public override Task<SrsEntryEditedEventArgs> OpenSrsEditWindow(SrsEntry entry)
    {
        throw new System.NotImplementedException();
    }

    public override void SendMainWindowCloseEvent()
    {
        throw new System.NotImplementedException();
    }

    public override void SetMainWindow(ContentControl window)
    {
        throw new System.NotImplementedException();
    }
}