using Mcv.PluginV2;
using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
namespace Mcv.MainViewPlugin
{
    /// <summary>
    /// Interaction logic for MainOptionsPanel.xaml
    /// </summary>
    public partial class MainOptionsPanel : UserControl
    {
        public MainOptionsPanel()
        {
            InitializeComponent();
        }
        internal void SetViewModel(MainOptionsViewModel vm)
        {
            this.DataContext = vm;
        }
        internal MainOptionsViewModel GetViewModel()
        {
            return (MainOptionsViewModel)this.DataContext;
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
        /// <summary>
        /// 極力、現在のカルチャーに合わせた名前を使用する。
        /// </summary>
        /// <param name="fontFamily"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
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
    class MainTabPage : IOptionsTabPage
    {
        public string HeaderText { get; }

        public UserControl TabPagePanel => _panel;

        public void Apply()
        {
            var optionsVm = _panel.GetViewModel();
            optionsVm.OriginOptions.Set(optionsVm.ChangedOptions);
        }

        public void Cancel()
        {
        }
        private readonly MainOptionsPanel _panel;
        public MainTabPage(string displayName, MainOptionsPanel panel)
        {
            HeaderText = displayName;
            _panel = panel;
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
    public class SiteConnectionColorTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ParameterString = parameter as string;
            if (ParameterString == null)
            {
                return System.Windows.DependencyProperty.UnsetValue;
            }

            if (Enum.IsDefined(value.GetType(), value) == false)
            {
                return System.Windows.DependencyProperty.UnsetValue;
            }

            object paramvalue = Enum.Parse(value.GetType(), ParameterString);

            return (int)paramvalue == (int)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked && parameter is string paramStr)
                return Enum.Parse(targetType, paramStr);

            return System.Windows.DependencyProperty.UnsetValue;
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
}
