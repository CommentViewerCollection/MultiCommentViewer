using Common;
using System;
using System.Windows.Media;

namespace TwitchSitePlugin
{
    internal class TwitchSiteOptions : DynamicOptionsBase, ITwitchSiteOptions
    {
        /// <summary>
        /// @コテハンを自動登録するか
        /// </summary>
        public bool NeedAutoSubNickname { get => GetValue(); set => SetValue(value); }
        public string NeedAutoSubNicknameStr { get => GetValue(); set => SetValue(value); }
        public Color NoticeBackColor { get => GetValue(); set => SetValue(value); }
        public Color NoticeForeColor { get => GetValue(); set => SetValue(value); }
        protected override void Init()
        {
            Dict.Add(nameof(NeedAutoSubNickname), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(NeedAutoSubNicknameStr), new Item { DefaultValue = "@|＠", Predicate = s => !string.IsNullOrEmpty(s), Serializer = s => s, Deserializer = s => s });
            Dict.Add(nameof(NoticeBackColor), new Item { DefaultValue = ColorFromArgb("#FFFFFF00"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(NoticeForeColor), new Item { DefaultValue = ColorFromArgb("#FFFFFFFF"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
        }
        internal TwitchSiteOptions Clone()
        {
            return (TwitchSiteOptions)this.MemberwiseClone();
        }
        internal void Set(TwitchSiteOptions changedOptions)
        {
            foreach (var src in changedOptions.Dict)
            {
                var v = src.Value;
                SetValue(v.Value, src.Key);
            }
        }
        #region Converters
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
