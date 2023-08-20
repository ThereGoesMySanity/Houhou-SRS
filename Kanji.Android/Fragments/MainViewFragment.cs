using Android.OS;
using Android.Views;
using AndroidX.Fragment.App;
using Avalonia.Android;
using Kanji.Interface.Actors;
using Kanji.Interface.Views;

namespace Kanji.Android.Fragments;
public class MainViewFragment : Fragment
{
    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        NavigationActor.Instance.SetMainWindow(new MainView());
        return new AvaloniaView(Context)
        {
            Content = NavigationActor.Instance.MainWindow
        };
    }
}