using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Kanji.Interface.Converters
{
    public class EnumBooleanConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString == null)
                return AvaloniaProperty.UnsetValue;

            if (Enum.IsDefined(value.GetType(), value) == false)
                return AvaloniaProperty.UnsetValue;

            object parameterValue = Enum.Parse(value.GetType(), parameterString);

            return parameterValue.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool b && !b)
                return BindingOperations.DoNothing;
            string parameterString = parameter as string;
            if (parameterString == null)
                return AvaloniaProperty.UnsetValue;

            return Enum.Parse(targetType, parameterString);
        }
        #endregion
    }
}
