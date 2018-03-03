using System;
using System.Collections.Generic;
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
using Common.Wpf;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SitePlugin;
namespace YouTubeLiveCommentViewer
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
    class MainOptionsViewModel:ViewModelBase
    {
        public ICommand ShowFontSelectorCommand { get; }
        private void ShowFontSelector()
        {
            Messenger.Default.Send(new SetFontMessage(ChangedOptions.FontFamily, ChangedOptions.FontStyle, ChangedOptions.FontWeight, ChangedOptions.FontSize));
            Messenger.Default.Send(new ShowFontSelectorViewOkMessage());
            MessengerInstance.Send(new GetFontMessage((fontFamily, fontStyle, fontWeight, fontSize) =>
            {
                ChangedOptions.FontFamily = fontFamily;
                ChangedOptions.FontStyle = fontStyle;
                ChangedOptions.FontWeight = fontWeight;
                ChangedOptions.FontSize = fontSize;
                RaisePropertyChanged(nameof(CommentFont));
            }));
        }
        private string ConvertFontFamilyToName(FontFamily fontFamily, CultureInfo culture)
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
        public string CommentFont
        {
            get
            {
                return $"{ConvertFontFamilyToName(FontFamily, CultureInfo.CurrentCulture)}, {FontSize}pt";
            }
        }
        public FontFamily FontFamily
        {
            get { return ChangedOptions.FontFamily; }
            set { ChangedOptions.FontFamily = value; }
        }
        public FontStyle FontStyle
        {
            get { return ChangedOptions.FontStyle; }
            set { ChangedOptions.FontStyle = value; }
        }
        public FontWeight FontWeight
        {
            get { return ChangedOptions.FontWeight; }
            set { ChangedOptions.FontWeight = value; }
        }
        public int FontSize
        {
            get { return ChangedOptions.FontSize; }
            set { ChangedOptions.FontSize = value; }
        }
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
        public Color HorizontalGridLineColor
        {
            get { return ChangedOptions.HorizontalGridLineColor; }
            set { ChangedOptions.HorizontalGridLineColor = value; }
        }
        public Color VerticalGridLineColor
        {
            get { return ChangedOptions.VerticalGridLineColor; }
            set { ChangedOptions.VerticalGridLineColor = value; }
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
        public bool IsPixelScrolling
        {
            get { return ChangedOptions.IsPixelScrolling; }
            set { ChangedOptions.IsPixelScrolling = value; }
        }
        private readonly IOptions _origin;
        private readonly IOptions changed;
        public IOptions OriginOptions { get { return _origin; } }
        public IOptions ChangedOptions { get { return changed; } }
        public MainOptionsViewModel(IOptions options)
        {
            _origin = options;
            changed = options.Clone() as IOptions;
            ShowFontSelectorCommand = new RelayCommand(ShowFontSelector);
        }
        public MainOptionsViewModel()
        {
            if (GalaSoft.MvvmLight.ViewModelBase.IsInDesignModeStatic)
            {
                _origin = new DynamicOptionsTest
                {
                    ForeColor = Colors.Red,
                    BackColor = Colors.Black,
                    InfoBackColor = Colors.Yellow,
                    InfoForeColor = Colors.Black,
                    SelectedRowBackColor=Colors.Aqua,
                    SelectedRowForeColor=Colors.Pink,
                    VerticalGridLineColor = Colors.Green,
                    HorizontalGridLineColor = Colors.LightGray,
                };
                changed = _origin.Clone() as IOptions;
            }
            else
            {
                throw new NotSupportedException();
            }
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
}
