using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Kanji.Interface.Converters
{
    public class ValueToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string && parameter == null)
            {
                return !string.IsNullOrWhiteSpace(value.ToString());
            }
            else if (parameter != null)
            {
                if (value == null)
                {
                    return false;
                }

                return value.ToString() == parameter.ToString();
            }

            // The parameter is null. Test the equality with the null value.
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
