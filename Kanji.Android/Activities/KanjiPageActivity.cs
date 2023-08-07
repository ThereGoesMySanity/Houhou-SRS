using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.AppCompat.App;
using Avalonia.Android;
using Kanji.Database.Dao;
using Kanji.Interface.Actors;
using Kanji.Interface.ViewModels;
using Kanji.Interface.Views;

namespace Kanji.Android.Activities;
[Activity(Label = "@string/kanjiPage")]
public class KanjiPageActivity : NavigationActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        var content = new KanjiPage();
        if (Intent.Extras?.ContainsKey("kanji") ?? false)
        {
            var kanji = Intent.Extras.GetString("kanji");
            var filter = Intent.Extras.GetString("kanjiFilter") ?? "";
            Actor.KanjiVm.Navigate(new KanjiDao().GetFirstMatchingKanji(kanji).Result, filter);
        }

        SetContentView(new AvaloniaView(this) {
            Content = content
        });
    }
}