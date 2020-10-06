// Credits to John Myczek
// http://stackoverflow.com/questions/833943/watermark-hint-text-textbox-in-wpf

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;






namespace Kanji.Interface.Controls
{
    /// <summary>
    /// Adorner for the watermark
    /// </summary>
    internal class WatermarkAdorner : Control
    {
        #region Private Fields

        /// <summary>
        /// <see cref="ContentPresenter"/> that holds the watermark
        /// </summary>
        private readonly ContentPresenter contentPresenter;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="WatermarkAdorner"/> class
        /// </summary>
        /// <param name="adornedElement"><see cref="UIElement"/> to be adorned</param>
        /// <param name="watermark">The watermark</param>
        public WatermarkAdorner(Visual adornedElement, object watermark)
        {
            var adornerLayer = AdornerLayer.GetAdornerLayer(adornedElement);
            if (adornerLayer != null)
                AdornerLayer.SetAdornedElement(this, adornedElement);
            this.IsHitTestVisible = false;

            this.contentPresenter = new ContentPresenter();
            this.contentPresenter.Content = watermark;
            this.contentPresenter.Opacity = 0.5;
            //TODO: where padding?
            this.contentPresenter.Margin = new Thickness(Control.Margin.Left, Control.Margin.Top, 0, 0);

            if (this.Control is ItemsControl && !(this.Control is ComboBox))
            {
                this.contentPresenter.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
                this.contentPresenter.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
            }
            VisualChildren.Add(contentPresenter);

            // Hide the control adorner when the adorned element is hidden
            this[!IsVisibleProperty] = adornedElement[!IsVisibleProperty];

            Control feWatermark = watermark as Control;
            if (feWatermark != null && feWatermark.DataContext == null)
            {
                feWatermark.DataContext = this.Control.DataContext;
            }

        }

        #endregion

        #region Private Properties

        /// <summary>
        /// Gets the control that is being adorned
        /// </summary>
        private Control Control
        {
            get => AdornerLayer.GetAdornedElement(this) as Control;
        }

        #endregion
    }
}
