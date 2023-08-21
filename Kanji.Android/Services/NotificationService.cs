using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using AndroidX.Arch.Core.Executor;
using Java.Lang;
using Kanji.Database.Dao;
using Kanji.Interface.Business;
using Kanji.Interface.Helpers;

namespace Kanji.Android;
[BroadcastReceiver]
public class NotificationBackgroundService : BroadcastReceiver
{
    private const string CHANNEL_ID = "AvailableReviews";
    public override void OnReceive(Context context, Intent intent)
    {
        if (ConfigurationHelper.Instance == null)
        {
            ConfigurationHelper.Instance = new AndroidConfigurationHelper(context);
            ConfigurationHelper.Instance.InitializeConfiguration();
        }
        var currentData = Task.Run(() => new SrsEntryDao().GetReviewInfo()).GetAwaiter().GetResult();

        var channel = new NotificationChannel(CHANNEL_ID, CHANNEL_ID, NotificationImportance.Default)
        {
            Description = "Reviews available"
        };

        var manager = (NotificationManager)context.GetSystemService(Context.NotificationService);
        manager.CreateNotificationChannel(channel);

        if (currentData.AvailableReviewsCount > 0 && currentData.RecentReviewsCount > 0)
        {
            var intent2 = new Intent(context, typeof(MainActivity));
            intent2.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask | ActivityFlags.ClearTop);
            var pendingIntent = PendingIntent.GetActivity(context, 0, intent2, PendingIntentFlags.Immutable);

            var builder = new Notification.Builder(context, CHANNEL_ID)
                .SetSmallIcon(Resource.Drawable.nt_icon)
                .SetContentTitle("Reviews available")
                .SetContentText($"{currentData.AvailableReviewsCount} available reviews")
                .SetCategory(Notification.CategoryRecommendation)
                .SetVisibility(NotificationVisibility.Public)
                .SetOnlyAlertOnce(false)
                .SetContentIntent(pendingIntent)
                .SetAutoCancel(true);

            manager.Notify(1, builder.Build());
        }
        else if (currentData.AvailableReviewsCount == 0)
        {
            manager.Cancel(1);
        }
        ScheduleUpdate(context);
    }

    public static void ScheduleUpdate(Context context)
    {
        var alarmManager = context.GetSystemService(Context.AlarmService) as AlarmManager;
        if (alarmManager != null)
        {
            var intent = new Intent(context, typeof(NotificationBackgroundService));
            var pendingIntent = PendingIntent.GetBroadcast(context, 1, intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
            alarmManager.SetAndAllowWhileIdle(AlarmType.RtcWakeup, JavaSystem.CurrentTimeMillis() + 60 * 60 * 1000, pendingIntent);
        }
    }
}