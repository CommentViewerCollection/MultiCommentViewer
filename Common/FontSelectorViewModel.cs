using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Globalization;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System.Windows.Markup;
using System.Windows.Data;

namespace Common.Wpf
{
    public class ShowFontSelectorViewOkMessage : MessageBase { }
    public class SetFontMessage : MessageBase
    {
        public FontFamily FontFamily { get; private set; }
        public FontStyle FontStyle { get; private set; }
        public FontWeight FontWeight { get; private set; }
        public int FontSize { get; private set; }
        public SetFontMessage(FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, int fontSize)
        {
            FontFamily = fontFamily;
            FontStyle = fontStyle;
            FontWeight = fontWeight;
            FontSize = fontSize;
        }
    }
    public class GetFontMessage : MessageBase
    {
        public Action<FontFamily, FontStyle, FontWeight, int> Callback { get; private set; }
        public GetFontMessage(Action<FontFamily, FontStyle, FontWeight, int> callback)
        {
            Callback = callback;
        }
    }
    public class FontSelectorViewOkMessage : MessageBase { }
    public class FontSelectorViewCancelMessage : MessageBase { }
    public sealed class FontSelectorViewModel : ViewModelBase
    {
        #region Command
        private RelayCommand _okCommand;
        public ICommand OkCommand
        {
            get { return _okCommand; }
        }

        private RelayCommand _cancelCommand;
        public ICommand CancelCommand
        {
            get { return _cancelCommand; }
        }
        #endregion

