using Android.OS;
using Kanji.Interface.Actors;
using Fragment = AndroidX.Fragment.App.Fragment;

namespace Kanji.Android.Fragments;
public abstract class NavigationFragment : Fragment
{
    protected AndroidNavigationActor Actor => NavigationActor.Instance as AndroidNavigationActor;

    public override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
    }
    public override void OnStart()
    {
        base.OnStart();
    }
    public override void OnStop()
    {
        base.OnStop();
    }
}
