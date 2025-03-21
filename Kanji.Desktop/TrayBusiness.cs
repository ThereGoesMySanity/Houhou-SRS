using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using DesktopNotifications;
using Kanji.Interface.Actors;
using Kanji.Interface.Models;

namespace Kanji.Interface.Business;

internal class KanjiNotification : Notification
{
    public TrayNotificationEnum Type { get; set; }
}

public class TrayBusiness
{
    private readonly INotificationManager notificationManager;

    #region Static
    /// <summary>
    /// Gets the loaded instance.
    /// </summary>
    public static TrayBusiness? Instance { get; set; }

    #endregion

    #region Constructors

    public TrayBusiness(INotificationManager notificationManager)
    {
        this.notificationManager = notificationManager;
        // Subscribe to the main window close event to show notification if needed.
        notificationManager.NotificationActivated += OnTrayNotificationClicked;
        SrsBusiness.Instance!.PropertyChanged += OnSrsBusinessPropertyChanged;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Shows the review alert.
    /// </summary>
    private void CheckReviews()
    {
        var info = SrsBusiness.Instance?.CurrentReviewInfo;
        if (info is null || info.RecentReviewsCount == 0) return;
        

        string notificationMessage;
        if (info.AvailableReviewsCount == 1)
        {
            notificationMessage = $"1 review is available.{Environment.NewLine}Click to start reviewing.";
        }
        else
        {
            notificationMessage = 
                $"{info.AvailableReviewsCount} reviews are available. ({info.RecentReviewsCount} recent){Environment.NewLine}Click to start reviewing.";
        }

        ShowNotification(TrayNotificationEnum.ReviewNotification, "Houhou", notificationMessage, "Review");
    }

    /// <summary>
    /// Shows a balloon tip.
    /// </summary>
    /// <param name="notificationType">Notification type.</param>
    /// <param name="title">Title to show in the balloon tip.</param>
    /// <param name="message">Message to show in the balloon tip.</param>
    /// <param name="icon">Icon to show in the balloon tip.</param>
    private void ShowNotification(TrayNotificationEnum notificationType, string title, string message, string? button = null)
    {
        var notification = new KanjiNotification()
        {
            Title = title,
            Body = message,
            Type = notificationType,
        };
        if (button is not null) notification.Buttons.Add((button, button));
        notificationManager.ShowNotification(notification);
    }

    /// <summary>
    /// Opens or focuses the main window.
    /// </summary>
    public void OpenOrFocusMainWindow()
    {
        (NavigationActor.Instance as DesktopNavigationActor)?.OpenOrFocus();
    }

    /// <summary>
    /// Opens or focuses the main window, navigate to the SRS page
    /// and starts a review session.
    /// </summary>
    public void StartReviewing()
    {
        (NavigationActor.Instance as DesktopNavigationActor)?.OpenOrFocus();
        NavigationActor.Instance.NavigateToReviewSession();
    }

    /// <summary>
    /// Exits the application.
    /// I mean, yeah, for real, that's what the ExitApplication method does.
    /// </summary>
    public void ExitApplication()
    {
        (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown();
    }

    #region Event handlers

    private void OnSrsBusinessPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SrsBusiness.CurrentReviewInfo))
        {
            CheckReviews();
        }
    }

    /// <summary>
    /// Event handler triggered when a tray balloon tip is clicked.
    /// Directs the user to the SRS review module.
    /// </summary>
    private void OnTrayNotificationClicked(object? sender, NotificationActivatedEventArgs e)
    {
        if (e.Notification is not KanjiNotification n) return;
        switch (n.Type)
        {
            case TrayNotificationEnum.ReviewNotification:
                StartReviewing();
                break;
            case TrayNotificationEnum.ExitNotification:
                ExitApplication();
                break;
        }
    }

    /// <summary>
    /// Event handler triggered when the main window of Houhou is closed.
    /// Issues a notification if the settings approve of that.
    /// </summary>
    public void OnMainWindowClosed(object sender, EventArgs e)
    {
        if (Properties.UserSettings.Instance.WindowCloseAction == WindowCloseActionEnum.Warn)
        {
            ShowNotification(TrayNotificationEnum.ExitNotification, "Houhou",
                string.Format("Houhou is still running in the background.{0}Click here to shut it down.", Environment.NewLine), "Close");
        }
    }

    #endregion

    #endregion
}