        #region Properties
        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    RaisePropertyChanged();
                }
            }
        }
        private SolidColorBrush _background;
        public SolidColorBrush Background
        {
            get { return _background; }
            set
            {
                _background = value;
                RaisePropertyChanged();
            }
        }
        public string SelectedFont
        {
            get { return $"abcABCあいう文字💐🤔"; }
        }
        private FontFamily _selectedFontFamily;
        public FontFamily SelectedFontFamily
        {
            get { return _selectedFontFamily; }
            set
            {
                if (_selectedFontFamily != value)
                {
                    _selectedFontFamily = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(SelectedFont));
                }
            }
        }
        private FontStyle _selectedFontStyle;
        public FontStyle SelectedFontStyle
        {
            get { return _selectedFontStyle; }
            set
            {
                if (_selectedFontStyle != value)
                {
                    _selectedFontStyle = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(SelectedFont));
                }
            }
        }
        private FontWeight _selectedFontWeight;
        public FontWeight SelectedFontWeight
        {
            get { return _selectedFontWeight; }
            set
            {
                if (_selectedFontWeight != value)
                {
                    _selectedFontWeight = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(SelectedFont));
                }
            }
        }
        private int _selectedFontSize;
        public int SelectedFontSize
        {
            get { return _selectedFontSize; }
            set
            {
                if (_selectedFontSize != value)
                {
                    _selectedFontSize = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(SelectedFont));
                }
            }
        }
        public ObservableCollection<FontFamilyViewModel> FontFamillyCollection { get; private set; }
        public ObservableCollection<FontStyleViewModel> FontStyleCollection { get; private set; }
        public ObservableCollection<FontWeightViewModel> FontWeightCollection { get; private set; }
        public ObservableCollection<int> FontSizeCollection { get; private set; }

        #endregion

        FontFamily _beforeFontFamily;
        private FontStyle _beforeFontStyle;
        private FontWeight _beforeFontWeight;
        int _beforeFontSize;
        public FontSelectorViewModel()
        {
            var fontList = Fonts.SystemFontFamilies.OrderBy(f => f.ToString()).Select(f => new FontFamilyViewModel(f, CultureInfo.CurrentCulture));
            FontFamillyCollection = new ObservableCollection<FontFamilyViewModel>(fontList);

            var fontStyleList = new List<FontStyleViewModel>{
                new FontStyleViewModel(FontStyles.Normal),
                new FontStyleViewModel(FontStyles.Italic),
                new FontStyleViewModel(FontStyles.Oblique),
            };
            FontStyleCollection = new ObservableCollection<FontStyleViewModel>(fontStyleList);


            var fontWeightList = new List<FontWeightViewModel>
            {
                new FontWeightViewModel(FontWeights.Black),
                new FontWeightViewModel(FontWeights.Bold),
                new FontWeightViewModel(FontWeights.DemiBold),
                new FontWeightViewModel(FontWeights.ExtraBlack),
                new FontWeightViewModel(FontWeights.ExtraBold),
                new FontWeightViewModel(FontWeights.ExtraLight),
                new FontWeightViewModel(FontWeights.Heavy),
                new FontWeightViewModel(FontWeights.Light),
                new FontWeightViewModel(FontWeights.Medium),
                new FontWeightViewModel(FontWeights.Normal),
                new FontWeightViewModel(FontWeights.Regular),
                new FontWeightViewModel(FontWeights.SemiBold),
                new FontWeightViewModel(FontWeights.Thin),
                new FontWeightViewModel(FontWeights.UltraBlack),
                new FontWeightViewModel(FontWeights.UltraBold),
                new FontWeightViewModel(FontWeights.UltraLight),
            };
            FontWeightCollection = new ObservableCollection<FontWeightViewModel>(fontWeightList);

            FontSizeCollection = new ObservableCollection<int>(Enumerable.Range(8, 30));

            Messenger.Default.Register<SetFontMessage>(this, message =>
            {
                Set(message.FontFamily, message.FontStyle, message.FontWeight, message.FontSize);
            });
            Messenger.Default.Register<GetFontMessage>(this, message =>
            {
                message.Callback(SelectedFontFamily, SelectedFontStyle, SelectedFontWeight, SelectedFontSize);
            });

            _okCommand = new RelayCommand(ExecuteOkCommand);
            _cancelCommand = new RelayCommand(ExecuteCancelCommand);

            //Set()が呼ばれる前にViewが初期化された場合に備え、全てのプロパティに値を入れておく。
            Title = "フォント選択";
            SelectedFontFamily = new FontFamily("Meiryo");
            _beforeFontFamily = SelectedFontFamily;
            SelectedFontStyle = FontStyles.Normal;
            _beforeFontStyle = SelectedFontStyle;
            SelectedFontWeight = FontWeights.Normal;
            _beforeFontWeight = SelectedFontWeight;
            SelectedFontSize = 10;
            _beforeFontSize = SelectedFontSize;

            Background = new SolidColorBrush(Colors.LightGray);
        }

        private void Set(FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, int fontSize)
        {
            _beforeFontFamily = fontFamily;
            _beforeFontStyle = fontStyle;
            _beforeFontWeight = fontWeight;
            _beforeFontSize = fontSize;
            SelectedFontFamily = fontFamily;
            SelectedFontSize = fontSize;
        }
        /// <summary>
        /// 変更を元に戻す
        /// </summary>
        private void Reset()
        {
            SelectedFontFamily = _beforeFontFamily;
            SelectedFontStyle = _beforeFontStyle;
            SelectedFontWeight = _beforeFontWeight;
            SelectedFontSize = _beforeFontSize;
        }
        private void ExecuteOkCommand()
        {
            Messenger.Default.Send(new FontSelectorViewOkMessage());
        }
        private void ExecuteCancelCommand()
        {
            Reset();
            Messenger.Default.Send(new FontSelectorViewCancelMessage());
        }
    }
    internal class ListBoxScroll : ListBox
    {
        public ListBoxScroll() : base()
        {
            SelectionChanged += new SelectionChangedEventHandler(ListBoxScroll_SelectionChanged);
        }

        void ListBoxScroll_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ScrollIntoView(SelectedItem);
        }
    }
    static class Utils
    {
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
    public class FontFamilyViewModel
    {
        public string Text { get; private set; }
        public FontFamily FontFamily { get; private set; }
        
        public FontFamilyViewModel(FontFamily fontFamily, CultureInfo culture)
        {
            Text = Utils.ConvertFontFamilyToName(fontFamily, culture);
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
            return base.GetHashCode();
        }
    }
    public class FontStyleViewModel
    {
        public string Text { get; private set; }
        public FontStyle FontStyle { get; private set; }
        public FontStyleViewModel(FontStyle fontStyle)
        {
            Text = fontStyle.ToString();
            FontStyle = fontStyle;
        }
        public override bool Equals(object obj)
        {
            var b = obj as FontStyleViewModel;
            if (b == null)
                return false;
            return this.FontStyle.Equals(b.FontStyle);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
    public class FontWeightViewModel
    {
        public string Text { get; private set; }
        public FontWeight FontWeight { get; private set; }
        public FontWeightViewModel(FontWeight fontWeight)
        {
            Text = fontWeight.ToString();
            FontWeight = fontWeight;
        }
        public override bool Equals(object obj)
        {
            var b = obj as FontWeightViewModel;
            if (b == null)
                return false;
            return this.FontWeight.Equals(b.FontWeight);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
    internal class FontFamilyToFontFamilyViewModelConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var fontFamily = value as FontFamily;
            return new FontFamilyViewModel(fontFamily, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var viewModel = value as FontFamilyViewModel;
            return viewModel.FontFamily;

        }
    }
    internal class FontStyleToFontStyleViewModelConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var fontFamily = (FontStyle)value;
            return new FontStyleViewModel(fontFamily);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var viewModel = value as FontStyleViewModel;
            return viewModel.FontStyle;
        }
    }
    internal class FontWeightToFontWeightViewModelConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var fontFamily = (FontWeight)value;
            return new FontWeightViewModel(fontFamily);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var viewModel = value as FontWeightViewModel;
            return viewModel.FontWeight;

        }
    }
    public class FontFamilyToJpStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var fontFamily = value as FontFamily;
            return Utils.ConvertFontFamilyToName(fontFamily, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var familyName = value as string;
            return new FontFamily(familyName);
        }
    }
}
