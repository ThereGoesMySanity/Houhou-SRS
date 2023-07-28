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
        public static readonly StyledProperty<ISolidColorBrush> FillProperty = AvaloniaProperty.Register<OutlinedTextBlock, ISolidColorBrush>(nameof(Fill), Brushes.Black);

        public static readonly StyledProperty<ISolidColorBrush> StrokeProperty = AvaloniaProperty.Register<OutlinedTextBlock, ISolidColorBrush>(nameof(Stroke), Brushes.Black);

        public static readonly StyledProperty<double> StrokeThicknessProperty = AvaloniaProperty.Register<OutlinedTextBlock, double>(nameof(StrokeThickness), 1d);

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

        //TODO: might be impossible
        // public override void Render(DrawingContext drawingContext)
        // {
        //     base.Render(drawingContext);

        //     //drawingContext.DrawGeometry(this.Fill, new Pen(this.Stroke, this.StrokeThickness), FormattedText.Geometry);
        // }
    }
}
