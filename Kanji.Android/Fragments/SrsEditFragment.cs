using System.Threading.Tasks;
using Android.OS;
using Android.Views;
using AndroidX.Work;
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
    private readonly TaskCompletionSource<SrsEntryEditedEventArgs> resultTask;

    public SrsEditFragment(SrsEntry entry, TaskCompletionSource<SrsEntryEditedEventArgs> resultTask)
    {
        DataContext = new SrsEntryViewModel(entry);
        DataContext.FinishedEditing += OnFinishedEditing;
        this.resultTask = resultTask;
    }
    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        var view = new EditSrsEntry()
        {
            DataContext = this.DataContext
        };
        return new AvaloniaView(Context) {
            Content = view
        };
    }

    #region Methods

    /// <summary>
    /// Event callback designed to close the window when the edition is over.
    /// </summary>
    private void OnFinishedEditing(object sender, SrsEntryEditedEventArgs e)
    {
        SetResult(e);
        ParentFragmentManager.PopBackStack();
    }

    private void SetResult(SrsEntryEditedEventArgs e)
    {
        if (e.SrsEntry != null)
        {
            e.SrsEntry.Meanings = MultiValueFieldHelper.Trim(e.SrsEntry.Meanings);
            e.SrsEntry.Readings = MultiValueFieldHelper.Trim(e.SrsEntry.Readings);
            e.SrsEntry.Tags = MultiValueFieldHelper.Trim(e.SrsEntry.Tags);
        }
        resultTask.SetResult(e);
    }

    /// <summary>
    /// Disposes what needs be when the window is closed.
    /// </summary>
    public override void OnDestroyView()
    {
        base.OnDestroyView();
        if (!resultTask.Task.IsCompleted) SetResult(new SrsEntryEditedEventArgs(DataContext.Entry, false));
        DataContext.FinishedEditing -= OnFinishedEditing;
        DataContext.Dispose();
        DataContext = null;
    }
    #endregion
}