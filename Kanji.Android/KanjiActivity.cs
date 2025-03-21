using System.IO;
using System.Threading.Tasks;
using Android.OS;
using AndroidX.AppCompat.App;
using Avalonia;
using Config.Net;
using Kanji.Common.Helpers;
using Kanji.Database.Dao;
using Kanji.Interface;
using Kanji.Interface.Actors;
using Kanji.Interface.Business;
using Kanji.Interface.Helpers;

namespace Kanji.Android;
public abstract class KanjiActivity : AppCompatActivity
{
    public abstract int FragmentContentId { get; }

    private static AppBuilder s_appBuilder = null;
    private readonly int contentLayoutId;

    protected KanjiActivity(int contentLayoutId) : base(contentLayoutId)
    {
        this.contentLayoutId = contentLayoutId;
    }

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        NotificationBackgroundService.ScheduleUpdate(this);

        if (s_appBuilder == null) 
        {
            s_appBuilder = AppBuilder.Configure<App>().UseAndroid().SetupWithoutStarting();

            ConfigurationHelper.Instance = new AndroidConfigurationHelper(this);

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

            SrsBusiness.Initialize();
        }
        SetContentView(contentLayoutId);
        (NavigationActor.Instance as AndroidNavigationActor).Activity = this;
    }
}