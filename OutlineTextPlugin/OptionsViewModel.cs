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
    class OptionsViewModel:ViewModelBase
    {
        private readonly Options _options;

        public ICommand CloseCommand { get; }
        private void Close()
        {
            MessengerInstance.Send(new CloseOptionsViewMessage());
        }
        public ObservableCollection<FontFamilyViewModel> FontFamillyCollection { get; private set; }
        public ObservableCollection<int> FontSizeCollection { get; private set; }
        public ObservableCollection<CommentViewModel> Comments { get; }
        private FontFamilyViewModel _selectedFontFamily;
        public FontFamilyViewModel SelectedFontFamily
        {
            get { return _selectedFontFamily; }
            set
            {
                if (_selectedFontFamily != null &&  _options.FontFamily == value.FontFamily)
                    return;
                _options.FontFamily = value.FontFamily;
                _selectedFontFamily = value;
            }
        }
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
        public bool IsUserNameWrapping
        {
            get { return _options.IsUserNameWrapping; }
            set { _options.IsUserNameWrapping = value; }
        }
        #endregion //VerticalGridLine

        public Color SelectedRowBackColor
        {
            get { return _options.SelectedRowBackColor; }
            set
            {
                _options.SelectedRowBackColor = value;
                RaisePropertyChanged();
            }
        }
        public Color SelectedRowForeColor
        {
            get { return _options.SelectedRowForeColor; }
            set
            {
                _options.SelectedRowForeColor = value;
                RaisePropertyChanged();
            }
        }
        public VerticalAlignment VerticalAlignment
        {
            get { return _options.VerticalAlignment; }
            set
            {
                _options.VerticalAlignment = value;
                RaisePropertyChanged();
            }
        }
        public int LineMargin
        {
            get { return _options.LineMargin; }
            set
            {
                _options.LineMargin = value;
                RaisePropertyChanged();
            }
        }

        public OptionsViewModel() : this(new Options()) { }
        public OptionsViewModel(Options options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _options.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
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
                        RaisePropertyChanged(nameof(IsUserNameWrapping));
                        break;
                    case nameof(_options.VerticalAlignment):
                        RaisePropertyChanged(nameof(VerticalAlignment));
                        break;
                }
            };
            CloseCommand = new RelayCommand(Close);

            Comments = new ObservableCollection<CommentViewModel>();

            var fontList = Fonts.SystemFontFamilies.OrderBy(f => f.ToString()).Select(f => new FontFamilyViewModel(f, CultureInfo.CurrentCulture));
            FontFamillyCollection = new ObservableCollection<FontFamilyViewModel>(fontList);
            SelectedFontFamily = new FontFamilyViewModel(_options.FontFamily, CultureInfo.CurrentCulture);

            var sizeList = Enumerable.Range(6, 40);
            FontSizeCollection = new ObservableCollection<int>(sizeList);
        }
    }
    public class EnumBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            string ParameterString = parameter as string;
            if (ParameterString == null)
            {
                return DependencyProperty.UnsetValue;
            }

            if (Enum.IsDefined(value.GetType(), value) == false)
            {
                return DependencyProperty.UnsetValue;
            }

            object paramvalue = Enum.Parse(value.GetType(), ParameterString);

            if (paramvalue.Equals(value))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string ParameterString = parameter as string;
            if (ParameterString == null)
            {
                return DependencyProperty.UnsetValue;
            }

            return Enum.Parse(targetType, ParameterString);
        }
    }
}
