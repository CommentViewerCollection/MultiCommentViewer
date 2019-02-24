using System;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
namespace OpenrecYoyakuPlugin
{
    /// <summary>
    /// boolを反転させる
    /// </summary>
    public class NotConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Not((bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Not((bool)value);
        }
        private bool Not(bool b)
        {
            return !b;
        }
    }
}
