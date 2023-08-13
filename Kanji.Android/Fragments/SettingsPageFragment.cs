using System;
using Android.OS;
using Android.Views;
using Avalonia.Android;
using Kanji.Interface.Models;
using Kanji.Interface.Views;

namespace Kanji.Android.Fragments;
public class SettingsPageFragment : NavigationFragment
{
    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        Bundle args = RequireArguments();
        var content = new KanjiPage();
        if (args.ContainsKey("settingsPage") && Enum.TryParse(args.GetString("settingsPage"), out SettingsCategoryEnum page))
        {
            Actor.SettingsVm.Navigate(page);
        }
        return new AvaloniaView(Context) {
            Content = content
        };
    }
}