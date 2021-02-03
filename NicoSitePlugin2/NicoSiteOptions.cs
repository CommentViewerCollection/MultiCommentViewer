using System;
using System.Windows.Media;
using Common;
namespace NicoSitePlugin
{
    internal class NicoSiteOptions : DynamicOptionsBase, INicoSiteOptions
    {
        public int OfficialRoomsRetrieveCount { get => GetValue(); set => SetValue(value); }
        public bool CanUploadPlayerStatus { get => GetValue(); set => SetValue(value); }

        public int ResNum { get => GetValue(); set => SetValue(value); }

        public Color OperatorBackColor { get => GetValue(); set => SetValue(value); }
        public Color OperatorForeColor { get => GetValue(); set => SetValue(value); }

        public virtual bool IsShow184 { get => GetValue(); set => SetValue(value); }

        public bool IsAutoSetNickname { get => GetValue(); set => SetValue(value); }
        public bool IsShow184Id { get => GetValue(); set => SetValue(value); }
        public bool IsAutoGetUsername { get => GetValue(); set => SetValue(value); }
        public Color ItemBackColor { get => GetValue(); set => SetValue(value); }
        public Color ItemForeColor { get => GetValue(); set => SetValue(value); }
        public Color AdBackColor { get => GetValue(); set => SetValue(value); }
        public Color AdForeColor { get => GetValue(); set => SetValue(value); }
        public Color SpiBackColor { get => GetValue(); set => SetValue(value); }
        public Color SpiForeColor { get => GetValue(); set => SetValue(value); }
        public bool IsShowEmotion { get => GetValue(); set => SetValue(value); }
        public Color EmotionBackColor { get => GetValue(); set => SetValue(value); }
        public Color EmotionForeColor { get => GetValue(); set => SetValue(value); }
        protected override void Init()
        {
            Dict.Add(nameof(OfficialRoomsRetrieveCount), new Item { DefaultValue = 3, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(CanUploadPlayerStatus), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(ResNum), new Item { DefaultValue = 100, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => int.Parse(s) });

            Dict.Add(nameof(OperatorBackColor), new Item { DefaultValue = ColorFromArgb("#FFFF0000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(OperatorForeColor), new Item { DefaultValue = ColorFromArgb("#FF000000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });

            Dict.Add(nameof(IsShow184), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsAutoSetNickname), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsShow184Id), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(IsAutoGetUsername), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });

            Dict.Add(nameof(AdBackColor), new Item { DefaultValue = ColorFromArgb("#FFFFFFFF"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(AdForeColor), new Item { DefaultValue = ColorFromArgb("#FFFF0000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(ItemBackColor), new Item { DefaultValue = ColorFromArgb("#FFFFFFFF"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(ItemForeColor), new Item { DefaultValue = ColorFromArgb("#FFFF0000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(SpiBackColor), new Item { DefaultValue = ColorFromArgb("#FFFFFFFF"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(SpiForeColor), new Item { DefaultValue = ColorFromArgb("#FFFF0000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(IsShowEmotion), new Item { DefaultValue = true, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(EmotionBackColor), new Item { DefaultValue = ColorFromArgb("#FFFFFFFF"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(EmotionForeColor), new Item { DefaultValue = ColorFromArgb("#FFFF0000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
        }
        internal NicoSiteOptions Clone()
        {
            return (NicoSiteOptions)this.MemberwiseClone();
        }
        internal void Set(NicoSiteOptions changedOptions)
        {
            foreach (var src in changedOptions.Dict)
            {
                var v = src.Value;
                SetValue(v.Value, src.Key);
            }
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
    }
}
