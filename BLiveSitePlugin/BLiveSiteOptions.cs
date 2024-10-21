using Common;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace BLiveSitePlugin
{
    public interface IBLiveSiteOptions: INotifyPropertyChanged
    {
        int StampSize { get; }
        bool IsPlayStampMusic { get; }
        string StampMusicFilePath { get; }
        bool IsPlayYellMusic { get; }
        string YellMusicFilePath { get; }
        bool IsAutoSetNickname { get; }
    }
    internal class BLiveSiteOptions : DynamicOptionsBase, IBLiveSiteOptions
    {
        public int StampSize { get => GetValue(); set => SetValue(value); }
        public bool IsPlayStampMusic { get => GetValue(); set => SetValue(value); }
        public string StampMusicFilePath { get => GetValue(); set => SetValue(value); }
        public bool IsPlayYellMusic { get => GetValue(); set => SetValue(value); }
        public string YellMusicFilePath { get => GetValue(); set => SetValue(value); }
        public bool IsAutoSetNickname { get => GetValue(); set => SetValue(value); }
        protected override void Init()
        {
            Dict.Add(nameof(StampSize), new Item { DefaultValue = 64, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(IsPlayStampMusic), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(StampMusicFilePath), new Item { DefaultValue = "", Predicate = s => !string.IsNullOrEmpty(s), Serializer = s => s, Deserializer = s => s });
            Dict.Add(nameof(IsPlayYellMusic), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(YellMusicFilePath), new Item { DefaultValue = "", Predicate = s => !string.IsNullOrEmpty(s), Serializer = s => s, Deserializer = s => s });
            Dict.Add(nameof(IsAutoSetNickname), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
        }
        internal BLiveSiteOptions Clone()
        {
            return (BLiveSiteOptions)this.MemberwiseClone();
        }
        internal void Set(BLiveSiteOptions changedOptions)
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
