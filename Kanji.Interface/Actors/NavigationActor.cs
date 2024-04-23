using System.Threading.Tasks;
using Avalonia.Controls;
using Kanji.Database.Entities;
using Kanji.Interface.Extensions;
using Kanji.Interface.Models;
using Kanji.Interface.Utilities;
using Kanji.Interface.ViewModels;
using Kanji.Interface.Views;

namespace Kanji.Interface.Actors;

public abstract class NavigationActor : NotifyPropertyChanged, INavigationActor
{
    /// <summary>
    /// Gets or sets the singleton instance of this actor.
    /// </summary>
    public static INavigationActor Instance { get; set; }


    #region Fields

    private NavigationPageEnum _currentPage;

    #endregion

    #region Property

    /// <summary>
    /// Gets the currently active page.
    /// </summary>
    public NavigationPageEnum CurrentPage
    {
        get { return _currentPage; }
        protected set
        {
            if (value != _currentPage)
            {
                _currentPage = value;
                RaisePropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the reference to the kanji view model
    /// to enable kanji navigation.
    /// </summary>
    public KanjiViewModel KanjiVm { get; set; }

    /// <summary>
    /// Gets or sets the reference to the SRS view model
    /// to enable SRS module navigation.
    /// </summary>
    public SrsViewModel SrsVm { get; set; }

    /// <summary>
    /// Gets or sets the reference to the Settings view model
    /// to enable settings page navigation.
    /// </summary>
    public SettingsViewModel SettingsVm { get; set; }

    /// <summary>
    /// Gets or sets a reference to the main window.
    /// </summary>
    public ContentControl MainWindow { get; protected set; }

    /// <summary>
    /// Gets or sets the current modal window.
    /// </summary>
    public virtual ContentControl ActiveWindow { get; set; }

    public TopLevel TopLevel => TopLevel.GetTopLevel(ActiveWindow ?? MainWindow);

    #endregion

    #region Constructors

    public NavigationActor()
    {
        CurrentPage = NavigationPageEnum.Home;
    }

    #endregion

    #region Methods

    #region Public

    /// <summary>
    /// Navigates to the page referred by the given enum value.
    /// </summary>
    /// <param name="page">Page enum value.</param>
    public void Navigate(NavigationPageEnum page)
    {
        if (CurrentPage == page)
            return;

        CurrentPage = page;
    }

    /// <summary>
    /// Navigates to the page referred by the start page setting.
    /// </summary>
    public void NavigateToStartPage()
    {
        Navigate(Properties.UserSettings.Instance.StartPage.ToNavigationPage());
    }

    /// <summary>
    /// Navigates to the SRS page and starts a review session.
    /// </summary>
    public async void NavigateToReviewSession()
    {
        CurrentPage = NavigationPageEnum.Srs;
        await SrsVm.StartReviewSession();
    }

    /// <summary>
    /// Navigates to the kanji page, and performs an intra-navigation
    /// to the kanji referred by the given character.
    /// </summary>
    /// <param name="character">Character driving the navigation.</param>
    public void NavigateToKanji(KanjiWritingCharacter character)
        => NavigateToKanji(character.Kanji, character.OriginalVocab.KanjiWriting);
    public void NavigateToKanji(KanjiEntity kanji, string filterText)
    {
        CurrentPage = NavigationPageEnum.Kanji;
        KanjiVm.Navigate(kanji, filterText);
    }

    /// <summary>
    /// Navigates to the settings page, and performs an intra-navigation
    /// to the specified settings page.
    /// </summary>
    /// <param name="page">Page to navigate to.</param>
    public void NavigateToSettings(SettingsCategoryEnum page)
    {
        CurrentPage = NavigationPageEnum.Settings;
        SettingsVm.Navigate(page);
    }

    public abstract Task<SrsEntryEditedEventArgs> OpenSrsEditWindow(SrsEntry entry);
    public abstract Task<SrsReviewViewModel> OpenReviewSession();

    public abstract void SetMainWindow(ContentControl control);

    public abstract void SendMainWindowCloseEvent();

    #endregion

    #endregion
}