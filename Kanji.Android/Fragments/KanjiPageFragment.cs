using Android.App;
using Android.OS;
using Android.Views;
using Avalonia.Android;
using Kanji.Database.Dao;
using Kanji.Interface.Views;

namespace Kanji.Android.Fragments;
public class KanjiPageFragment : NavigationFragment
{
    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        Bundle args = RequireArguments();
        var content = new KanjiPage();
        if (args.ContainsKey("kanji"))
        {
            var kanji = args.GetString("kanji");
            var filter = args.GetString("kanjiFilter") ?? "";
            Actor.KanjiVm.Navigate(new KanjiDao().GetFirstMatchingKanji(kanji).Result, filter);
        }
        return new AvaloniaView(Context) {
            Content = content
        };
    }
}