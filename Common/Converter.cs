using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Controls;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Documents;
using System.Collections.ObjectModel;
using SitePlugin;
using System.Windows.Media.Imaging;

namespace Common.Wpf
{
    public class NameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var collection = new ObservableCollection<Inline>();
            var items = value as IEnumerable<IMessagePart>;
            if (items == null)
            {
                return collection;
            }
            //IMessagePart before = null;
            foreach (IMessagePart item in items)
            {
                if (item is IMessageText text)
                {
                    collection.Add(new Run(text.Text));
                }
            }
            return collection;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class MessageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var collection = new ObservableCollection<Inline>();
            var items = value as IEnumerable<IMessagePart>;
            if (items == null)
            {
                return collection;
            }
            foreach (IMessagePart item in items)
            {
                if (item is IMessageText text)
                {
                    collection.Add(new Run(text.Text));
                }
                else if (item is IMessageImage remoteIcon)
                {
                    var uri = remoteIcon.Url;
                    var wc = new System.Net.WebClient { CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.CacheIfAvailable) };
                    var bi = new BitmapImage();
                    Image image = null;
                    try
                    {
                        bi.BeginInit();
                        bi.StreamSource = new System.IO.MemoryStream(wc.DownloadData(uri));
                        bi.EndInit();
                        bi.Freeze();
                        image = new Image()
                        {
                            Width = remoteIcon.Width ?? bi.Width,
                            Height = remoteIcon.Height ?? bi.Height,
                            Source = bi,
                            ToolTip = remoteIcon.Alt,
                        };
                    }
                    catch (System.Net.WebException)
                    {

                    }
                    catch (System.IO.IOException) { }
                    catch (InvalidOperationException) { }
                    finally
                    {
                        wc.Dispose();
                    }
                    if (image != null)
                    {
                        collection.Add(new InlineUIContainer(image));
                    }
                }
            }
            return collection;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// boolを反転させる
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool))]
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
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>ValueConversionを付加するために自前で実装</remarks>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool))
                return Visibility.Collapsed;

            var b = (bool)value;
            if (b)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class IntToFontSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var n = (int)value;
            var converter = new FontSizeConverter();
            return converter.ConvertFromString($"{n}pt");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// DataGridLengthとdoubleの相互変換
    /// </summary>
    public class DataGridLengthValueConverter : IValueConverter
    {
        private readonly DataGridLengthConverter _converter = new DataGridLengthConverter();
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return _converter.ConvertFrom(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var length = (DataGridLength)value;
            return length.Value;
        }
    }
    public class ColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                return new SolidColorBrush(color);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
