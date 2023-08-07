using Android.OS;
using Avalonia.Android;
using Kanji.Interface.Views;

namespace Kanji.Android.Activities;

public class SrsEditActivity : NavigationActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        var content = new EditSrsEntryWindow();
        if (Intent.Extras?.ContainsKey("srsEntry") ?? false)
        {
            
        }

        SetContentView(new AvaloniaView(this) {
            Content = content
        });
    }
}