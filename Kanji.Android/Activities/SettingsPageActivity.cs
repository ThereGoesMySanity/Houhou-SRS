using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.AppCompat.App;
using Avalonia.Android;
using Kanji.Interface.Actors;
using Kanji.Interface.Models;
using Kanji.Interface.Views;

namespace Kanji.Android.Activities;
public class SettingsPageActivity : NavigationActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        var content = new SettingsPage();
        if ((Intent.Extras?.ContainsKey("settingsPage") ?? false)
                && Enum.TryParse(Intent.Extras.GetString("settingsPage"), out SettingsCategoryEnum page))
        {
            Actor.SettingsVm.Navigate(page);
        }

        SetContentView(new AvaloniaView(this) {
            Content = content
        });
    }
}