using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Platform;
using Kanji.Common.Models;

namespace Kanji.Interface.Converters;

public class ReviewCountToIconConverter : IValueConverter
{
    private readonly WindowIcon alertIcon;
    private readonly WindowIcon idleIcon;

    public ReviewCountToIconConverter()
    {
        alertIcon = new WindowIcon(AssetLoader.Open(new Uri("avares://Kanji.Interface/Data/Resources/TrayIconAlert.ico")));
        idleIcon = new WindowIcon(AssetLoader.Open(new Uri("avares://Kanji.Interface/Data/Resources/TrayIconIdle.ico")));
    }
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not ReviewInfo info) return AvaloniaProperty.UnsetValue;

        return info.AvailableReviewsCount > 0? alertIcon : idleIcon;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}