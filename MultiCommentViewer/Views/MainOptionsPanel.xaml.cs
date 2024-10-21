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
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Common;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MultiCommentViewer.Test;
using SitePlugin;
namespace MultiCommentViewer
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
    class MainOptionsViewModel : INotifyPropertyChanged
    {
        public Color BackColor
        {
            get { return ChangedOptions.BackColor; }
            set { ChangedOptions.BackColor = value; }
        }
        public Color ForeColor
        {
            get { return ChangedOptions.ForeColor; }
            set { ChangedOptions.ForeColor = value; }
        }
        public Color NoticeCommentBackColor
        {
            get { return ChangedOptions.InfoBackColor; }
            set { ChangedOptions.InfoBackColor = value; }
        }
        public Color NoticeCommentForeColor
        {
            get { return ChangedOptions.InfoForeColor; }
            set { ChangedOptions.InfoForeColor = value; }
        }
        public Color SiteInfoBackColor
        {
            get { return ChangedOptions.BroadcastInfoBackColor; }
            set { ChangedOptions.BroadcastInfoBackColor = value; }
        }
        public Color SiteInfoForeColor
        {
            get { return ChangedOptions.BroadcastInfoForeColor; }
            set { ChangedOptions.BroadcastInfoForeColor = value; }
        }
        public Color SelectedRowBackColor
        {
            get { return ChangedOptions.SelectedRowBackColor; }
            set { ChangedOptions.SelectedRowBackColor = value; }
        }
        public Color SelectedRowForeColor
        {
            get { return ChangedOptions.SelectedRowForeColor; }
            set { ChangedOptions.SelectedRowForeColor = value; }
        }
        public Color FirstCommentBackColor
        {
            get { return ChangedOptions.FirstCommentBackColor; }
            set { ChangedOptions.FirstCommentBackColor = value; }
        }
        public Color FirstCommentForeColor
        {
            get { return ChangedOptions.FirstCommentForeColor; }
            set { ChangedOptions.FirstCommentForeColor = value; }
        }
        public Color VerticalGridLineColor
        {
            get { return ChangedOptions.VerticalGridLineColor; }
            set { ChangedOptions.VerticalGridLineColor = value; }
        }
        public Color HorizontalGridLineColor
        {
            get { return ChangedOptions.HorizontalGridLineColor; }
            set { ChangedOptions.HorizontalGridLineColor = value; }
        }
        public bool IsUserNameWrapping
        {
            get { return ChangedOptions.IsUserNameWrapping; }
            set { ChangedOptions.IsUserNameWrapping = value; }
        }
        public bool IsAddingNewCommentTop
        {
            get { return ChangedOptions.IsAddingNewCommentTop; }
            set { ChangedOptions.IsAddingNewCommentTop = value; }
        }
        public bool IsAutoCheckIfUpdateExists
        {
            get { return ChangedOptions.IsAutoCheckIfUpdateExists; }
            set { ChangedOptions.IsAutoCheckIfUpdateExists = value; }
        }
        public FontFamilyViewModel FontFamily
        {
            get { return new FontFamilyViewModel(ChangedOptions.FontFamily, CultureInfo.CurrentCulture); }
            set { ChangedOptions.FontFamily = value.FontFamily; }
        }
        public int FontSize
        {
            get { return ChangedOptions.FontSize; }
            set { ChangedOptions.FontSize = value; }
        }
        public bool IsBold
        {
            get
            {
                return ChangedOptions.FontWeight == FontWeights.Bold;
            }
            set
            {
                var b = value;
                if (b)
                {
                    ChangedOptions.FontWeight = FontWeights.Bold;
                }
                else
                {
                    ChangedOptions.FontWeight = FontWeights.Normal;
                }
            }
        }
        public FontFamilyViewModel FirstCommentFontFamily
        {
            get { return new FontFamilyViewModel(ChangedOptions.FirstCommentFontFamily, CultureInfo.CurrentCulture); }
            set { ChangedOptions.FirstCommentFontFamily = value.FontFamily; }
        }
        public int FirstCommentFontSize
        {
            get { return ChangedOptions.FirstCommentFontSize; }
            set { ChangedOptions.FirstCommentFontSize = value; }
        }
        public bool IsFirstCommentBold
        {
            get
            {
                return ChangedOptions.FirstCommentFontWeight == FontWeights.Bold;
            }
            set
            {
                var b = value;
                if (b)
                {
                    ChangedOptions.FirstCommentFontWeight = FontWeights.Bold;
                }
                else
                {
                    ChangedOptions.FirstCommentFontWeight = FontWeights.Normal;
                }
            }
        }
        public bool IsPixelScrolling
        {
            get { return ChangedOptions.IsPixelScrolling; }
            set { ChangedOptions.IsPixelScrolling = value; }
        }
        public bool IsEnabledSiteConnectionColor
        {
            get { return ChangedOptions.IsEnabledSiteConnectionColor; }
            set { ChangedOptions.IsEnabledSiteConnectionColor = value; }
        }
        public SiteConnectionColorType SiteConnectionColorType
        {
            get { return ChangedOptions.SiteConnectionColorType; }
            set { ChangedOptions.SiteConnectionColorType = value; }
        }

        public ICommand DefaultThemeCommand { get; set; }
        public ICommand DarkThemeCommand { get; set; }
        public ICommand SukesukeThemeCommand { get; set; }
        public Color TitleBackColor
        {
            get => ChangedOptions.TitleBackColor;
            set => ChangedOptions.TitleBackColor = value;
        }
        public Color TitleForeColor
        {
            get => ChangedOptions.TitleForeColor;
            set => ChangedOptions.TitleForeColor = value;
        }
        public Color ViewBackColor
        {
            get => ChangedOptions.ViewBackColor;
            set => ChangedOptions.ViewBackColor = value;
        }
        public Color WindowBorderColor
        {
            get => ChangedOptions.WindowBorderColor;
            set => ChangedOptions.WindowBorderColor = value;
        }
        public Color SystemButtonBackColor
        {
            get => ChangedOptions.SystemButtonBackColor;
            set => ChangedOptions.SystemButtonBackColor = value;
        }
        public Color SystemButtonForeColor
        {
            get => ChangedOptions.SystemButtonForeColor;
            set => ChangedOptions.SystemButtonForeColor = value;
        }
        public Color SystemButtonBorderColor
        {
            get => ChangedOptions.SystemButtonBorderColor;
            set => ChangedOptions.SystemButtonBorderColor = value;
        }
        public Color SystemButtonMouseOverBackColor
        {
            get => ChangedOptions.SystemButtonMouseOverBackColor;
            set => ChangedOptions.SystemButtonMouseOverBackColor = value;
        }
        public Color SystemButtonMouseOverForeColor
        {
            get => ChangedOptions.SystemButtonMouseOverForeColor;
            set => ChangedOptions.SystemButtonMouseOverForeColor = value;
        }
        public Color SystemButtonMouseOverBorderColor
        {
            get => ChangedOptions.SystemButtonMouseOverBorderColor;
            set => ChangedOptions.SystemButtonMouseOverBorderColor = value;
        }
        public Color MenuBackColor
        {
            get => ChangedOptions.MenuBackColor;
            set => ChangedOptions.MenuBackColor = value;
        }
        public Color MenuForeColor
        {
            get => ChangedOptions.MenuForeColor;
            set => ChangedOptions.MenuForeColor = value;
        }
        public Color MenuItemCheckMarkColor
        {
            get => ChangedOptions.MenuItemCheckMarkColor;
            set => ChangedOptions.MenuItemCheckMarkColor = value;
        }
        public Color MenuItemMouseOverBackColor
        {
            get => ChangedOptions.MenuItemMouseOverBackColor;
            set => ChangedOptions.MenuItemMouseOverBackColor = value;
        }
        public Color MenuItemMouseOverForeColor
        {
            get => ChangedOptions.MenuItemMouseOverForeColor;
            set => ChangedOptions.MenuItemMouseOverForeColor = value;
        }
        public Color MenuItemMouseOverBorderColor
        {
            get => ChangedOptions.MenuItemMouseOverBorderColor;
            set => ChangedOptions.MenuItemMouseOverBorderColor = value;
        }
        public Color MenuItemMouseOverCheckMarkColor
        {
            get => ChangedOptions.MenuItemMouseOverCheckMarkColor;
            set => ChangedOptions.MenuItemMouseOverCheckMarkColor = value;
        }
        public Color MenuSeparatorBackColor
        {
            get => ChangedOptions.MenuSeparatorBackColor;
            set => ChangedOptions.MenuSeparatorBackColor = value;
        }
        public Color MenuPopupBorderColor
        {
            get => ChangedOptions.MenuPopupBorderColor;
            set => ChangedOptions.MenuPopupBorderColor = value;
        }
        public Color ButtonBackColor
        {
            get => ChangedOptions.ButtonBackColor;
            set => ChangedOptions.ButtonBackColor = value;
        }
        public Color ButtonForeColor
        {
            get => ChangedOptions.ButtonForeColor;
            set => ChangedOptions.ButtonForeColor = value;
        }
        public Color ButtonBorderColor
        {
            get => ChangedOptions.ButtonBorderColor;
            set => ChangedOptions.ButtonBorderColor = value;
        }
        public Color CommentListBackColor
        {
            get => ChangedOptions.CommentListBackColor;
            set => ChangedOptions.CommentListBackColor = value;
        }
        public Color CommentListHeaderBackColor
        {
            get => ChangedOptions.CommentListHeaderBackColor;
            set => ChangedOptions.CommentListHeaderBackColor = value;
        }
        public Color CommentListHeaderForeColor
        {
            get => ChangedOptions.CommentListHeaderForeColor;
            set => ChangedOptions.CommentListHeaderForeColor = value;
        }
        public Color CommentListHeaderBorderColor
        {
            get => ChangedOptions.CommentListHeaderBorderColor;
            set => ChangedOptions.CommentListHeaderBorderColor = value;
        }
        public Color CommentListBorderColor
        {
            get => ChangedOptions.CommentListBorderColor;
            set => ChangedOptions.CommentListBorderColor = value;
        }
        public Color CommentListSeparatorColor
        {
            get => ChangedOptions.CommentListSeparatorColor;
            set => ChangedOptions.CommentListSeparatorColor = value;
        }
        public Color ConnectionListBackColor
        {
            get => ChangedOptions.ConnectionListBackColor;
            set => ChangedOptions.ConnectionListBackColor = value;
        }
        public Color ConnectionListHeaderBackColor
        {
            get => ChangedOptions.ConnectionListHeaderBackColor;
            set => ChangedOptions.ConnectionListHeaderBackColor = value;
        }
        public Color ConnectionListHeaderForeColor
        {
            get => ChangedOptions.ConnectionListHeaderForeColor;
            set => ChangedOptions.ConnectionListHeaderForeColor = value;
        }
        public Color ConnectionListRowBackColor
        {
            get => ChangedOptions.ConnectionListRowBackColor;
            set => ChangedOptions.ConnectionListRowBackColor = value;
        }
        public Color ScrollBarBorderColor
        {
            get => ChangedOptions.ScrollBarBorderColor;
            set => ChangedOptions.ScrollBarBorderColor = value;
        }
        public Color ScrollBarThumbBackColor
        {
            get => ChangedOptions.ScrollBarThumbBackColor;
            set => ChangedOptions.ScrollBarThumbBackColor = value;
        }
        public Color ScrollBarThumbMouseOverBackColor
        {
            get => ChangedOptions.ScrollBarThumbMouseOverBackColor;
            set => ChangedOptions.ScrollBarThumbMouseOverBackColor = value;
        }
        public Color ScrollBarThumbPressedBackColor
        {
            get => ChangedOptions.ScrollBarThumbPressedBackColor;
            set => ChangedOptions.ScrollBarThumbPressedBackColor = value;
        }
        public Color ScrollBarBackColor
        {
            get => ChangedOptions.ScrollBarBackColor;
            set => ChangedOptions.ScrollBarBackColor = value;
        }
        public Color ScrollBarButtonBackColor
        {
            get => ChangedOptions.ScrollBarButtonBackColor;
            set => ChangedOptions.ScrollBarButtonBackColor = value;
        }
        public Color ScrollBarButtonForeColor
        {
            get => ChangedOptions.ScrollBarButtonForeColor;
            set => ChangedOptions.ScrollBarButtonForeColor = value;
        }
        public Color ScrollBarButtonBorderColor
        {
            get => ChangedOptions.ScrollBarButtonBorderColor;
            set => ChangedOptions.ScrollBarButtonBorderColor = value;
        }
        public Color ScrollBarButtonDisabledBackColor
        {
            get => ChangedOptions.ScrollBarButtonDisabledBackColor;
            set => ChangedOptions.ScrollBarButtonDisabledBackColor = value;
        }
        public Color ScrollBarButtonDisabledForeColor
        {
            get => ChangedOptions.ScrollBarButtonDisabledForeColor;
            set => ChangedOptions.ScrollBarButtonDisabledForeColor = value;
        }
        public Color ScrollBarButtonDisabledBorderColor
        {
            get => ChangedOptions.ScrollBarButtonDisabledBorderColor;
            set => ChangedOptions.ScrollBarButtonDisabledBorderColor = value;
        }
        public Color ScrollBarButtonMouseOverBackColor
        {
            get => ChangedOptions.ScrollBarButtonMouseOverBackColor;
            set => ChangedOptions.ScrollBarButtonMouseOverBackColor = value;
        }
        public Color ScrollBarButtonMouseOverForeColor
        {
            get => ChangedOptions.ScrollBarButtonMouseOverForeColor;
            set => ChangedOptions.ScrollBarButtonMouseOverForeColor = value;
        }
        public Color ScrollBarButtonMouseOverBorderColor
        {
            get => ChangedOptions.ScrollBarButtonMouseOverBorderColor;
            set => ChangedOptions.ScrollBarButtonMouseOverBorderColor = value;
        }
        public Color ScrollBarButtonPressedBackColor
        {
            get => ChangedOptions.ScrollBarButtonPressedBackColor;
            set => ChangedOptions.ScrollBarButtonPressedBackColor = value;
        }
        public Color ScrollBarButtonPressedForeColor
        {
            get => ChangedOptions.ScrollBarButtonPressedForeColor;
            set => ChangedOptions.ScrollBarButtonPressedForeColor = value;
        }
        public Color ScrollBarButtonPressedBorderColor
        {
            get => ChangedOptions.ScrollBarButtonPressedBorderColor;
            set => ChangedOptions.ScrollBarButtonPressedBorderColor = value;
        }


        public Color YouTubeLiveBackColor
        {
            get { return ChangedOptions.YouTubeLiveBackColor; }
            set { ChangedOptions.YouTubeLiveBackColor = value; }
        }
        public Color YouTubeLiveForeColor
        {
            get { return ChangedOptions.YouTubeLiveForeColor; }
            set { ChangedOptions.YouTubeLiveForeColor = value; }
        }
        public Color OpenrecBackColor
        {
            get { return ChangedOptions.OpenrecBackColor; }
            set { ChangedOptions.OpenrecBackColor = value; }
        }
        public Color OpenrecForeColor
        {
            get { return ChangedOptions.OpenrecForeColor; }
            set { ChangedOptions.OpenrecForeColor = value; }
        }
        public Color BLiveBackColor
        {
            get { return ChangedOptions.BLiveBackColor; }
            set { ChangedOptions.BLiveBackColor = value; }
        }
        public Color BLiveForeColor
        {
            get { return ChangedOptions.BLiveForeColor; }
            set { ChangedOptions.BLiveForeColor = value; }
        }
        public Color MixchBackColor
        {
            get { return ChangedOptions.MixchBackColor; }
            set { ChangedOptions.MixchBackColor = value; }
        }
        public Color MixchForeColor
        {
            get { return ChangedOptions.MixchForeColor; }
            set { ChangedOptions.MixchForeColor = value; }
        }
        public Color TwitchBackColor
        {
            get { return ChangedOptions.TwitchBackColor; }
            set { ChangedOptions.TwitchBackColor = value; }
        }
        public Color TwitchForeColor
        {
            get { return ChangedOptions.TwitchForeColor; }
            set { ChangedOptions.TwitchForeColor = value; }
        }
        public Color NicoLiveBackColor
        {
            get { return ChangedOptions.NicoLiveBackColor; }
            set { ChangedOptions.NicoLiveBackColor = value; }
        }
        public Color NicoLiveForeColor
        {
            get { return ChangedOptions.NicoLiveForeColor; }
            set { ChangedOptions.NicoLiveForeColor = value; }
        }
        public Color TwicasBackColor
        {
            get { return ChangedOptions.TwicasBackColor; }
            set { ChangedOptions.TwicasBackColor = value; }
        }
        public Color TwicasForeColor
        {
            get { return ChangedOptions.TwicasForeColor; }
            set { ChangedOptions.TwicasForeColor = value; }
        }
        public Color LineLiveBackColor
        {
            get { return ChangedOptions.LineLiveBackColor; }
            set { ChangedOptions.LineLiveBackColor = value; }
        }
        public Color LineLiveForeColor
        {
            get { return ChangedOptions.LineLiveForeColor; }
            set { ChangedOptions.LineLiveForeColor = value; }
        }

        public Color WhowatchBackColor
        {
            get { return ChangedOptions.WhowatchBackColor; }
            set { ChangedOptions.WhowatchBackColor = value; }
        }
        public Color WhowatchForeColor
        {
            get { return ChangedOptions.WhowatchForeColor; }
            set { ChangedOptions.WhowatchForeColor = value; }
        }

        public Color MirrativBackColor
        {
            get { return ChangedOptions.MirrativBackColor; }
            set { ChangedOptions.MirrativBackColor = value; }
        }
        public Color MirrativForeColor
        {
            get { return ChangedOptions.MirrativForeColor; }
            set { ChangedOptions.MirrativForeColor = value; }
        }

        public Color PeriscopeBackColor
        {
            get { return ChangedOptions.PeriscopeBackColor; }
            set { ChangedOptions.PeriscopeBackColor = value; }
        }
        public Color PeriscopeForeColor
        {
            get { return ChangedOptions.PeriscopeForeColor; }
            set { ChangedOptions.PeriscopeForeColor = value; }
        }

        public IOptions OriginOptions { get; private set; }
        public IOptions ChangedOptions { get; private set; }
        public ObservableCollection<FontFamilyViewModel> FontFamillyCollection { get; private set; }
        public ObservableCollection<int> FontSizeCollection { get; private set; }
        public ObservableCollection<InfoType> InfoTypeCollection { get; private set; }
        public InfoType SelectedInfoType
        {
            get => ChangedOptions.ShowingInfoLevel;
            set => ChangedOptions.ShowingInfoLevel = value;
        }
        public MainOptionsViewModel(IOptions options)
        {
            Init(options);
        }

        private void Init(IOptions options)
        {
            OriginOptions = options;
            ChangedOptions = options.Clone() as IOptions;
            ChangedOptions.PropertyChanged += ChangedOptions_PropertyChanged;

            var fontList = Fonts.SystemFontFamilies.OrderBy(f => f.ToString()).Select(f => new FontFamilyViewModel(f, CultureInfo.CurrentCulture));
            FontFamillyCollection = new ObservableCollection<FontFamilyViewModel>(fontList);
            //FontFamily = new FontFamilyViewModel(new FontFamily("Meiryo"), CultureInfo.CurrentCulture);
            FontFamily = new FontFamilyViewModel(ChangedOptions.FontFamily, CultureInfo.CurrentCulture);
            FirstCommentFontFamily = new FontFamilyViewModel(ChangedOptions.FirstCommentFontFamily, CultureInfo.CurrentCulture);

            var sizeList = Enumerable.Range(6, 40);
            FontSizeCollection = new ObservableCollection<int>(sizeList);
            //FontSize = 10;

            InfoTypeCollection = new ObservableCollection<InfoType>(Enum.GetValues(typeof(InfoType)).Cast<InfoType>());
            SelectedInfoType = options.ShowingInfoLevel;

            DefaultThemeCommand = new RelayCommand(DefaultTheme);
            DarkThemeCommand = new RelayCommand(DarkTheme);
            SukesukeThemeCommand = new RelayCommand(SukesukeTheme);

        }

        private void ChangedOptions_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ChangedOptions.TitleBackColor):
                    RaisePropertyChanged(nameof(TitleBackColor));
                    break;
                case nameof(ChangedOptions.TitleForeColor):
                    RaisePropertyChanged(nameof(TitleForeColor));
                    break;
                case nameof(ChangedOptions.ViewBackColor):
                    RaisePropertyChanged(nameof(ViewBackColor));
                    break;
                case nameof(ChangedOptions.MenuBackColor):
                    RaisePropertyChanged(nameof(MenuBackColor));
                    break;
                case nameof(ChangedOptions.MenuForeColor):
                    RaisePropertyChanged(nameof(MenuForeColor));
                    break;
                case nameof(ChangedOptions.MenuItemCheckMarkColor):
                    RaisePropertyChanged(nameof(MenuItemCheckMarkColor));
                    break;
                case nameof(ChangedOptions.MenuItemMouseOverBackColor):
                    RaisePropertyChanged(nameof(MenuItemMouseOverBackColor));
                    break;
                case nameof(ChangedOptions.MenuItemMouseOverForeColor):
                    RaisePropertyChanged(nameof(MenuItemMouseOverForeColor));
                    break;
                case nameof(ChangedOptions.MenuItemMouseOverBorderColor):
                    RaisePropertyChanged(nameof(MenuItemMouseOverBorderColor));
                    break;
                case nameof(ChangedOptions.MenuItemMouseOverCheckMarkColor):
                    RaisePropertyChanged(nameof(MenuItemMouseOverCheckMarkColor));
                    break;
                case nameof(ChangedOptions.MenuSeparatorBackColor):
                    RaisePropertyChanged(nameof(MenuSeparatorBackColor));
                    break;
                case nameof(ChangedOptions.MenuPopupBorderColor):
                    RaisePropertyChanged(nameof(MenuPopupBorderColor));
                    break;
                case nameof(ChangedOptions.ButtonBackColor):
                    RaisePropertyChanged(nameof(ButtonBackColor));
                    break;
                case nameof(ChangedOptions.ButtonForeColor):
                    RaisePropertyChanged(nameof(ButtonForeColor));
                    break;
                case nameof(ChangedOptions.ButtonBorderColor):
                    RaisePropertyChanged(nameof(ButtonBorderColor));
                    break;
                case nameof(ChangedOptions.WindowBorderColor):
                    RaisePropertyChanged(nameof(WindowBorderColor));
                    break;
                case nameof(ChangedOptions.SystemButtonBackColor):
                    RaisePropertyChanged(nameof(SystemButtonBackColor));
                    break;
                case nameof(ChangedOptions.SystemButtonForeColor):
                    RaisePropertyChanged(nameof(SystemButtonForeColor));
                    break;
                case nameof(ChangedOptions.SystemButtonBorderColor):
                    RaisePropertyChanged(nameof(SystemButtonBorderColor));
                    break;
                case nameof(ChangedOptions.SystemButtonMouseOverBackColor):
                    RaisePropertyChanged(nameof(SystemButtonMouseOverBackColor));
                    break;
                case nameof(ChangedOptions.SystemButtonMouseOverForeColor):
                    RaisePropertyChanged(nameof(SystemButtonMouseOverForeColor));
                    break;
                case nameof(ChangedOptions.SystemButtonMouseOverBorderColor):
                    RaisePropertyChanged(nameof(SystemButtonMouseOverBorderColor));
                    break;
                case nameof(ChangedOptions.CommentListBackColor):
                    RaisePropertyChanged(nameof(CommentListBackColor));
                    break;
                case nameof(ChangedOptions.CommentListBorderColor):
                    RaisePropertyChanged(nameof(CommentListBorderColor));
                    break;
                case nameof(ChangedOptions.CommentListHeaderBackColor):
                    RaisePropertyChanged(nameof(CommentListHeaderBackColor));
                    break;
                case nameof(ChangedOptions.CommentListHeaderForeColor):
                    RaisePropertyChanged(nameof(CommentListHeaderForeColor));
                    break;
                case nameof(ChangedOptions.CommentListHeaderBorderColor):
                    RaisePropertyChanged(nameof(CommentListHeaderBorderColor));
                    break;
                case nameof(ChangedOptions.CommentListSeparatorColor):
                    RaisePropertyChanged(nameof(CommentListSeparatorColor));
                    break;
                case nameof(ChangedOptions.ConnectionListBackColor):
                    RaisePropertyChanged(nameof(ConnectionListBackColor));
                    break;
                case nameof(ChangedOptions.ConnectionListHeaderBackColor):
                    RaisePropertyChanged(nameof(ConnectionListHeaderBackColor));
                    break;
                case nameof(ChangedOptions.ConnectionListHeaderForeColor):
                    RaisePropertyChanged(nameof(ConnectionListHeaderForeColor));
                    break;
                case nameof(ChangedOptions.ConnectionListRowBackColor):
                    RaisePropertyChanged(nameof(ConnectionListRowBackColor));
                    break;

                case nameof(ChangedOptions.ScrollBarBackColor):
                    RaisePropertyChanged(nameof(ScrollBarBackColor));
                    break;
                case nameof(ChangedOptions.ScrollBarBorderColor):
                    RaisePropertyChanged(nameof(ScrollBarBorderColor));
                    break;
                case nameof(ChangedOptions.ScrollBarThumbBackColor):
                    RaisePropertyChanged(nameof(ScrollBarThumbBackColor));
                    break;
                case nameof(ChangedOptions.ScrollBarThumbMouseOverBackColor):
                    RaisePropertyChanged(nameof(ScrollBarThumbMouseOverBackColor));
                    break;
                case nameof(ChangedOptions.ScrollBarThumbPressedBackColor):
                    RaisePropertyChanged(nameof(ScrollBarThumbPressedBackColor));
                    break;

                case nameof(ChangedOptions.ScrollBarButtonBackColor):
                    RaisePropertyChanged(nameof(ScrollBarButtonBackColor));
                    break;
                case nameof(ChangedOptions.ScrollBarButtonForeColor):
                    RaisePropertyChanged(nameof(ScrollBarButtonForeColor));
                    break;
                case nameof(ChangedOptions.ScrollBarButtonBorderColor):
                    RaisePropertyChanged(nameof(ScrollBarButtonBorderColor));
                    break;

                case nameof(ChangedOptions.ScrollBarButtonDisabledBackColor):
                    RaisePropertyChanged(nameof(ScrollBarButtonDisabledBackColor));
                    break;
                case nameof(ChangedOptions.ScrollBarButtonDisabledForeColor):
                    RaisePropertyChanged(nameof(ScrollBarButtonDisabledForeColor));
                    break;
                case nameof(ChangedOptions.ScrollBarButtonDisabledBorderColor):
                    RaisePropertyChanged(nameof(ScrollBarButtonDisabledBorderColor));
                    break;
                case nameof(ChangedOptions.ScrollBarButtonMouseOverBackColor):
                    RaisePropertyChanged(nameof(ScrollBarButtonMouseOverBackColor));
                    break;
                case nameof(ChangedOptions.ScrollBarButtonMouseOverForeColor):
                    RaisePropertyChanged(nameof(ScrollBarButtonMouseOverForeColor));
                    break;
                case nameof(ChangedOptions.ScrollBarButtonMouseOverBorderColor):
                    RaisePropertyChanged(nameof(ScrollBarButtonMouseOverBorderColor));
                    break;
                case nameof(ChangedOptions.ScrollBarButtonPressedBackColor):
                    RaisePropertyChanged(nameof(ScrollBarButtonPressedBackColor));
                    break;
                case nameof(ChangedOptions.ScrollBarButtonPressedForeColor):
                    RaisePropertyChanged(nameof(ScrollBarButtonPressedForeColor));
                    break;
                case nameof(ChangedOptions.ScrollBarButtonPressedBorderColor):
                    RaisePropertyChanged(nameof(ScrollBarButtonPressedBorderColor));
                    break;
            }
        }

        private void DefaultTheme()
        {
            var black = Colors.Black;
            var white = Colors.White;
            var lightGray = Color.FromArgb(0xFF, 0xDD, 0xDD, 0xDD);
            var control = Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF0);
            TitleBackColor = white;
            TitleForeColor = black;
            ViewBackColor = white;
            WindowBorderColor = black;
            SystemButtonBackColor = white;
            SystemButtonForeColor = black;
            SystemButtonBorderColor = white;
            SystemButtonMouseOverBackColor = lightGray;
            SystemButtonMouseOverForeColor = black;
            SystemButtonMouseOverBorderColor = lightGray;
            MenuBackColor = control;
            MenuForeColor = black;
            MenuItemCheckMarkColor = Colors.Blue;
            MenuItemMouseOverBackColor = control;
            MenuItemMouseOverForeColor = black;
            MenuItemMouseOverBorderColor= Color.FromArgb(0xFF, 0x26, 0xA0, 0xDA);
            MenuItemMouseOverCheckMarkColor = Colors.Blue;
            MenuSeparatorBackColor = lightGray;
            MenuPopupBorderColor = black;

            ButtonBackColor = lightGray;
            ButtonForeColor = black;
            ButtonBorderColor = black;
            CommentListBackColor = control;
            CommentListHeaderBackColor = Color.FromArgb(0xFF, 0xF4, 0xF5, 0xF7);
            CommentListHeaderForeColor = black;
            CommentListHeaderBorderColor = black;

            CommentListBorderColor = black;
            CommentListSeparatorColor = Color.FromArgb(0xFF, 0xE4, 0xE5, 0xE7);

            ScrollBarBackColor = control;
            ScrollBarBorderColor = control;
            ScrollBarThumbBackColor = Color.FromArgb(0xFF, 0xCD, 0xCD, 0xCD);
            ScrollBarButtonBackColor = control;
            ScrollBarButtonForeColor = Color.FromArgb(0xFF, 0x9F, 0x9F, 0x9F);
            ScrollBarButtonBorderColor = control;
            ScrollBarButtonDisabledBackColor = control;
            ScrollBarButtonDisabledForeColor = Color.FromArgb(0xFF, 0xD5, 0xD5, 0xD5);
            ScrollBarButtonDisabledBorderColor = control;
            ScrollBarButtonMouseOverBackColor= Color.FromArgb(0xFF, 0xDA, 0xDA, 0xDA);
            ScrollBarButtonMouseOverForeColor = black;
            ScrollBarButtonMouseOverBorderColor = Color.FromArgb(0xFF, 0xDA, 0xDA, 0xDA);
            ScrollBarButtonPressedBackColor = Color.FromArgb(0xFF, 0x60, 0x60, 0x60);
            ScrollBarButtonPressedForeColor = white;
            ScrollBarButtonPressedBorderColor= Color.FromArgb(0xFF, 0x60, 0x60, 0x60);
        }
        private void DarkTheme()
        {
            var dark = Color.FromArgb(0xFF, 0x2D, 0x2D, 0x30);
            var lightDark = Color.FromArgb(0xFF, 0x3E, 0x3E, 0x42);
            var white = Colors.White;
            TitleBackColor = dark;
            TitleForeColor = white;
            ViewBackColor = dark;
            WindowBorderColor = dark;
            SystemButtonBackColor = dark;
            SystemButtonForeColor = white;
            SystemButtonBorderColor = white;
            SystemButtonMouseOverBackColor = dark;
            SystemButtonMouseOverForeColor = white;
            SystemButtonMouseOverBorderColor = white;
            MenuBackColor = dark;
            MenuForeColor = white;
            MenuItemCheckMarkColor = white;
            MenuItemMouseOverBackColor = lightDark;
            MenuItemMouseOverForeColor = white;
            MenuItemMouseOverBorderColor = white;
            MenuItemMouseOverCheckMarkColor = white;
            MenuSeparatorBackColor = lightDark;
            MenuPopupBorderColor = lightDark;
            ButtonBackColor = lightDark;
            ButtonForeColor = white;
            ButtonBorderColor = Colors.DarkGray;
            CommentListBackColor = dark;
            CommentListHeaderBackColor = lightDark;
            CommentListHeaderForeColor = white;
            CommentListBorderColor = lightDark;
            CommentListSeparatorColor = lightDark;
            ScrollBarBorderColor = lightDark;
            ScrollBarThumbBackColor = Color.FromArgb(0xFF, 0x68, 0x68, 0x68);
            //ScrollBarThumbMouseOverBackColor
            //ScrollBarThumbPressedBackColor
            ScrollBarBackColor = lightDark;
            ScrollBarButtonDisabledBackColor = lightDark;
            ScrollBarButtonDisabledForeColor = Color.FromArgb(0xFF, 0x99, 0x99, 0x99);
        }
        private void SukesukeTheme()
        {
            var clear = Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF);
            var black = Colors.Black;
            var white = Colors.White;
            TitleBackColor = clear;
            TitleForeColor = white;
            ViewBackColor = clear;
            WindowBorderColor = black;
            SystemButtonBackColor = clear;
            SystemButtonForeColor = white;
            SystemButtonBorderColor = black;
            SystemButtonMouseOverBackColor = Colors.Red;
            SystemButtonMouseOverForeColor = white;
            SystemButtonMouseOverBorderColor = white;
            MenuBackColor = clear;
            MenuForeColor = white;
            MenuItemCheckMarkColor = black;
            MenuItemMouseOverBackColor = Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF0);
            MenuItemMouseOverForeColor = black;
            MenuItemMouseOverCheckMarkColor = black;
            MenuSeparatorBackColor = white;
            MenuPopupBorderColor = black;
            ButtonBackColor = clear;
            ButtonForeColor = white;
            ButtonBorderColor = black;
            CommentListBackColor = clear;
            CommentListHeaderBackColor = clear;
            CommentListHeaderForeColor = white;
            CommentListBorderColor = clear;
            CommentListSeparatorColor = clear;
        }
        public MainOptionsViewModel()
        {
            if ((bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue))
            {
                var options = new DynamicOptionsTest
                {
                    ForeColor = Colors.Red,
                    BackColor = Colors.Black,
                    InfoBackColor = Colors.Yellow,
                    InfoForeColor = Colors.Black,
                    SelectedRowBackColor = Colors.Aqua,
                    SelectedRowForeColor = Colors.Pink,
                    VerticalGridLineColor = Colors.Green,
                    HorizontalGridLineColor = Colors.LightGray,
                    FontFamily = new FontFamily("Meiryo"),
                    FirstCommentFontFamily = new FontFamily("MS Gothic"),
                    FontSize = 16,
                    FirstCommentFontSize = 24,
                    IsPixelScrolling = false,
                };
                Init(options);
                IsBold = false;
                IsFirstCommentBold = true;
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        #region INotifyPropertyChanged
        [NonSerialized]
        private System.ComponentModel.PropertyChangedEventHandler _propertyChanged;
        /// <summary>
        ///
        /// </summary>
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged
        {
            add { _propertyChanged += value; }
            remove { _propertyChanged -= value; }
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="propertyName"></param>
        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            _propertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        public Task AfterCommentAdded()
        {
            throw new NotImplementedException();
        }
        #endregion
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
}
