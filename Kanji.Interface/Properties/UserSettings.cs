using System;
using Config.Net;
using Kanji.Interface.Models;

namespace Kanji.Interface.Properties;
public static class UserSettings
{
    public static IUserSettings Instance { get; set; }
}
public interface IUserSettings
{

    [Option(DefaultValue = WindowCloseActionEnum.Warn)]
    WindowCloseActionEnum WindowCloseAction { get; set; }

    string AudioUri { get; set; }

    [Option(DefaultValue = "Default")]
    string RadicalSetName { get; set; }

    [Option(DefaultValue = "Default")]
    string SrsLevelSetName { get; set; }

    [Option(DefaultValue = KanaTypeEnum.Hiragana)]
    KanaTypeEnum KunYomiReadingType { get; set; }

    [Option(DefaultValue = KanaTypeEnum.Hiragana)]
    KanaTypeEnum NanoriReadingType { get; set; }

    [Option(DefaultValue = KanaTypeEnum.Hiragana)]
    KanaTypeEnum OnYomiReadingType { get; set; }

    [Option(DefaultValue = false)]
    bool ShowNanori { get; set; }

    [Option(DefaultValue = 4)]
    int CollapseMeaningsLimit { get; set; }

    string UserDirectoryPath { get; set; }

    [Option(DefaultValue = true)]
    bool ShowKanjiBookRanking { get; set; }

    [Option(DefaultValue = true)]
    bool ShowKanjiGrade { get; set; }

    [Option(DefaultValue = true)]
    bool ShowKanjiJlptLevel { get; set; }

    [Option(DefaultValue = true)]
    bool ShowKanjiWkLevel { get; set; }

    [Option(DefaultValue = true)]
    bool ShowKanjiStrokes { get; set; }

    [Option(DefaultValue = true)]
    bool ShowVocabBookRanking { get; set; }

    [Option(DefaultValue = true)]
    bool ShowVocabWikipediaRank { get; set; }

    [Option(DefaultValue = true)]
    bool ShowVocabJlptLevel { get; set; }

    [Option(DefaultValue = true)]
    bool ShowVocabWkLevel { get; set; }
    string WkApiKey { get; set; }

    [Option(DefaultValue = "WaniKani,WK%level%")]
    string WkTags { get; set; }

    [Option(DefaultValue = RadicalSortModeEnum.Frequency)]
    RadicalSortModeEnum RadicalSortMode { get; set; }

    [Option(DefaultValue = 40)]
    int KanjiPerPage { get; set; }

    [Option(DefaultValue = 100)]
    int AudioVolume { get; set; }

    [Option(DefaultValue = true)]
    bool AnimateStrokes { get; set; }

    [Option(DefaultValue = 1000.0)]
    double StrokeAnimationDelay { get; set; }

    [Option(DefaultValue = false)]
    bool AutoSkipReviews { get; set; }

    [Option(DefaultValue = true)]
    bool IsAutoUpdateCheckEnabled { get; set; }

    [Option(DefaultValue = true)]
    bool IsIgnoreAnswerShortcutEnabled { get; set; }

    [Option(DefaultValue = 10000)]
    int SrsEntriesPerPage { get; set; }

    [Option(DefaultValue = AudioAutoplayModeEnum.Disabled)]
    AudioAutoplayModeEnum AudioAutoplayMode { get; set; }

    [Option(DefaultValue = 24.0)]
    double VocabSrsDelayHours { get; set; }

    [Option(DefaultValue = true)]
    bool TrayShowNotifications { get; set; }

    string LastSrsTagsValue { get; set; }

    [Option(DefaultValue = 20)]
    int VocabPerPage { get; set; }

    [Option(DefaultValue = "01:00:00")]
    TimeSpan TrayCheckInterval { get; set; }

    [Option(DefaultValue = 1L)]
    long TrayNotificationCountThreshold { get; set; }

    [Option(DefaultValue = StartPageEnum.Home)]
    StartPageEnum StartPage { get; set; }
    // string UpdateCheckUri { get; set; }
}