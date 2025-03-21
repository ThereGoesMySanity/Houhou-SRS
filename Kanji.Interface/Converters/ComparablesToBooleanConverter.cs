﻿using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Kanji.Interface.Converters
{
    public enum ComparablesToBooleanConversionEnum
    {
        Equal = 0,
        Different = 1,
        Less = 2,
        Greater = 3,
        LessOrEqual = 4,
        GreaterOrEqual = 5
    }

    public class ComparablesToBooleanConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Count() == 2 &&
                (parameter == null || parameter is ComparablesToBooleanConversionEnum))
            {
                if (values[0] is IComparable && values[1] is IComparable)
                {
                    // Compare the values.
                    IComparable a = (IComparable)values[0];
                    IComparable b = (IComparable)values[1];
                    int result = a.CompareTo(b);

                    // Get the operation type.
                    ComparablesToBooleanConversionEnum o = (parameter != null) ?
                        (ComparablesToBooleanConversionEnum)parameter
                        : ComparablesToBooleanConversionEnum.Equal;

                    // Answer.
                    switch (o)
                    {
                        case ComparablesToBooleanConversionEnum.Equal:
                            return result == 0;
                        case ComparablesToBooleanConversionEnum.Different:
                            return result != 0;
                        case ComparablesToBooleanConversionEnum.Less:
                            return result < 0;
                        case ComparablesToBooleanConversionEnum.Greater:
                            return result > 0;
                        case ComparablesToBooleanConversionEnum.LessOrEqual:
                            return result <= 0;
                        case ComparablesToBooleanConversionEnum.GreaterOrEqual:
                            return result >= 0;
                        default:
                            throw new ArgumentException(string.Format("Unknown enum value: \"{0}\".", o));
                    }
                }
                else
                {
                    return false;
                }
            }

            throw new ArgumentException("This converter takes two IComparable objects as values, "
            + "and an optional ComparablesToBooleanConversionEnum value as a parameter.");
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            // I dare you to try and implement this.
            throw new NotImplementedException();
        }
    }
}
