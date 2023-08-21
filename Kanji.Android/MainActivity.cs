using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Kanji.Android.Fragments;
using Kanji.Interface.Actors;

namespace Kanji.Android;

[Activity(
    Label = "Houhou",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : KanjiActivity
{
    public MainActivity() : base(Resource.Layout.main) {}

    public override int FragmentContentId => Resource.Id.main_content;

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        if (savedInstanceState == null)
        {
            SupportFragmentManager.BeginTransaction().SetReorderingAllowed(true)
                .Add(Resource.Id.main_content, new MainViewFragment(), null)
                .Commit();
        }
    }
}
