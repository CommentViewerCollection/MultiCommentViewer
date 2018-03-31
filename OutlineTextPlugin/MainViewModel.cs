using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Plugin;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;

namespace OutlineTextPlugin
{
    class MainViewModel :ViewModelBase
    {
        private readonly Options _options;
        public ICommand ShowOptionsViewCommand { get; }
        private void ShowOptionsView()
        {
            MessengerInstance.Send(new ShowOptionsViewMessage(new OptionsViewModel(_options)));
        }
        public bool IsEnabled
        {
            get { return _options.IsEnabled; }
            set { _options.IsEnabled = value; }
        }
        public double Height
        {
            get { return _options.MainViewHeight; }
            set { _options.MainViewHeight = value; }
        }
        public double Width
        {
            get { return _options.MainViewWidth; }
            set { _options.MainViewWidth = value; }
        }
        public double Left
        {
            get { return _options.MainViewLeft; }
            set { _options.MainViewLeft = value; }
        }
        public double Top
        {
            get { return _options.MainViewTop; }
            set { _options.MainViewTop = value; }
        }
        public ObservableCollection<CommentViewModel> Comments { get; }
        public FontFamily FontFamily => _options.FontFamily;
        public FontStyle FontStyle => _options.FontStyle;
        public FontWeight FontWeight => _options.FontWeight;
        public int FontSize
        {
            get { return _options.FontSize; }
            set { _options.FontSize = value; }
        }
        public bool IsBold
        {
            get
            {
                return _options.FontWeight == FontWeights.Bold;
            }
            set
            {
                var b = value;
                if (b)
                {
                    _options.FontWeight = FontWeights.Bold;
                }
                else
                {
                    _options.FontWeight = FontWeights.Normal;
                }
            }
        }
        public Color BackColor
        {
            get { return _options.BackColor; }
            set { _options.BackColor = value; }
        }
        public Brush Background => new SolidColorBrush(_options.BackColor);
        public int CommentOutlineTextThickness
        {
            get { return _options.CommentOutlineTextThickness; }
            set { _options.CommentOutlineTextThickness = value; }
        }
        public Color CommentOutlineStrokeColor
        {
            get { return _options.CommentOutlineStrokeColor; }
            set { _options.CommentOutlineStrokeColor = value; }
        }
        public Brush CommentOutlineStrokeColorBrush => new SolidColorBrush(_options.CommentOutlineStrokeColor);
        public Color CommentOutlineFillColor
        {
            get { return _options.CommentOutlineFillColor; }
            set { _options.CommentOutlineFillColor = value; }
        }
        public Brush CommentOutlineFillColorBrush => new SolidColorBrush(_options.CommentOutlineFillColor);

        public DataGridGridLinesVisibility GridLinesVisibility
        {
            get
            {
                if (_options.IsShowHorizontalGridLine && _options.IsShowVerticalGridLine)
                    return DataGridGridLinesVisibility.All;
                else if (_options.IsShowHorizontalGridLine)
                    return DataGridGridLinesVisibility.Horizontal;
                else if (_options.IsShowVerticalGridLine)
                    return DataGridGridLinesVisibility.Vertical;
                else
                    return DataGridGridLinesVisibility.None;
            }
        }
        #region HorizontalGridLine
        public bool IsShowHorizontalGridLine
        {
            get { return _options.IsShowHorizontalGridLine; }
            set { _options.IsShowHorizontalGridLine = value; }
        }
        public Color HorizontalGridLineColor
        {
            get { return _options.HorizontalGridLineColor; }
            set { _options.HorizontalGridLineColor = value; }
        }
        public Brush HorizontalGridLineBrush => new SolidColorBrush(_options.HorizontalGridLineColor);
        #endregion //HorizontalGridLine

        #region VerticalGridLin
        public bool IsShowVerticalGridLine
        {
            get { return _options.IsShowVerticalGridLine; }
            set { _options.IsShowVerticalGridLine = value; }
        }
        public Color VerticalGridLineColor
        {
            get { return _options.VerticalGridLineColor; }
            set { _options.VerticalGridLineColor = value; }
        }
        public Brush VerticalGridLineBrush => new SolidColorBrush(_options.VerticalGridLineColor);
        #endregion //VerticalGridLin

        public double ThumbnailWidth
        {
            get { return _options.ThumbnailWidth; }
            set { _options.ThumbnailWidth = value; }
        }
        public double UserNameWidth
        {
            get { return _options.UserNameWidth; }
            set { _options.UserNameWidth = value; }
        }
        public double MessageWidth
        {
            get { return _options.MessageWidth; }
            set { _options.MessageWidth = value; }
        }
        public int ThumbnailDisplayIndex
        {
            get { return _options.ThumbnailDisplayIndex; }
            set { _options.ThumbnailDisplayIndex = value; }
        }
        public int UserNameDisplayIndex
        {
            get { return _options.UsernameDisplayIndex; }
            set { _options.UsernameDisplayIndex = value; }
        }
        public int MessageDisplayIndex
        {
            get { return _options.MessageDisplayIndex; }
            set { _options.MessageDisplayIndex = value; }
        }

