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
    internal static class ConverterTools
    {
        public static InlineUIContainer RemoteImage2UiContainer(IMessageImage remoteIcon)
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
                };
                if (!string.IsNullOrEmpty(remoteIcon.Alt))
                {
                    image.ToolTip = remoteIcon.Alt;
                }
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
            return image != null ? new InlineUIContainer(image) : null;
        }
        public static InlineUIContainer ImagePortion2UiContainer(IMessageImagePortion ico)
        {
            var bitmap = new System.Drawing.Bitmap(ico.Image);
            var rectangle = new System.Drawing.Rectangle(ico.SrcX, ico.SrcY, ico.SrcWidth, ico.SrcHeight);
            var cropped = bitmap.Clone(rectangle, bitmap.PixelFormat);
            var bitmapImage = ConverterTools.ConvertFromBitmap(cropped, System.Drawing.Imaging.ImageFormat.Png);
            var image = new Image()
            {
                Width = ico.Width,
                Height = ico.Height,
                Source = bitmapImage,
            };
            if (!string.IsNullOrEmpty(ico.Alt))
            {
                image.ToolTip = ico.Alt;
            }
            return new InlineUIContainer(image);
        }
        public static BitmapImage ConvertFromBitmap(System.Drawing.Bitmap bitmap, System.Drawing.Imaging.ImageFormat format)
        {
            using (var stream = new System.IO.MemoryStream())
            {
                bitmap.Save(stream, format);
                stream.Seek(0, System.IO.SeekOrigin.Begin);
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
        }
        public static Inline Convert(IMessageImage remoteIcon)
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
                var width = remoteIcon.Width == null || remoteIcon.Width.Value == 0 ? bi.Width : remoteIcon.Width.Value;
                var height = remoteIcon.Height == null || remoteIcon.Height.Value == 0 ? bi.Height : remoteIcon.Height.Value;
                image = new Image()
                {
                    Width =width,
                    Height = height,
                    Source = bi,
                };
                if (!string.IsNullOrEmpty(remoteIcon.Alt))
                {
                    image.ToolTip = remoteIcon.Alt;
                }
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
            return new InlineUIContainer(image);
        }
    }
    public class ThumbnailConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var items = new ObservableCollection<Inline>();
            var image = value as IMessageImage;
            if (image == null || string.IsNullOrEmpty(image.Url))
                return items;//nullを返したらDataGridでVirtualizationModeをRecyclingにしている場合に他の画像が表示されてしまう
            var inline = ConverterTools.Convert(image);
            items.Add(inline);
            return items;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
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
            IMessagePart before = null;
            foreach (IMessagePart item in items)
            {
                if (item is IMessageText text)
                {
                    collection.Add(new Run(text.Text));
                }
                else if(item is IMessageImage remoteImage)
                {
                    var uiContainer = ConverterTools.RemoteImage2UiContainer(remoteImage);
                    if (uiContainer != null)
                    {
                        collection.Add(uiContainer);
                    }
                }
                else if (item is IMessageImagePortion ico)
                {
                    if (before is IMessageText)
                        collection.Add(new Run(" "));

                    var uiContainer = ConverterTools.ImagePortion2UiContainer(ico);
                    if (uiContainer != null)
                    {
                        collection.Add(uiContainer);
                    }
                }
                before = item;
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
            IMessagePart before = null;
            foreach (IMessagePart item in items)
            {
                if (item is IMessageText text)
                {
                    collection.Add(new Run(text.Text));
                }
                else if (item is IMessageLink link)
                {
                    collection.Add(new Hyperlink(new Run(link.Text)) { NavigateUri = new Uri(link.Url) });
                }
                else if (item is IMessageImage remoteImage)
                {
                    var uiContainer = ConverterTools.RemoteImage2UiContainer(remoteImage);
                    if (uiContainer != null)
                    {
                        collection.Add(uiContainer);
                    }
                }
                else if (item is IMessageImagePortion ico)
                {
                    if (before is IMessageText)
                        collection.Add(new Run(" "));

                    var uiContainer = ConverterTools.ImagePortion2UiContainer(ico);
                    if (uiContainer != null)
                    {
                        collection.Add(uiContainer);
                    }
                }
                before = item;
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
    public class GridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var converter = new System.Windows.GridLengthConverter();
            return converter.ConvertFrom(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var length = (GridLength)value;
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
