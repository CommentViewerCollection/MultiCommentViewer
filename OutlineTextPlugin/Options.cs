using Common;
using System.Windows;
using System.Windows.Media;
using System.Linq;
using System;

namespace OutlineTextPlugin
{
    internal class Options : DynamicOptionsBase
    {
        public FontFamily FontFamily { get => GetValue(); set => SetValue(value); }
        public FontStyle FontStyle { get => GetValue(); set => SetValue(value); }
        public FontWeight FontWeight { get => GetValue(); set => SetValue(value); }
        public int FontSize { get => GetValue(); set => SetValue(value); }
        public int CommentOutlineTextThickness { get => GetValue(); set => SetValue(value); }
        public Color CommentOutlineStrokeColor { get => GetValue(); set => SetValue(value); }
        public Color CommentOutlineFillColor { get => GetValue(); set => SetValue(value); }

        
        public bool IsShowHorizontalGridLine { get => GetValue(); set => SetValue(value); }
        public Color HorizontalGridLineColor { get => GetValue(); set => SetValue(value); }
        public bool IsShowVerticalGridLine { get => GetValue(); set => SetValue(value); }
        public Color VerticalGridLineColor { get => GetValue(); set => SetValue(value); }

        public double ThumbnailWidth { get => GetValue(); set => SetValue(value); }
        public double UserNameWidth { get => GetValue(); set => SetValue(value); }
        public double MessageWidth { get => GetValue(); set => SetValue(value); }
        public int ThumbnailDisplayIndex { get => GetValue(); set => SetValue(value); }
        public int UsernameDisplayIndex { get => GetValue(); set => SetValue(value); }
        public int MessageDisplayIndex { get => GetValue(); set => SetValue(value); }
        public Color BackColor { get => GetValue(); set => SetValue(value); }
        protected override void Init()
        {
            Dict.Add(nameof(FontFamily), new Item { DefaultValue = new FontFamily("メイリオ, Meiryo"), Predicate = f => true, Serializer = f => FontFamilyToString(f), Deserializer = s => FontFamilyFromString(s) });
            Dict.Add(nameof(FontStyle), new Item { DefaultValue = FontStyles.Normal, Predicate = f => true, Serializer = f => FontStyleToString(f), Deserializer = s => FontStyleFromString(s) });
            Dict.Add(nameof(FontWeight), new Item { DefaultValue = FontWeights.Normal, Predicate = f => true, Serializer = f => FontWeightToString(f), Deserializer = s => FontWeightFromString(s) });
            Dict.Add(nameof(FontSize), new Item { DefaultValue = 14, Predicate = f => f > 0, Serializer = f => f.ToString(), Deserializer = s => int.Parse(s) });

            Dict.Add(nameof(CommentOutlineTextThickness), new Item { DefaultValue = 1, Predicate = f => f > 0, Serializer = f => f.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(CommentOutlineStrokeColor), new Item { DefaultValue = ColorFromArgb("#FF000000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(CommentOutlineFillColor), new Item { DefaultValue = ColorFromArgb("#FFFFFFFF"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });

            Dict.Add(nameof(IsShowVerticalGridLine), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(VerticalGridLineColor), new Item { DefaultValue = ColorFromArgb("#FFDCDCDC"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(IsShowHorizontalGridLine), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(HorizontalGridLineColor), new Item { DefaultValue = ColorFromArgb("#FFDCDCDC"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });


            Dict.Add(nameof(ThumbnailWidth), new Item { DefaultValue = 100, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(UserNameWidth), new Item { DefaultValue = 100, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });
            Dict.Add(nameof(MessageWidth), new Item { DefaultValue = 300, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => double.Parse(s) });

            Dict.Add(nameof(ThumbnailDisplayIndex), new Item { DefaultValue = 0, Predicate = n => n >= 0, Serializer = n => n.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(UsernameDisplayIndex), new Item { DefaultValue = 1, Predicate = n => n >= 0, Serializer = n => n.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(MessageDisplayIndex), new Item { DefaultValue = 2, Predicate = n => n >= 0, Serializer = n => n.ToString(), Deserializer = s => int.Parse(s) });

            Dict.Add(nameof(BackColor), new Item { DefaultValue = ColorFromArgb("#FFFFFFFF"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
        }
        internal Options Clone()
        {
            return (Options)this.MemberwiseClone();
        }
        internal void Set(Options changedOptions)
        {
            foreach (var src in changedOptions.Dict)
            {
                var v = src.Value;
                SetValue(v.Value, src.Key);
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
        private bool IsValidArgb(string argb)
        {
            const string pattern = "#(?<a>[0-9a-fA-F]{2})(?<r>[0-9a-fA-F]{2})(?<g>[0-9a-fA-F]{2})(?<b>[0-9a-fA-F]{2})";
            return System.Text.RegularExpressions.Regex.IsMatch(argb, pattern);
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
