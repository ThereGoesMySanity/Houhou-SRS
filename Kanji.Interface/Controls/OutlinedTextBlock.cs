// Credits to Kent Boogaart
// http://stackoverflow.com/questions/93650/apply-stroke-to-a-textblock-in-wpf

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;





namespace Kanji.Interface.Controls
{
    public class OutlinedTextBlock : TextBlock
    {
        public static readonly AvaloniaProperty FillProperty = AvaloniaProperty.Register<OutlinedTextBlock, ISolidColorBrush>("Fill", Brushes.Black);

        public static readonly AvaloniaProperty StrokeProperty = AvaloniaProperty.Register<OutlinedTextBlock, ISolidColorBrush>("Stroke", Brushes.Black);

        public static readonly AvaloniaProperty StrokeThicknessProperty = AvaloniaProperty.Register<OutlinedTextBlock, double>("StrokeThickness", 1d);

        static OutlinedTextBlock()
        {
            AffectsRender<OutlinedTextBlock>(FillProperty, StrokeProperty, StrokeThicknessProperty);
        }

        private Geometry textGeometry;

        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }

        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        public override void Render(DrawingContext drawingContext)
        {
            base.Render(drawingContext);

            //TODO: might be impossible
            //drawingContext.DrawGeometry(this.Fill, new Pen(this.Stroke, this.StrokeThickness), FormattedText.Geometry);
        }
    }
}
