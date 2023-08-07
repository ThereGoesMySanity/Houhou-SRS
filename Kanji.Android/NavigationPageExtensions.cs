using System;
using Kanji.Android.Activities;
using Kanji.Interface.Models;

namespace Kanji.Android.Extensions;
public static class NavigationPageExtensions
{
    public static Type GetActivity(this NavigationPageEnum page)
    {
        return page switch
        {
            NavigationPageEnum.Home => typeof(MainActivity),

            NavigationPageEnum.Kanji => typeof(KanjiPageActivity),
            NavigationPageEnum.Vocab => typeof(VocabPageActivity),
            NavigationPageEnum.Settings => typeof(SettingsPageActivity),
            NavigationPageEnum.Srs => typeof(SrsPageActivity),

            _ => null,
        };
    }
}