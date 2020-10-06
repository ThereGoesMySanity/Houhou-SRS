// Thanks Neutrino
// http://stackoverflow.com/questions/942548/setting-a-property-with-an-eventtrigger

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Avalonia.Controls;

namespace Kanji.Interface.Utilities
{
    /// <summary>
    /// Sets the designated property to the supplied value. TargetObject
    /// optionally designates the object on which to set the property. If
    /// TargetObject is not supplied then the property is set on the object
    /// to which the trigger is attached.
    /// </summary>
    //public class SetPropertyAction : TriggerAction<Control>
    //{
    //    // PropertyName AvaloniaProperty.

    //    /// <summary>
    //    /// The property to be executed in response to the trigger.
    //    /// </summary>
    //    public string PropertyName
    //    {
    //        get { return (string)GetValue(PropertyNameProperty); }
    //        set { SetValue(PropertyNameProperty, value); }
    //    }

    //    public static readonly AvaloniaProperty PropertyNameProperty
    //        = AvaloniaProperty.Register("PropertyName", typeof(string),
    //        typeof(SetPropertyAction));


    //    // PropertyValue AvaloniaProperty.

    //    /// <summary>
    //    /// The value to set the property to.
    //    /// </summary>
    //    public object PropertyValue
    //    {
    //        get { return GetValue(PropertyValueProperty); }
    //        set { SetValue(PropertyValueProperty, value); }
    //    }

    //    public static readonly AvaloniaProperty PropertyValueProperty
    //        = AvaloniaProperty.Register("PropertyValue", typeof(object),
    //        typeof(SetPropertyAction));


    //    // TargetObject AvaloniaProperty.

    //    /// <summary>
    //    /// Specifies the object upon which to set the property.
    //    /// </summary>
    //    public object TargetObject
    //    {
    //        get { return GetValue(TargetObjectProperty); }
    //        set { SetValue(TargetObjectProperty, value); }
    //    }

    //    public static readonly AvaloniaProperty TargetObjectProperty
    //        = AvaloniaProperty.Register("TargetObject", typeof(object),
    //        typeof(SetPropertyAction));


    //    // Private Implementation.

    //    protected override void Invoke(object parameter)
    //    {
    //        object target = TargetObject ?? AssociatedObject;
    //        PropertyInfo propertyInfo = target.GetType().GetProperty(
    //            PropertyName,
    //            BindingFlags.Instance|BindingFlags.Public
    //            |BindingFlags.NonPublic|BindingFlags.InvokeMethod);

    //        propertyInfo.SetValue(target, PropertyValue);
    //    }
    //}
}
