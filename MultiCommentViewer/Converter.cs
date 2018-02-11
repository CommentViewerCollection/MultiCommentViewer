using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Interactivity;
using SitePlugin;
namespace MultiCommentViewer
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
                else if(item is IMessageImage remoteIcon)
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
}