        public TextWrapping UserNameWrapping
        {
            get
            {
                if (_options.IsUserNameWrapping)
                    return TextWrapping.Wrap;
                else
                    return TextWrapping.NoWrap;
            }
        }
        public VerticalAlignment VerticalAlignment => _options.VerticalAlignment;
        private Thickness _rowBorderThickness;
        public Thickness RowBorderThickness
        {
            get
            {
                if(_rowBorderThickness == null)
                {
                    _rowBorderThickness = new Thickness(0, 0, 0, _options.LineMargin);
                }
                return _rowBorderThickness;
            }
        }
        public async void Add(ICommentData comment)
        {
            try
            {
                await _dispatcher.BeginInvoke((Action)(() =>
                {
                    Comments.Insert(0, new CommentViewModel(comment));
                }), DispatcherPriority.Normal);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        private Dispatcher _dispatcher;
        public MainViewModel(Options options):this()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _options.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(_options.IsEnabled):
                        RaisePropertyChanged(nameof(IsEnabled));
                        break;
                    case nameof(_options.FontFamily):
                        RaisePropertyChanged(nameof(FontFamily));
                        break;
                    case nameof(_options.FontStyle):
                        RaisePropertyChanged(nameof(FontStyle));
                        break;
                    case nameof(_options.FontWeight):
                        RaisePropertyChanged(nameof(FontWeight));
                        break;
                    case nameof(_options.FontSize):
                        RaisePropertyChanged(nameof(FontSize));
                        break;
                    case nameof(_options.CommentOutlineTextThickness):
                        RaisePropertyChanged(nameof(CommentOutlineTextThickness));
                        break;
                    case nameof(_options.CommentOutlineStrokeColor):
                        RaisePropertyChanged(nameof(CommentOutlineStrokeColorBrush));
                        break;
                    case nameof(_options.CommentOutlineFillColor):
                        RaisePropertyChanged(nameof(CommentOutlineFillColorBrush));
                        break;
                    case nameof(_options.IsShowHorizontalGridLine):
                    case nameof(_options.IsShowVerticalGridLine):
                        RaisePropertyChanged(nameof(GridLinesVisibility));
                        break;
                    case nameof(_options.HorizontalGridLineColor):
                        RaisePropertyChanged(nameof(HorizontalGridLineBrush));
                        break;
                    case nameof(_options.VerticalGridLineColor):
                        RaisePropertyChanged(nameof(VerticalGridLineBrush));
                        break;
                    case nameof(_options.BackColor):
                        RaisePropertyChanged(nameof(Background));
                        break;
                    case nameof(_options.IsUserNameWrapping):
                        RaisePropertyChanged(nameof(UserNameWrapping));
                        break;
                    case nameof(_options.ThumbnailWidth):
                        RaisePropertyChanged(nameof(ThumbnailWidth));
                        break;
                    case nameof(_options.VerticalAlignment):
                        RaisePropertyChanged(nameof(VerticalAlignment));
                        break;
                    case nameof(_options.LineMargin):
                        _rowBorderThickness = new Thickness(0, 0, 0, _options.LineMargin);
                        RaisePropertyChanged(nameof(RowBorderThickness));
                        break;
                }
            };
        }
        public MainViewModel()
        {
            Comments = new ObservableCollection<CommentViewModel>();
            ShowOptionsViewCommand = new RelayCommand(ShowOptionsView);
        }
    }
    public class FontFamilyViewModel
    {
        public string Text { get; private set; }
        public FontFamily FontFamily { get; private set; }

        public FontFamilyViewModel(FontFamily fontFamily, CultureInfo culture)
        {
            Text = ConvertFontFamilyToName(fontFamily, culture);
            FontFamily = fontFamily;
        }
        public override bool Equals(object obj)
        {
            var b = obj as FontFamilyViewModel;
            if (b == null)
                return false;
            return this.FontFamily.Equals(b.FontFamily);
        }
        public override int GetHashCode()
        {
            return FontFamily.GetHashCode();
        }
        public static string ConvertFontFamilyToName(FontFamily fontFamily, CultureInfo culture)
        {
            string text;
            var lang = XmlLanguage.GetLanguage(culture.IetfLanguageTag);
            if (fontFamily.FamilyNames.ContainsKey(lang))
            {
                text = fontFamily.FamilyNames[lang];
            }
            else
            {
                text = fontFamily.ToString();
            }
            return text;
        }
    }
    public class MarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var len = (double)value;
            var margin = double.Parse((string)parameter);
            return len - margin;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
