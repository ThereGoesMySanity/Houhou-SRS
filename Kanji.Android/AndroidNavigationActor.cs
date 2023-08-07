using System.Threading.Tasks;
using Android.Content;
using Avalonia.Controls;
using Kanji.Android.Activities;
using Kanji.Android.Extensions;
using Kanji.Database.Entities;
using Kanji.Interface.Actors;
using Kanji.Interface.Models;
using Kanji.Interface.Utilities;
using Kanji.Interface.ViewModels;

namespace Kanji.Android;
public class AndroidNavigationActor : NotifyPropertyChanged, INavigationActor
{
    private NavigationPageEnum _currentPage;

    /// <summary>
    /// Gets the currently active page.
    /// </summary>
    public NavigationPageEnum CurrentPage
    {
        get { return _currentPage; }
        private set
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
    public ContentControl MainWindow { get; private set; }

    /// <summary>
    /// Gets or sets the current modal window.
    /// </summary>
    public ContentControl ActiveWindow { get; set; }

    internal Context Context { get; set; }

    public void CloseMainWindow()
    {
        throw new System.NotImplementedException();
    }

    public void Navigate(NavigationPageEnum page)
    {
        if (page != CurrentPage)
        {
            var intent = new Intent(Context, page.GetActivity());
            Context.StartActivity(intent);
        }
    }

    public void NavigateToKanji(KanjiWritingCharacter character)
        => NavigateToKanji(character.Kanji, character.OriginalVocab.KanjiWriting);
    public void NavigateToKanji(KanjiEntity kanji, string filterText)
    {
        var intent = new Intent(Context, typeof(KanjiPageActivity));
        intent.PutExtra("kanji", kanji.Character);
        intent.PutExtra("kanjiFilter", filterText);
        Context.StartActivity(intent);
    }

    public void NavigateToReviewSession()
    {
        var intent = new Intent(Context, typeof(SrsPageActivity));
        intent.PutExtra("startReviews", true);
        Context.StartActivity(intent);
    }

    public void NavigateToSettings(SettingsCategoryEnum page)
    {
        var intent = new Intent(Context, typeof(SettingsPageActivity));
        intent.PutExtra("settingsPage", page.ToString());
        Context.StartActivity(intent);
    }

    public void NavigateToStartPage()
    {
        Context.StartActivity(typeof(MainActivity));
    }

    public void OpenMainWindow()
    {
        throw new System.NotImplementedException();
    }

    public void OpenOrFocus()
    {
        throw new System.NotImplementedException();
    }

    public Task<SrsEntryEditedEventArgs> OpenSrsEditWindow(SrsEntry entry)
    {
        throw new System.NotImplementedException();
    }

    public void SendMainWindowCloseEvent()
    {
        throw new System.NotImplementedException();
    }

    public void SetMainWindow(ContentControl window)
    {
        throw new System.NotImplementedException();
    }
}