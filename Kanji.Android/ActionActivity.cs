using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Kanji.Interface.Actors;

namespace Kanji.Android;

[Activity(
    Label = "Houhou",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    Exported = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
[IntentFilter(new[] { Intent.ActionProcessText, Intent.ActionDefine }, Categories = new[] { Intent.CategoryDefault }, DataMimeType = "text/plain")]
public class ActionActivity : KanjiActivity
{
    public ActionActivity() : base(Resource.Layout.main)
    {
    }

    public override int FragmentContentId => Resource.Id.main_content;

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        string item = Intent.Action switch
        {
            Intent.ActionProcessText =>
                item = Intent.GetStringExtra(Intent.ExtraProcessText),
            Intent.ActionDefine =>
                item = Intent.GetStringExtra(Intent.ExtraText),
            _ => null,
        };
        if (item != null)
        {
            NavigationActor.Instance.OpenSrsEditWindow(new Database.Entities.SrsEntry() {
                 AssociatedVocab = item
            }).ContinueWith(t => Finish());
        }
        else
        {
            Finish();
        }
    }
}