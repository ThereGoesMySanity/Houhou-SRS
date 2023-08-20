using System;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.AppCompat.App;
using AndroidX.Fragment.App;
using Avalonia;
using Avalonia.Android;
using Config.Net;
using Kanji.Android.Fragments;
using Kanji.Common.Helpers;
using Kanji.Database.Dao;
using Kanji.Interface;
using Kanji.Interface.Actors;
using Kanji.Interface.Business;
using Kanji.Interface.Helpers;
using Kanji.Interface.Models;
using Kanji.Interface.Views;
using Newtonsoft.Json;

namespace Kanji.Android;

[Activity(
    Label = "Kanji.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AppCompatActivity
{
    private static AppBuilder s_appBuilder = null;
    public MainActivity() : base(Resource.Layout.main) {}

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        if (s_appBuilder == null) 
        {
            s_appBuilder = AppBuilder.Configure<App>().UseAndroid().SetupWithoutStarting();

            // Initialize the logging system.
            LogHelper.InitializeLoggingSystem();


            ConfigurationHelper.Instance = new AndroidConfigurationHelper(this);

            // Initialize settings.
            Interface.Properties.UserSettings.Instance = new ConfigurationBuilder<Interface.Properties.IUserSettings>()
                .UseIniFile(Path.Combine(ConfigurationHelper.Instance.UserContentDirectoryPath, "UserSettings.ini"))
                .Build();

            // Initialize the configuration system.
            ConfigurationHelper.Instance.InitializeConfiguration();
            DaoConnection.Instance = new DaoConnection(ConfigurationHelper.Instance.CommonDataDictionaryDatabaseFilePath, ConfigurationHelper.Instance.UserContentSrsDatabaseFilePath);

            // Load the navigation actor.
            NavigationActor.Instance = new AndroidNavigationActor(this);

            MessageBoxActor.Instance = new AndroidMessageBoxActor();

            // Start loading user resources.
            Task.Run(RadicalStore.Instance.InitializeAsync).GetAwaiter().GetResult();
            Task.Run(SrsLevelStore.Instance.InitializeAsync).GetAwaiter().GetResult();

            // Load the autostart configuration.
            AutostartBusiness.Instance.Load();

            // Start the version business.
            VersionBusiness.Initialize();

            SetContentView(Resource.Layout.main);
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
}
