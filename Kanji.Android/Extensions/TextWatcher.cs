using System;
using Android.Text;
using Java.Lang;

namespace Kanji.Android.Extensions;
public class BindingTextWatcher : Java.Lang.Object, ITextWatcher
{
    private readonly Func<string> getter;
    private readonly Action<string> setter;

    public BindingTextWatcher(Func<string> getter, Action<string> setter)
    {
        this.getter = getter;
        this.setter = setter;
    }
    public void AfterTextChanged(IEditable s)
    {
        var current = getter();
        if (s.ToString() != current) s.Replace(0, s.Length(), current);
    }

    public void BeforeTextChanged(ICharSequence s, int start, int count, int after)
    {
    }

    public void OnTextChanged(ICharSequence s, int start, int before, int count)
    {
        setter(s.ToString());
    }
}