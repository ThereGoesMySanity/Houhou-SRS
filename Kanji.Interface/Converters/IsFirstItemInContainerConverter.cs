// Thank you Rohit Vats
// http://stackoverflow.com/questions/12125764/change-style-of-last-item-in-listbox

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;




namespace Kanji.Interface.Converters
{
    public class IsFirstItemInContainerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            var items = parameter as ItemsControl;
            return items.Items.Cast<object>().TakeWhile(i => i != value).Count();
                //== ic.Items.Count - 1;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
