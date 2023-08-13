using System;
using Kanji.Android.Fragments;
using Kanji.Interface.Models;

namespace Kanji.Android.Extensions;
public static class NavigationPageExtensions
{
    public static Type GetActivity(this NavigationPageEnum page)
    {
        return page switch
        {
            NavigationPageEnum.Home => typeof(MainActivity),

            NavigationPageEnum.Kanji => typeof(KanjiPageFragment),
            NavigationPageEnum.Vocab => typeof(VocabPageFragment),
            NavigationPageEnum.Settings => typeof(SettingsPageFragment),
            NavigationPageEnum.Srs => typeof(SrsPageFragment),

            _ => null,
        };
    }
}