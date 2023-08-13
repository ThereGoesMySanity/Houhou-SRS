using System;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.AppCompat.App;
using Avalonia;
using Avalonia.Android;
using Config.Net;
using Kanji.Android.Fragments;
using Kanji.Common.Helpers;
using Kanji.Interface;
using Kanji.Interface.Actors;
using Kanji.Interface.Business;
using Kanji.Interface.Helpers;

namespace Kanji.Android;

[Activity(
    Label = "Kanji.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true)]
public class MainActivity : AppCompatActivity, IActivityResultHandler, IActivityNavigationService
{
    public Action<int, Result, Intent> ActivityResult { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public Action<int, string[], Permission[]> RequestPermissionsResult { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public event EventHandler<AndroidBackRequestedEventArgs> BackRequested;

    public MainActivity() : base(Resource.Layout.main) {}

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        // Initialize the logging system.
        LogHelper.InitializeLoggingSystem();

        // Initialize settings.
        Interface.Properties.UserSettings.Instance = new ConfigurationBuilder<Interface.Properties.IUserSettings>()
            .UseIniFile(Path.Combine(ConfigurationHelper.UserContentDirectoryPath, "UserSettings.ini"))
            .Build();

        // Initialize the configuration system.
        ConfigurationHelper.InitializeConfiguration();

        // Load the navigation actor.
        NavigationActor.Instance = new AndroidNavigationActor();

        // Start loading user resources.
        Task.WhenAll(RadicalStore.Instance.InitializeAsync(),
                SrsLevelStore.Instance.InitializeAsync());

        // Load the autostart configuration.
        AutostartBusiness.Instance.Load();

        // Start the version business.
        VersionBusiness.Initialize();

        if (savedInstanceState == null)
        {
            SupportFragmentManager.BeginTransaction().SetReorderingAllowed(true)
                .Add(Resource.Id.main_content, new MainViewFragment(), null)
                .AddToBackStack(null)
                .Commit();
        }
        SrsBusiness.Initialize();
    }
}
