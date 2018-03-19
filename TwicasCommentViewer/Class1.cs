using SitePlugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TwicasCommentViewer
{
    /// <summary>
    /// 丸サムネに対応
    /// </summary>
    public class ThumbnailConverterNext : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var items = new ObservableCollection<Inline>();
            if (values == null || values.Length != 2)
            {
                return items;
            }
            var imageItem = values[0] as IMessageImage;
            if (imageItem == null)
                return items;//nullを返したらDataGridでVirtualizationModeをRecyclingにしている場合に他の画像が表示されてしまう
            var isEllipseThumbnail = (bool)values[1];
            isEllipseThumbnail = true;
            
            if (isEllipseThumbnail)
            {
                var bitmapImage = ConverterTools.Convert2ImageSource(imageItem);
                int width = imageItem.Width ?? (int)bitmapImage.Width;
                var height = imageItem.Height ?? (int)bitmapImage.Height;
                var ellipse = new Ellipse
                {
                    Fill = new ImageBrush { ImageSource = bitmapImage },
                    Width = width,
                    Height = height,
                };
                items.Add(new InlineUIContainer(ellipse));
            }
            else
            {
                var image = ConverterTools.Convert2Image(imageItem);
                items.Add(new InlineUIContainer(image));
            }
            
            return items;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
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
        public static Image Convert2Image(IMessageImage remoteIcon)
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
                    Width = width,
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
            return image;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="remoteIcon"></param>
        /// <returns>404とかで画像が取れなかったらnull</returns>
        public static BitmapImage Convert2ImageSource(IMessageImage remoteIcon)
        {
            var uri = remoteIcon.Url;
            var wc = new System.Net.WebClient { CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.CacheIfAvailable) };
            System.IO.MemoryStream ms = null;
            try
            {
                ms = new System.IO.MemoryStream(wc.DownloadData(uri));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            if(ms == null)
            {
                return null;
            }
            var bi = new BitmapImage();
            try
            {
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.EndInit();
                bi.Freeze();
            }
            catch (InvalidOperationException)
            {
                return null;
            }
            finally
            {
                wc.Dispose();
            }
            return bi;
        }
    }
}
