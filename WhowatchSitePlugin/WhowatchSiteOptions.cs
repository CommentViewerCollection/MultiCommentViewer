using Common;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Runtime.Serialization;
namespace WhowatchSitePlugin
{
    internal class WhowatchSiteOptions : DynamicOptionsBase, IWhowatchSiteOptions
    {
        /// <summary>
        /// @コテハンを自動登録するか
        /// </summary>
        public bool NeedAutoSubNickname { get => GetValue(); set => SetValue(value); }
        public int LiveCheckIntervalSec { get => GetValue(); set => SetValue(value); }
        public int LiveDataRetrieveIntervalSec { get => GetValue(); set => SetValue(value); }
        public Color ItemBackColor { get => GetValue(); set => SetValue(value); }
        public Color ItemForeColor { get => GetValue(); set => SetValue(value); }
        void IWhowatchSiteOptions.Set(IWhowatchSiteOptions siteOptions)
        {
            if(siteOptions is WhowatchSiteOptions op)
            {
                Set(op);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        protected override void Init()
        {
            Dict.Add(nameof(NeedAutoSubNickname), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(LiveCheckIntervalSec), new Item { DefaultValue = 30, Predicate = f => f >= 30, Serializer = f => f.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(LiveDataRetrieveIntervalSec), new Item { DefaultValue = 1, Predicate = f => f >= 1, Serializer = f => f.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(ItemBackColor), new Item { DefaultValue = ColorFromArgb("#FFFF0000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(ItemForeColor), new Item { DefaultValue = ColorFromArgb("#FF000000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });

        }
        public WhowatchSiteOptions Clone()
        {
            return DeepClone(this);
        }

        internal void Set(WhowatchSiteOptions changedOptions)
        {
            foreach (var src in changedOptions.Dict)
            {
                var v = src.Value;
                SetValue(v.Value, src.Key);
            }
        }

        IWhowatchSiteOptions IWhowatchSiteOptions.Clone()
        {
            return Clone();
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
