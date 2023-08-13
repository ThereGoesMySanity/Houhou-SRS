using Android.OS;
using Android.Views;
using AndroidX.Fragment.App;
using Avalonia.Android;
using Kanji.Interface.Views;

namespace Kanji.Android.Fragments;
public class MainViewFragment : Fragment
{
    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        return new AvaloniaView(Context)
        {
            Content = new MainView()
        };
    }
}