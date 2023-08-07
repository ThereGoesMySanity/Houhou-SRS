using System.Threading.Tasks;
using Avalonia.Controls;
using Kanji.Database.Entities;
using Kanji.Interface.Models;
using Kanji.Interface.ViewModels;

namespace Kanji.Interface.Actors;

public interface INavigationActor
{

    NavigationPageEnum CurrentPage { get; }

    /// <summary>
    /// Gets or sets the reference to the kanji view model
    /// to enable kanji navigation.
    /// </summary>
    KanjiViewModel KanjiVm { get; set; }

    /// <summary>
    /// Gets or sets the reference to the SRS view model
    /// to enable SRS module navigation.
    /// </summary>
    SrsViewModel SrsVm { get; set; }

    /// <summary>
    /// Gets or sets the reference to the Settings view model
    /// to enable settings page navigation.
    /// </summary>
    SettingsViewModel SettingsVm { get; set; }

    /// <summary>
    /// Gets or sets a reference to the main window.
    /// </summary>
    ContentControl MainWindow { get; }

    /// <summary>
    /// Gets or sets the current modal window.
    /// </summary>
    ContentControl ActiveWindow { get; set; }

    void Navigate(NavigationPageEnum page);

    void NavigateToStartPage();

    void NavigateToReviewSession();

    void NavigateToKanji(KanjiEntity character, string filterText);
    void NavigateToKanji(KanjiWritingCharacter character);

    void NavigateToSettings(SettingsCategoryEnum page);

    void OpenMainWindow();
    void CloseMainWindow();
    void OpenOrFocus();
    void SendMainWindowCloseEvent();

    Task<SrsEntryEditedEventArgs> OpenSrsEditWindow(SrsEntry entry);

    void SetMainWindow(ContentControl window);

}