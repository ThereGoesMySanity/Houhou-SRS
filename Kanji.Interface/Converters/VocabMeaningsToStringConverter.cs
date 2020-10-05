﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Kanji.Database.Entities;
using Kanji.Interface.Helpers;

namespace Kanji.Interface.Converters
{
    class VocabMeaningsToStringConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is VocabEntity)
            {
                try
                {
                    VocabCategoriesToStringConverter categoriesConverter = new VocabCategoriesToStringConverter();
                    IEnumerable<VocabMeaning> meanings = ((VocabEntity)value).Meanings;

                    TextBlock doc = new TextBlock();
                    bool onlyOne = meanings.Count() == 1;
                    int count = 0;
                    int maxCount = Kanji.Interface.Properties.Settings.Default.CollapseMeaningsLimit;
                    if (meanings.Count() > maxCount)
                    {
                        maxCount--;
                    }

                    foreach (VocabMeaning meaning in meanings)
                    {
                        if (!onlyOne)
                        {
                            doc.Text += $"{++count}. \n";
                            //TODO
                            //meaningIndexRun.FontWeight = FontWeights.Bold;
                            //meaningParagraph.Inlines.Add(meaningIndexRun);
                        }

                        // Append the categories string.
                        string categoriesString = (string)categoriesConverter.Convert(
                            meaning.Categories, typeof(string), null, culture);
                        if (!string.IsNullOrEmpty(categoriesString))
                        {
                            doc.Text += string.Format("[{0}] \n", categoriesString.ToLower());
                            //categoriesRun.Foreground = new SolidColorBrush(Colors.Gray);
                            //categoriesRun.FontSize = 10;
                            //meaningParagraph.Inlines.Add(categoriesRun);
                        }

                        doc.Text += meaning.Meaning;
                        //DispatcherHelper.Invoke(() =>
                        //{
                        //    meaningParagraph.Inlines.Add(new Run(meaning.Meaning));
                        //    doc.Blocks.Add(meaningParagraph);
                        //});

                        if (parameter == null && count >= maxCount)
                        {
                            break;
                        }
                    }

                    return doc;
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                throw new ArgumentException("The value must be a vocab entity.");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
