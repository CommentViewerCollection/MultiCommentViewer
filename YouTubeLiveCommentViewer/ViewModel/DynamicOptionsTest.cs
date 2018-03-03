using Common;
using SitePlugin;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace YouTubeLiveCommentViewer
{
    class DynamicOptionsTest : DynamicOptionsBase, IOptions
    {
        public string PluginDir => "plugins";

        public FontFamily FontFamily { get => GetValue(); set => SetValue(value); }
        public FontStyle FontStyle { get => GetValue(); set => SetValue(value); }
        public FontWeight FontWeight { get => GetValue(); set => SetValue(value); }
        public int FontSize { get => GetValue(); set => SetValue(value); }
        public FontFamily FirstCommentFontFamily { get => GetValue(); set => SetValue(value); }
        public FontStyle FirstCommentFontStyle { get => GetValue(); set => SetValue(value); }
        public FontWeight FirstCommentFontWeight { get => GetValue(); set => SetValue(value); }
        public int FirstCommentFontSize { get => GetValue(); set => SetValue(value); }
        public string SettingsDirPath { get => GetValue(); set => SetValue(value); }
        public Color BackColor { get => GetValue(); set => SetValue(value); }
        public Color ForeColor { get => GetValue(); set => SetValue(value); }
        public double MainViewHeight { get => GetValue(); set => SetValue(value); }
        public double MainViewWidth { get => GetValue(); set => SetValue(value); }
        public double MainViewLeft { get => GetValue(); set => SetValue(value); }
        public double MainViewTop { get => GetValue(); set => SetValue(value); }
        public Color HorizontalGridLineColor { get => GetValue(); set => SetValue(value); }
        public Color VerticalGridLineColor { get => GetValue(); set => SetValue(value); }
        public Color InfoForeColor { get => GetValue(); set => SetValue(value); }
        public Color InfoBackColor { get => GetValue(); set => SetValue(value); }
        public Color SelectedRowBackColor { get => GetValue(); set => SetValue(value); }
        public Color SelectedRowForeColor { get => GetValue(); set => SetValue(value); }
        //public double ConnectionNameWidth { get => GetValue(); set => SetValue(value); }
        //public bool IsShowConnectionName { get => GetValue(); set => SetValue(value); }
        //public int ConnectionNameDisplayIndex { get => GetValue(); set => SetValue(value); }
        public double ThumbnailWidth { get => GetValue(); set => SetValue(value); }
        public int ThumbnailDisplayIndex { get => GetValue(); set => SetValue(value); }
        public bool IsShowThumbnail { get => GetValue(); set => SetValue(value); }
        //public double CommentIdWidth { get => GetValue(); set => SetValue(value); }
        //public int CommentIdDisplayIndex { get => GetValue(); set => SetValue(value); }
        //public bool IsShowCommentId { get => GetValue(); set => SetValue(value); }
        public double UsernameWidth { get => GetValue(); set => SetValue(value); }
        public bool IsShowUsername { get => GetValue(); set => SetValue(value); }
        public int UsernameDisplayIndex { get => GetValue(); set => SetValue(value); }
        public double MessageWidth { get => GetValue(); set => SetValue(value); }
        public bool IsShowMessage { get => GetValue(); set => SetValue(value); }
        public int MessageDisplayIndex { get => GetValue(); set => SetValue(value); }
        //public double InfoWidth { get => GetValue(); set => SetValue(value); }
        //public bool IsShowInfo { get => GetValue(); set => SetValue(value); }
        //public int InfoDisplayIndex { get => GetValue(); set => SetValue(value); }
        public bool IsAutoCheckIfUpdateExists { get => GetValue(); set => SetValue(value); }
        public bool IsShowUserId { get => GetValue(); set => SetValue(value); }
        public bool IsShowPostTime { get => GetValue(); set => SetValue(value); }
        public double UserIdWidth { get => GetValue(); set => SetValue(value); }
        public double PostTimeWidth { get => GetValue(); set => SetValue(value); }
        public int UserIdDisplayIndex { get => GetValue(); set => SetValue(value); }
        public int PostTimeDisplayIndex { get => GetValue(); set => SetValue(value); }
        public bool IsTopmost { get => GetValue(); set => SetValue(value); }
        public bool IsUserNameWrapping { get => GetValue(); set => SetValue(value); }
        public bool IsAddingNewCommentTop { get => GetValue(); set => SetValue(value); }
        public bool IsPixelScrolling { get => GetValue(); set => SetValue(value); }
        protected override void Init()
        {
            Dict.Add(nameof(FontFamily), new Item { DefaultValue = new FontFamily("メイリオ, Meiryo"), Predicate = f => true, Serializer = f => FontFamilyToString(f), Deserializer = s => FontFamilyFromString(s) });
            Dict.Add(nameof(FontStyle), new Item { DefaultValue = FontStyles.Normal, Predicate = f => true, Serializer = f => FontStyleToString(f), Deserializer = s => FontStyleFromString(s) });
            Dict.Add(nameof(FontWeight), new Item { DefaultValue = FontWeights.Normal, Predicate = f => true, Serializer = f => FontWeightToString(f), Deserializer = s => FontWeightFromString(s) });
            Dict.Add(nameof(FontSize), new Item { DefaultValue = 14, Predicate = f => f > 0, Serializer = f => f.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(FirstCommentFontFamily), new Item { DefaultValue = new FontFamily("メイリオ, Meiryo"), Predicate = f => true, Serializer = f => FontFamilyToString(f), Deserializer = s => FontFamilyFromString(s) });
            Dict.Add(nameof(FirstCommentFontStyle), new Item { DefaultValue = FontStyles.Normal, Predicate = f => true, Serializer = f => FontStyleToString(f), Deserializer = s => FontStyleFromString(s) });
            Dict.Add(nameof(FirstCommentFontWeight), new Item { DefaultValue = FontWeights.Bold, Predicate = f => true, Serializer = f => FontWeightToString(f), Deserializer = s => FontWeightFromString(s) });
            Dict.Add(nameof(FirstCommentFontSize), new Item { DefaultValue = 14, Predicate = f => f > 0, Serializer = f => f.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(SettingsDirPath), new Item { DefaultValue = "settings", Predicate = s => !string.IsNullOrEmpty(s), Serializer = s => s, Deserializer = s => s });
            Dict.Add(nameof(BackColor), new Item { DefaultValue = ColorFromArgb("#FFEFEFEF"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(ForeColor), new Item { DefaultValue = ColorFromArgb("#FF000000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(MainViewHeight), new Item { DefaultValue = 550, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(MainViewWidth), new Item { DefaultValue = 716, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(MainViewLeft), new Item { DefaultValue = 0, Predicate = n => n >= 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(MainViewTop), new Item { DefaultValue = 0, Predicate = n => n >= 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(HorizontalGridLineColor), new Item { DefaultValue = ColorFromArgb("#FFDCDCDC"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(VerticalGridLineColor), new Item { DefaultValue = ColorFromArgb("#FFDCDCDC"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(InfoForeColor), new Item { DefaultValue = ColorFromArgb("#FF000000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(InfoBackColor), new Item { DefaultValue = ColorFromArgb("#FFFFFF00"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(SelectedRowBackColor), new Item { DefaultValue = ColorFromArgb("#FF0078D7"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(SelectedRowForeColor), new Item { DefaultValue = ColorFromArgb("#FFFFFFFF"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });

            
            Dict.Add(nameof(ThumbnailWidth), new Item { DefaultValue = 32, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            
            Dict.Add(nameof(UsernameWidth), new Item { DefaultValue = 100, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(MessageWidth), new Item { DefaultValue = 300, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            
            Dict.Add(nameof(UserIdWidth), new Item { DefaultValue = 100, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(PostTimeWidth), new Item { DefaultValue = 100, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            
            Dict.Add(nameof(ThumbnailDisplayIndex), new Item { DefaultValue = 0, Predicate = n => n >= 0, Serializer = n => n.ToString(), Deserializer = s => int.Parse(s) });
            
            Dict.Add(nameof(UsernameDisplayIndex), new Item { DefaultValue = 1, Predicate = n => n >= 0, Serializer = n => n.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(UserIdDisplayIndex), new Item { DefaultValue = 2, Predicate = n => n >= 0, Serializer = n => n.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(PostTimeDisplayIndex), new Item { DefaultValue = 3, Predicate = n => n >= 0, Serializer = n => n.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(MessageDisplayIndex), new Item { DefaultValue = 4, Predicate = n => n >= 0, Serializer = n => n.ToString(), Deserializer = s => int.Parse(s) });

            
            Dict.Add(nameof(IsShowThumbnail), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            
            Dict.Add(nameof(IsShowUsername), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsShowMessage), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            
            Dict.Add(nameof(IsShowUserId), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsShowPostTime), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });

            Dict.Add(nameof(IsAutoCheckIfUpdateExists), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });

            Dict.Add(nameof(IsTopmost), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });

            Dict.Add(nameof(IsUserNameWrapping), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsAddingNewCommentTop), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsPixelScrolling), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });

            //以下使わないけど、IOptionsに要求される
            //Dict.Add(nameof(ConnectionNameWidth), new Item { DefaultValue = 100, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            //Dict.Add(nameof(CommentIdWidth), new Item { DefaultValue = 100, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            //Dict.Add(nameof(InfoWidth), new Item { DefaultValue = 100, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            //Dict.Add(nameof(ConnectionNameDisplayIndex), new Item { DefaultValue = 0, Predicate = n => n >= 0, Serializer = n => n.ToString(), Deserializer = s => int.Parse(s) });
            //Dict.Add(nameof(CommentIdDisplayIndex), new Item { DefaultValue = 2, Predicate = n => n >= 0, Serializer = n => n.ToString(), Deserializer = s => int.Parse(s) });
            //Dict.Add(nameof(InfoDisplayIndex), new Item { DefaultValue = 5, Predicate = n => n >= 0, Serializer = n => n.ToString(), Deserializer = s => int.Parse(s) });
            //Dict.Add(nameof(IsShowConnectionName), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            //Dict.Add(nameof(IsShowCommentId), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            //Dict.Add(nameof(IsShowInfo), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
        }
        public ICommentOptions Clone()
        {
            return this.MemberwiseClone() as ICommentOptions;
        }

        public void Set(ICommentOptions options)
        {
            var props = typeof(ICommentOptions).GetProperties();
            foreach (var prop in props)
            {
                if (prop.CanRead && prop.CanWrite)
                {
                    var item = Dict[prop.Name];
                    var newVal = prop.GetValue(options);
                    if (item.Predicate(newVal))
                    {
                        item.Value = newVal;
                        RaisePropertyChanged(prop.Name);
                    }
                }
            }
        }
        #region Converters
        private FontFamily FontFamilyFromString(string str)
        {
            return new FontFamily(str);
        }
        private string FontFamilyToString(FontFamily family)
        {
            return family.FamilyNames.Values.First();
        }
        private FontStyle FontStyleFromString(string str)
        {
            return (FontStyle)new FontStyleConverter().ConvertFromString(str);
        }
        private string FontStyleToString(FontStyle style)
        {
            return new FontStyleConverter().ConvertToString(style);
        }
        private FontWeight FontWeightFromString(string str)
        {
            return (FontWeight)new FontWeightConverter().ConvertFromString(str);
        }
        private string FontWeightToString(FontWeight weight)
        {
            return new FontWeightConverter().ConvertToString(weight);
        }
        private Color ColorFromArgb(string argb)
        {
            if (argb == null)
                throw new ArgumentNullException("argb");
            var pattern = "#(?<a>[0-9a-fA-F]{2})(?<r>[0-9a-fA-F]{2})(?<g>[0-9a-fA-F]{2})(?<b>[0-9a-fA-F]{2})";
            var match = System.Text.RegularExpressions.Regex.Match(argb, pattern, System.Text.RegularExpressions.RegexOptions.Compiled);

            if (!match.Success)
            {
                throw new ArgumentException("形式が不正");
            }
            else
            {
                var a = byte.Parse(match.Groups["a"].Value, System.Globalization.NumberStyles.HexNumber);
                var r = byte.Parse(match.Groups["r"].Value, System.Globalization.NumberStyles.HexNumber);
                var g = byte.Parse(match.Groups["g"].Value, System.Globalization.NumberStyles.HexNumber);
                var b = byte.Parse(match.Groups["b"].Value, System.Globalization.NumberStyles.HexNumber);
                return Color.FromArgb(a, r, g, b);
            }
        }
        private string ColorToArgb(Color color)
        {
            var argb = color.ToString();
            return argb;
        }
        #endregion
    }
}
