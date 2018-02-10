using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using SitePlugin;
using Common;
namespace MultiCommentViewer.Test
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
        public double ConnectionNameWidth { get => GetValue(); set => SetValue(value); }
        public bool IsShowConnectionName { get => GetValue(); set => SetValue(value); }
        public int ConnectionNameDisplayIndex { get => GetValue(); set => SetValue(value); }
        public double ThumbnailWidth { get => GetValue(); set => SetValue(value); }
        public int ThumbnailDisplayIndex { get => GetValue(); set => SetValue(value); }
        public bool IsShowThumbnail { get => GetValue(); set => SetValue(value); }
        public double CommentIdWidth { get => GetValue(); set => SetValue(value); }
        public int CommentIdDisplayIndex { get => GetValue(); set => SetValue(value); }
        public bool IsShowCommentId { get => GetValue(); set => SetValue(value); }
        public double UsernameWidth { get => GetValue(); set => SetValue(value); }
        public bool IsShowUsername { get => GetValue(); set => SetValue(value); }
        public int UsernameDisplayIndex { get => GetValue(); set => SetValue(value); }
        public double MessageWidth { get => GetValue(); set => SetValue(value); }
        public bool IsShowMessage { get => GetValue(); set => SetValue(value); }
        public int MessageDisplayIndex { get => GetValue(); set => SetValue(value); }
        public double InfoWidth { get => GetValue(); set => SetValue(value); }
        public bool IsShowInfo { get => GetValue(); set => SetValue(value); }
        public int InfoDisplayIndex { get => GetValue(); set => SetValue(value); }
        protected override void Init()
        {
            Dict.Add(nameof(FontFamily), new Item { Value = new FontFamily("メイリオ"), DefaultValue = new FontFamily("メイリオ"), Predicate = f => true, Serializer = f => FontFamilyToString(f), Deserializer = s => FontFamilyFromString(s) });
            Dict.Add(nameof(FontStyle), new Item { Value = FontStyles.Normal, DefaultValue = FontStyles.Normal, Predicate = f => true, Serializer = f => FontStyleToString(f), Deserializer = s => FontStyleFromString(s) });
            Dict.Add(nameof(FontWeight), new Item { Value = FontWeights.Normal, DefaultValue = FontWeights.Normal, Predicate = f => true, Serializer = f => FontWeightToString(f), Deserializer = s => FontWeightFromString(s) });
            Dict.Add(nameof(FontSize), new Item { Value = 14, DefaultValue = 14, Predicate = f => f > 0, Serializer = f => f.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(FirstCommentFontFamily), new Item { Value = new FontFamily("メイリオ"), DefaultValue = new FontFamily("メイリオ"), Predicate = f => true, Serializer = f => FontFamilyToString(f), Deserializer = s => FontFamilyFromString(s) });
            Dict.Add(nameof(FirstCommentFontStyle), new Item { Value = FontStyles.Normal, DefaultValue = FontStyles.Normal, Predicate = f => true, Serializer = f => FontStyleToString(f), Deserializer = s => FontStyleFromString(s) });
            Dict.Add(nameof(FirstCommentFontWeight), new Item { Value = FontWeights.Bold, DefaultValue = FontWeights.Bold, Predicate = f => true, Serializer = f => FontWeightToString(f), Deserializer = s => FontWeightFromString(s) });
            Dict.Add(nameof(FirstCommentFontSize), new Item { Value = 14, DefaultValue = 14, Predicate = f => f > 0, Serializer = f => f.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(SettingsDirPath), new Item { Value = "settings", DefaultValue = "settings", Predicate = s => !string.IsNullOrEmpty(s), Serializer = s => s, Deserializer = s => s });
            Dict.Add(nameof(BackColor), new Item { Value = ColorFromArgb("#FF000000"), DefaultValue = ColorFromArgb("#FF000000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(ForeColor), new Item { Value = ColorFromArgb("#FF000000"), DefaultValue = ColorFromArgb("#FF000000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(MainViewHeight), new Item { Value = 0, DefaultValue = 500, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(MainViewWidth), new Item { Value = 0, DefaultValue = 500, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(MainViewLeft), new Item { Value = 0, DefaultValue = 500, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(MainViewTop), new Item { Value = 0, DefaultValue = 500, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(HorizontalGridLineColor), new Item { Value = ColorFromArgb("#FF000000"), DefaultValue = ColorFromArgb("#FF000000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(VerticalGridLineColor), new Item { Value = ColorFromArgb("#FF000000"), DefaultValue = ColorFromArgb("#FF000000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(InfoForeColor), new Item { Value = ColorFromArgb("#FF000000"), DefaultValue = ColorFromArgb("#FF000000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(InfoBackColor), new Item { Value = ColorFromArgb("#FF000000"), DefaultValue = ColorFromArgb("#FF000000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(SelectedRowBackColor), new Item { Value = ColorFromArgb("#FF000000"), DefaultValue = ColorFromArgb("#FF000000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(SelectedRowForeColor), new Item { Value = ColorFromArgb("#FF000000"), DefaultValue = ColorFromArgb("#FF000000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(ConnectionNameWidth), new Item { Value = 0, DefaultValue = 500, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(IsShowConnectionName), new Item { Value = true, DefaultValue = true, Predicate = b => b, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(ConnectionNameDisplayIndex), new Item { Value = 0, DefaultValue = 0, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => int.Parse(s) });

        }
        public IOptions Clone()
        {
            throw new NotImplementedException();
        }

        public void Set(IOptions options)
        {
            var props = typeof(IOptions).GetProperties();
            foreach(var prop in props)
            {
                if(prop.CanRead && prop.CanWrite)
                {
                    var item = Dict[prop.Name];
                    var newVal = prop.GetValue(options);
                    if(item.Predicate(newVal))
                        item.Value = newVal;
                }
            }
            throw new NotImplementedException();
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
