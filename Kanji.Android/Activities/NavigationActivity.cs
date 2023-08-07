using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.AppCompat.App;
using Avalonia.Android;
using Kanji.Interface.Actors;

namespace Kanji.Android.Activities;
public abstract class NavigationActivity : AppCompatActivity, IActivityResultHandler, IActivityNavigationService
{
    public Action<int, Result, Intent> ActivityResult { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public Action<int, string[], Permission[]> RequestPermissionsResult { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public event EventHandler<AndroidBackRequestedEventArgs> BackRequested;

    protected AndroidNavigationActor Actor => NavigationActor.Instance as AndroidNavigationActor;

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
    }
    protected override void OnStart()
    {
        base.OnStart();
        Actor.Context = this;
    }
    protected override void OnStop()
    {
        base.OnStop();
        if (Actor.Context == this)
            Actor.Context = null;
    }
}
