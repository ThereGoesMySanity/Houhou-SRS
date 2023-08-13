using Android.App;
using Android.OS;
using Android.Views;
using Avalonia.Android;
using Kanji.Interface.Views;

namespace Kanji.Android.Fragments;
public class VocabPageFragment : NavigationFragment
{
    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        var content = new VocabPage();
        return new AvaloniaView(Context) {
            Content = content
        };
    }
}