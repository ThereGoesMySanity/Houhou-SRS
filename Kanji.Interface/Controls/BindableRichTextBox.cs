// Credits to Leom Burke:
// http://www.codeproject.com/Articles/137209/Binding-and-styling-text-to-a-RichTextBox-in-WPF
// Thanks man.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;




namespace Kanji.Interface.Controls
{
    //class BindableRichTextBox : RichTextBox
    //{
    //    public static readonly AvaloniaProperty DocumentProperty =
    //        AvaloniaProperty.Register("Document", typeof(FlowDocument),
    //        typeof(BindableRichTextBox), new FrameworkPropertyMetadata
    //        (null, new PropertyChangedCallback(OnDocumentChanged)));

    //    public new FlowDocument Document
    //    {
    //        get
    //        {
    //            return (FlowDocument)this.GetValue(DocumentProperty);
    //        }

    //        set
    //        {
    //            this.SetValue(DocumentProperty, value);
    //        }
    //    }

    //    public static void OnDocumentChanged(AvaloniaObject obj,
    //        AvaloniaPropertyChangedEventArgs args)
    //    {
    //        RichTextBox rtb = (RichTextBox)obj;
    //        rtb.Dispatcher.Invoke(() =>
    //        {
    //            rtb.Document = ((FlowDocument)args.NewValue);
    //        });
    //    }
    //}
    //TODO
}
