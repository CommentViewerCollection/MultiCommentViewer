using SitePlugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public class NameConverterNext : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var collection = new ObservableCollection<Inline>();
            if (values == null || values.Length != 5)
            {
                return collection;
            }
            var items = values[0] as IEnumerable<IMessagePart>;
            if (items == null) return collection;
            var isOutlineText = (bool)values[1];
            var outlineTextThickness = (int)values[2];
            var outlineTextStrokeColor = (Color)values[3];
            var outlineTextFillColor = (Color)values[4];

            IMessagePart before = null;
            foreach (IMessagePart item in items)
            {
                if (item is IMessageText text)
                {
                    if (isOutlineText)
                    {
                        collection.Add(new InlineUIContainer(new OutlineText()
                        {
                            Text = text.Text,
                            Stroke = new SolidColorBrush(outlineTextStrokeColor),
                            Fill = new SolidColorBrush(outlineTextFillColor),
                            StrokeThickness = outlineTextThickness
                        }));
                    }
                    else
                    {
                        collection.Add(new Run(text.Text));
                    }
                }
                else if (item is IMessageLink link)
                {
                    if (isOutlineText)
                    {
                        collection.Add(new InlineUIContainer(new OutlineText()
                        {
                            Text = link.Text,
                            Stroke = new SolidColorBrush(outlineTextStrokeColor),
                            Fill = new SolidColorBrush(outlineTextFillColor),
                            StrokeThickness = outlineTextThickness
                        }));
                    }
                    else
                    {
                        collection.Add(new Hyperlink(new Run(link.Text)) { NavigateUri = new Uri(link.Url) });
                    }
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

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class MessageConverterNext : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var collection = new ObservableCollection<Inline>();
            if(values == null || values.Length != 5)
            {
                return collection;
            }
            var items = values[0] as IEnumerable<IMessagePart>;
            if (items == null) return collection;
            var isOutlineText = (bool)values[1];
            var outlineTextThickness = (int)values[2];
            var outlineTextStrokeColor = (Color)values[3];
            var outlineTextFillColor = (Color)values[4];

            IMessagePart before = null;
            foreach (IMessagePart item in items)
            {
                if (item is IMessageText text)
                {
                    if (isOutlineText)
                    {
                        collection.Add(new InlineUIContainer(new OutlineText()
                        {
                            Text = text.Text,
                            Stroke = new SolidColorBrush(outlineTextStrokeColor),
                            Fill = new SolidColorBrush(outlineTextFillColor),
                            StrokeThickness = outlineTextThickness
                        }));
                    }
                    else
                    {
                        collection.Add(new Run(text.Text));
                    }
                }
                else if(item is IMessageLink link)
                {
                    if (isOutlineText)
                    {
                        collection.Add(new InlineUIContainer(new OutlineText()
                        {
                            Text = link.Text,
                            Stroke = new SolidColorBrush(outlineTextStrokeColor),
                            Fill = new SolidColorBrush(outlineTextFillColor),
                            StrokeThickness = outlineTextThickness
                        }));
                    }
                    else
                    {
                        collection.Add(new Hyperlink(new Run(link.Text)) { NavigateUri = new Uri(link.Url) });
                    }
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
        public static BitmapImage Convert2ImageSource(IMessageImage remoteIcon)
        {
            var uri = remoteIcon.Url;
            var wc = new System.Net.WebClient { CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.CacheIfAvailable) };
            var bi = new BitmapImage();
            try
            {
                bi.BeginInit();
                bi.StreamSource = new System.IO.MemoryStream(wc.DownloadData(uri));
                bi.EndInit();
                bi.Freeze();
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
            return bi;
        }
        [Obsolete]
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
            return new InlineUIContainer(image);
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
    [ContentProperty("Text")]
    public class OutlineText : FrameworkElement
    {
        private FormattedText FormattedText;
        private Geometry TextGeometry;

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(OutlineText),
            new FrameworkPropertyMetadata(OnFormattedTextInvalidated));

        public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register(
            "TextAlignment", typeof(TextAlignment), typeof(OutlineText),
            new FrameworkPropertyMetadata(OnFormattedTextUpdated));

        public static readonly DependencyProperty TextDecorationsProperty = DependencyProperty.Register(
            "TextDecorations", typeof(TextDecorationCollection), typeof(OutlineText),
            new FrameworkPropertyMetadata(OnFormattedTextUpdated));

        public static readonly DependencyProperty TextTrimmingProperty = DependencyProperty.Register(
            "TextTrimming", typeof(TextTrimming), typeof(OutlineText),
            new FrameworkPropertyMetadata(OnFormattedTextUpdated));

        public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register(
            "TextWrapping", typeof(TextWrapping), typeof(OutlineText),
            new FrameworkPropertyMetadata(TextWrapping.NoWrap, OnFormattedTextUpdated));

        public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
            "Fill", typeof(Brush), typeof(OutlineText),
            new FrameworkPropertyMetadata(Brushes.Red, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
            "Stroke", typeof(Brush), typeof(OutlineText),
            new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
            "StrokeThickness", typeof(double), typeof(OutlineText),
            new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(
            typeof(OutlineText), new FrameworkPropertyMetadata(OnFormattedTextUpdated));

        public static readonly DependencyProperty FontSizeProperty = TextElement.FontSizeProperty.AddOwner(
            typeof(OutlineText), new FrameworkPropertyMetadata(OnFormattedTextUpdated));

        public static readonly DependencyProperty FontStretchProperty = TextElement.FontStretchProperty.AddOwner(
            typeof(OutlineText), new FrameworkPropertyMetadata(OnFormattedTextUpdated));

        public static readonly DependencyProperty FontStyleProperty = TextElement.FontStyleProperty.AddOwner(
            typeof(OutlineText), new FrameworkPropertyMetadata(OnFormattedTextUpdated));

        public static readonly DependencyProperty FontWeightProperty = TextElement.FontWeightProperty.AddOwner(
            typeof(OutlineText), new FrameworkPropertyMetadata(OnFormattedTextUpdated));

        public OutlineText()
        {
            this.TextDecorations = new TextDecorationCollection();
        }

        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        public FontFamily FontFamily
        {
            get { return (FontFamily)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        [TypeConverter(typeof(FontSizeConverter))]
        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public FontStretch FontStretch
        {
            get { return (FontStretch)GetValue(FontStretchProperty); }
            set { SetValue(FontStretchProperty, value); }
        }

        public FontStyle FontStyle
        {
            get { return (FontStyle)GetValue(FontStyleProperty); }
            set { SetValue(FontStyleProperty, value); }
        }

        public FontWeight FontWeight
        {
            get { return (FontWeight)GetValue(FontWeightProperty); }
            set { SetValue(FontWeightProperty, value); }
        }

        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }

        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)GetValue(TextAlignmentProperty); }
            set { SetValue(TextAlignmentProperty, value); }
        }

        public TextDecorationCollection TextDecorations
        {
            get { return (TextDecorationCollection)this.GetValue(TextDecorationsProperty); }
            set { this.SetValue(TextDecorationsProperty, value); }
        }

        public TextTrimming TextTrimming
        {
            get { return (TextTrimming)GetValue(TextTrimmingProperty); }
            set { SetValue(TextTrimmingProperty, value); }
        }

        public TextWrapping TextWrapping
        {
            get { return (TextWrapping)GetValue(TextWrappingProperty); }
            set { SetValue(TextWrappingProperty, value); }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            this.EnsureGeometry();

            drawingContext.DrawGeometry(this.Fill, new Pen(this.Stroke, this.StrokeThickness), this.TextGeometry);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            this.EnsureFormattedText();

            if (FormattedText == null)
                return new Size(0, 0);

            this.FormattedText.MaxTextWidth = Math.Min(3579139, availableSize.Width);
            this.FormattedText.MaxTextHeight = availableSize.Height;

            return new Size(this.FormattedText.Width, this.FormattedText.Height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            this.EnsureFormattedText();

            if (FormattedText == null)
                return new Size(0, 0);

            this.FormattedText.MaxTextWidth = finalSize.Width;
            this.FormattedText.MaxTextHeight = finalSize.Height > 0 ? finalSize.Height : 20;//finalSize.Heightが0の場合があって、それだと例外が投げられてしまうからとりあえず0ではない値を入れるようにした

            this.TextGeometry = null;

            return finalSize;
        }

        private static void OnFormattedTextInvalidated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var outlinedTextBlock = (OutlineText)dependencyObject;
            outlinedTextBlock.FormattedText = null;
            outlinedTextBlock.TextGeometry = null;

            outlinedTextBlock.InvalidateMeasure();
            outlinedTextBlock.InvalidateVisual();
        }

        private static void OnFormattedTextUpdated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var outlinedTextBlock = (OutlineText)dependencyObject;
            outlinedTextBlock.UpdateFormattedText();
            outlinedTextBlock.TextGeometry = null;

            outlinedTextBlock.InvalidateMeasure();
            outlinedTextBlock.InvalidateVisual();
        }

        private void EnsureFormattedText()
        {
            if (this.FormattedText != null || this.Text == null)
                return;

            this.FormattedText = new FormattedText(
                this.Text,
                CultureInfo.CurrentUICulture,
                this.FlowDirection,
                new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, FontStretches.Normal),
                this.FontSize,
                Brushes.Black);

            this.UpdateFormattedText();
        }

        private void UpdateFormattedText()
        {
            if (this.FormattedText == null)
                return;

            this.FormattedText.MaxLineCount = this.TextWrapping == TextWrapping.NoWrap ? 1 : int.MaxValue;
            this.FormattedText.TextAlignment = this.TextAlignment;
            this.FormattedText.Trimming = this.TextTrimming;

            this.FormattedText.SetFontSize(this.FontSize);
            this.FormattedText.SetFontStyle(this.FontStyle);
            this.FormattedText.SetFontWeight(this.FontWeight);
            this.FormattedText.SetFontFamily(this.FontFamily);
            this.FormattedText.SetFontStretch(this.FontStretch);
            this.FormattedText.SetTextDecorations(this.TextDecorations);
        }

        private void EnsureGeometry()
        {
            if (this.TextGeometry != null)
                return;

            this.EnsureFormattedText();

            if (FormattedText == null)
                return;

            this.TextGeometry = this.FormattedText.BuildGeometry(new Point());
        }
    }
}
