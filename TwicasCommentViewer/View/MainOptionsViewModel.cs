using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Common;
using Common.Wpf;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
namespace TwicasCommentViewer
{
    class MainOptionsViewModel :ViewModelBase
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
        #region HorizontalGridLine
        public bool IsShowHorizontalGridLine
        {
            get { return ChangedOptions.IsShowHorizontalGridLine; }
            set { ChangedOptions.IsShowHorizontalGridLine = value; }
        }
        public Color HorizontalGridLineColor
        {
            get { return ChangedOptions.HorizontalGridLineColor; }
            set { ChangedOptions.HorizontalGridLineColor = value; }
        }
        #endregion //HorizontalGridLine

        #region VerticalGridLin
        public bool IsShowVerticalGridLine
        {
            get { return ChangedOptions.IsShowVerticalGridLine; }
            set { ChangedOptions.IsShowVerticalGridLine = value; }
        }
        public Color VerticalGridLineColor
        {
            get { return ChangedOptions.VerticalGridLineColor; }
            set { ChangedOptions.VerticalGridLineColor = value; }
        }
        #endregion //VerticalGridLine
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
        public bool IsEllipseThumbnail
        {
            get { return ChangedOptions.IsEllipseThumbnail; }
            set { ChangedOptions.IsEllipseThumbnail = value; }
        }
        public bool IsSendCommentData
        {
            get { return ChangedOptions.IsSendCommentData; }
            set { ChangedOptions.IsSendCommentData = value; }
        }
        public bool IsShowComments
        {
            get { return ChangedOptions.IsShowComments; }
            set { ChangedOptions.IsShowComments = value; }
        }
        public int CommentUpdateInterval
        {
            get { return ChangedOptions.CommentUpdateInterval; }
            set { ChangedOptions.CommentUpdateInterval = value; }
        }
        private readonly IOptions _origin;
        private readonly IOptions changed;
        public IOptions OriginOptions { get { return _origin; } }
        public IOptions ChangedOptions { get { return changed; } }
        Dictionary<InfoType, string> _infoTypeTooltipTexts = new Dictionary<InfoType, string>
        {
            { InfoType.None, "何も表示しません" },
            { InfoType.Debug, "" },
            { InfoType.Notice, "" },
            { InfoType.Error, "致命的なエラー情報のみ" },
        };
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
}
