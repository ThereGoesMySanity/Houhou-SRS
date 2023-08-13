using Android.OS;
using Android.Views;
using Avalonia.Android;
using Kanji.Interface.Views;

namespace Kanji.Android.Fragments;
public class SrsPageFragment : NavigationFragment
{
    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        Bundle args = RequireArguments();
        var content = new SrsPage();
        if (args.GetBoolean("startReviewSession"))
        {
            Actor.SrsVm.StartReviewSession().GetAwaiter().GetResult();
        }
        return new AvaloniaView(Context) {
            Content = content
        };
    }
}