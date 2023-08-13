using Android.OS;
using Android.Views;
using Avalonia.Android;
using Kanji.Database.Dao;
using Kanji.Database.Entities;
using Kanji.Database.Helpers;
using Kanji.Interface.Models;
using Kanji.Interface.ViewModels;
using Kanji.Interface.Views;
using Newtonsoft.Json;

namespace Kanji.Android.Fragments;

public class SrsEditFragment : NavigationFragment
{
    private SrsEntryViewModel DataContext;
    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        Bundle args = RequireArguments();
        SrsEntry entry = new();
        if (args.ContainsKey("srsEntry"))
        {
            entry = new SrsEntryDao().GetItem(args.GetLong("srsEntry")).GetAwaiter().GetResult();
        }
        else if (args.ContainsKey("associatedKanji") || args.ContainsKey("associatedVocab"))
        {
            entry = new SrsEntryDao().GetSimilarItem(new SrsEntry{
                AssociatedKanji = args.GetString("associatedKanji"),
                AssociatedVocab = args.GetString("associatedVocab"),
            }).GetAwaiter().GetResult();
        }
        DataContext = new SrsEntryViewModel(entry);
        DataContext.FinishedEditing += OnFinishedEditing;
        var content = new EditSrsEntry()
        {
            DataContext = this.DataContext
        };
        return new AvaloniaView(Context) {
            Content = content
        };
    }

    #region Methods

    /// <summary>
    /// Event callback designed to close the window when the edition is over.
    /// </summary>
    private void OnFinishedEditing(object sender, SrsEntryEditedEventArgs e)
    {
        if (e.SrsEntry != null)
        {
            e.SrsEntry.Meanings = MultiValueFieldHelper.Trim(e.SrsEntry.Meanings);
            e.SrsEntry.Readings = MultiValueFieldHelper.Trim(e.SrsEntry.Readings);
            e.SrsEntry.Tags = MultiValueFieldHelper.Trim(e.SrsEntry.Tags);
        }
        Bundle result = new();
        result.PutString("result", JsonConvert.SerializeObject(e));
        ParentFragmentManager.SetFragmentResult("srsEntryEdited", result);
        ParentFragmentManager.BeginTransaction().SetReorderingAllowed(true)
            .Remove(this).Commit();
    }

    /// <summary>
    /// Disposes what needs be when the window is closed.
    /// </summary>
    public override void OnDestroyView()
    {
        base.OnDestroyView();
        DataContext.FinishedEditing -= OnFinishedEditing;
        DataContext.Dispose();
        DataContext = null;
    }
    #endregion
}