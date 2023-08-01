using Avalonia.Markup.Xaml.Converters;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia;
using Avalonia.Data;
using System;
using System.Globalization;

namespace Kanji.Interface.Converters
{
    public static class CustomAvaloniaConverters
    {
        public static readonly IValueConverter ThicknessConverter = new FuncValueConverter<string, Thickness>(Thickness.Parse);

        public static readonly IValueConverter BitmapConverter = new BitmapConverter();
    }
    public class BitmapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = (string)value;
            if (s == null)
                return BindingOperations.DoNothing;

            var uri = s.StartsWith("/")
                ? new Uri(s, UriKind.Relative)
                : new Uri(s, UriKind.RelativeOrAbsolute);

            if(uri.IsAbsoluteUri && uri.IsFile)
                return new Bitmap(uri.LocalPath);

            return new Bitmap(AssetLoader.Open(new Uri($"avares://Kanji.Interface{s}")));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

    }
}