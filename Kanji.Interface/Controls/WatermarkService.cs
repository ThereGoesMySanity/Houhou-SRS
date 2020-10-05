// Credits to John Myczek
// http://stackoverflow.com/questions/833943/watermark-hint-text-textbox-in-wpf

using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;





namespace Kanji.Interface.Controls
{
    /// <summary>
    /// Class that provides the Watermark attached property
    /// </summary>
    public static class WatermarkService
    {
        /// <summary>
        /// Watermark Attached Dependency Property
        /// </summary>
        public static readonly AvaloniaProperty WatermarkProperty = AvaloniaProperty.RegisterAttached<AvaloniaObject, AvaloniaObject, object>("Watermark"); 
        static WatermarkService()
        {
            WatermarkProperty.Changed.Subscribe(OnWatermarkChanged);
        }

        #region Private Fields

        /// <summary>
        /// Dictionary of ItemsControls
        /// </summary>
        private static readonly Dictionary<object, ItemsControl> itemsControls = new Dictionary<object, ItemsControl>();

        #endregion

        /// <summary>
        /// Gets the Watermark property.  This dependency property indicates the watermark for the control.
        /// </summary>
        /// <param name="d"><see cref="AvaloniaObject"/> to get the property from</param>
        /// <returns>The value of the Watermark property</returns>
        public static object GetWatermark(AvaloniaObject d)
        {
            return d.GetValue(WatermarkProperty);
        }

        /// <summary>
        /// Sets the Watermark property.  This dependency property indicates the watermark for the control.
        /// </summary>
        /// <param name="d"><see cref="AvaloniaObject"/> to set the property on</param>
        /// <param name="value">value of the property</param>
        public static void SetWatermark(AvaloniaObject d, object value)
        {
            d.SetValue(WatermarkProperty, value);
        }

        /// <summary>
        /// Handles changes to the Watermark property.
        /// </summary>
        /// <param name="d"><see cref="AvaloniaObject"/> that fired the event</param>
        /// <param name="e">A <see cref="AvaloniaPropertyChangedEventArgs"/> that contains the event data.</param>
        private static void OnWatermarkChanged(AvaloniaPropertyChangedEventArgs e)
        {
            Control control = (Control)e.Sender;
            control.Initialized += Control_Loaded;

            if (control is ComboBox || control is TextBox)
            {
                control.GotFocus += Control_GotKeyboardFocus;
                control.LostFocus += Control_Loaded;
            }

            if (control is TextBox textBox)
            {
                textBox.GetObservable(TextBox.TextProperty).Subscribe(val => Control_TextChanged(textBox));
            }

            if (control is ItemsControl i && !(control is ComboBox))
            {
                // for Items property  
                (i.Items as INotifyCollectionChanged).WeakSubscribe(ItemsChanged);
                itemsControls.Add(i.ItemContainerGenerator, i);
            }
        }

        #region Event Handlers

        /// <summary>
        /// Handle the GotFocus event on the control
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="RoutedEventArgs"/> that contains the event data.</param>
        private static void Control_GotKeyboardFocus(object sender, GotFocusEventArgs e)
        {
            Control c = (Control)sender;
            if (ShouldShowWatermark(c))
            {
                RemoveWatermark(c);
            }
        }

        static void Control_TextChanged(object sender)
        {
            Control control = (Control)sender;
            if (ShouldShowWatermark(control) && !control.IsFocused)
            {
                ShowWatermark(control);
            }
            else
            {
                RemoveWatermark(control);
            }
        }

        /// <summary>
        /// Handle the Loaded and LostFocus event on the control
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        private static void Control_Loaded(object sender, EventArgs e)
        {
            Control control = (Control)sender;
            if (ShouldShowWatermark(control))
            {
                ShowWatermark(control);
            }
        }

        /// <summary>
        /// Event handler for the items changed event
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="ItemsChangedEventArgs"/> that contains the event data.</param>
        private static void ItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ItemsControl control;
            if (itemsControls.TryGetValue(sender, out control))
            {
                if (ShouldShowWatermark(control))
                {
                    ShowWatermark(control);
                }
                else
                {
                    RemoveWatermark(control);
                }
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Remove the watermark from the specified element
        /// </summary>
        /// <param name="control">Element to remove the watermark from</param>
        private static void RemoveWatermark(IControl control)
        {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(control);

            // layer could be null if control is no longer in the visual tree
            if (layer != null)
            {
                Avalonia.Controls.Controls adorners = layer.Children;
                if (adorners == null)
                {
                    return;
                }

                foreach (Control adorner in adorners)
                {
                    if (adorner is WatermarkAdorner)
                    {
                        adorner.IsVisible = false;
                        layer.Children.Remove(adorner);
                    }
                }
            }
        }

        /// <summary>
        /// Show the watermark on the specified control
        /// </summary>
        /// <param name="control">Control to show the watermark on</param>
        private static void ShowWatermark(Control control)
        {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(control);

            // layer could be null if control is no longer in the visual tree
            if (layer != null)
            {
                layer.Children.Add(new WatermarkAdorner(control, GetWatermark(control)));
            }
        }

        /// <summary>
        /// Indicates whether or not the watermark should be shown on the specified control
        /// </summary>
        /// <param name="c"><see cref="Control"/> to test</param>
        /// <returns>true if the watermark should be shown; false otherwise</returns>
        private static bool ShouldShowWatermark(Control c)
        {
            if (c is ComboBox combo)
            {
                return combo.SelectedItem.ToString() == string.Empty;
            }
            else if (c is TextBox text)
            {
                return text.Text == string.Empty;
            }
            else if (c is ItemsControl)
            {
                return (c as ItemsControl).ItemCount == 0;
            }
            else
            {
                return false;
            }
        }

        #endregion
    }
}
