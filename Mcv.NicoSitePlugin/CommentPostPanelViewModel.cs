using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mcv.PluginV2;

namespace NicoSitePlugin
{
    class CommentPostPanelViewModel : ObservableObject
    {
        public ObservableCollection<CommentSize> CommentSizeCollection { get; }
        public CommentSize SelectedCommentSize { get; set; }
        public ObservableCollection<CommentPosition> CommentPositionCollection { get; }
        public CommentPosition SelectedCommentPosition { get; set; }
        public ObservableCollection<CommentColor> CommentColorCollection { get; }
        public CommentColor SelectedCommentColor { get; set; }
        private readonly INicoCommentProvider _nicoCommentProvider;
        private readonly ILogger _logger;
        private string _comment;

        public bool Is184 { get; set; }
        public string Comment
        {
            get
            {
                return _comment;
            }
            set
            {
                _comment = value;
                OnPropertyChanged();
            }
        }
        public ICommand PostCommentCommand { get; }
        private async void PostComment()
        {
            var is184 = Is184;
            var comment = Comment;
            var s184 = is184 ? "184" : "";
            var size = SelectedCommentSize == CommentSize.Medium ? (string)null : SelectedCommentSize.ToString();
            var pos = SelectedCommentPosition == CommentPosition.Naka ? (string)null : SelectedCommentPosition.ToString();
            var color = SelectedCommentColor == CommentColor.White ? (string)null : SelectedCommentColor.ToString();
            var list = new List<string>
            {
                s184,
                size,
                pos,
                color,
            };
            var mail = string.Join(" ", list).Trim().Replace("  ", " ");
            Comment = "";
            try
            {
                await _nicoCommentProvider.PostCommentAsync(comment, is184, color, size, pos);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                Comment = comment;
            }
        }
        /// <summary>
        /// デザイナ用
        /// </summary>
        public CommentPostPanelViewModel()
        {
            if ((bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue))
            {

            }
            else
            {
                throw new NotSupportedException();
            }
        }
        public CommentPostPanelViewModel(INicoCommentProvider nicoCommentProvider, ILogger logger)
        {
            _nicoCommentProvider = nicoCommentProvider;
            _logger = logger;
            PostCommentCommand = new RelayCommand(PostComment);
            CommentSizeCollection = new ObservableCollection<CommentSize>
            {
                CommentSize.Big,
                CommentSize.Medium,
                CommentSize.Small,
            };
            SelectedCommentSize = CommentSize.Medium;
            CommentPositionCollection = new ObservableCollection<CommentPosition>
            {
                CommentPosition.Ue,
                CommentPosition.Naka,
                CommentPosition.Shita,
            };
            SelectedCommentPosition = CommentPosition.Naka;

            CommentColorCollection = new ObservableCollection<CommentColor>();
            foreach (var color in CommentColor.AllColors.Where(c => c != CommentColor.None))
            {
                CommentColorCollection.Add(color);
            }
            SelectedCommentColor = CommentColor.White;
        }
    }
    public sealed class CommentSize
    {
        public static readonly Dictionary<string, CommentSize> Dict = new Dictionary<string, CommentSize>();
        public static readonly CommentSize Big = new CommentSize("big");
        public static readonly CommentSize Medium = new CommentSize("medium");
        public static readonly CommentSize Small = new CommentSize("small");
        public static readonly CommentSize None = new CommentSize("undefined");

        private string _value;
        private CommentSize(string type)
        {
            _value = type;
            Dict.Add(type, this);
        }
        public static CommentSize FromString(string type)
        {
            var upper = type.ToUpper();
            if (Dict.ContainsKey(upper))
                return Dict[upper];
            else
                return None;
        }
        public override string ToString()
        {
            return _value;
        }
    }
    public sealed class CommentPosition
    {
        public static readonly Dictionary<string, CommentPosition> Dict = new Dictionary<string, CommentPosition>();
        public static readonly CommentPosition Ue = new CommentPosition("ue");
        public static readonly CommentPosition Naka = new CommentPosition("naka");
        public static readonly CommentPosition Shita = new CommentPosition("shita");
        public static readonly CommentPosition None = new CommentPosition("undefined");

        private string _value;
        private CommentPosition(string type)
        {
            _value = type;
            Dict.Add(type, this);
        }
        public static CommentPosition FromString(string type)
        {
            var upper = type.ToUpper();
            if (Dict.ContainsKey(upper))
                return Dict[upper];
            else
                return None;
        }
        public override string ToString()
        {
            return _value;
        }
    }
    public sealed class CommentColor
    {
        public static IEnumerable<CommentColor> AllColors => Dict.Select(c => c.Value);
        public static readonly Dictionary<string, CommentColor> Dict = new Dictionary<string, CommentColor>();
        public static readonly CommentColor White = new CommentColor("white", Colors.White);
        public static readonly CommentColor Red = new CommentColor("red", Colors.Red);
        public static readonly CommentColor Pink = new CommentColor("pink", ColorFromArgb("#FFFF7D7F"));
        public static readonly CommentColor Orange = new CommentColor("orange", ColorFromArgb("#FFFEBF2D"));
        public static readonly CommentColor Yellow = new CommentColor("yellow", ColorFromArgb("#FFFEFF3C"));
        public static readonly CommentColor Green = new CommentColor("green", ColorFromArgb("#FF00FF3C"));
        public static readonly CommentColor Cyan = new CommentColor("cyan", ColorFromArgb("#FF07FFFF"));
        //以下の項目は色の設定がまだ
        public static readonly CommentColor Blue = new CommentColor("blue", ColorFromArgb("#FF000000"));
        public static readonly CommentColor Purple = new CommentColor("purple", ColorFromArgb("#FF000000"));
        public static readonly CommentColor Black = new CommentColor("black", ColorFromArgb("#FF000000"));
        public static readonly CommentColor White2 = new CommentColor("white2", ColorFromArgb("#FF000000"));
        public static readonly CommentColor Red2 = new CommentColor("red2", ColorFromArgb("#FF000000"));
        public static readonly CommentColor Pink2 = new CommentColor("pink2", ColorFromArgb("#FF000000"));
        public static readonly CommentColor Orange2 = new CommentColor("orange2", ColorFromArgb("#FF000000"));
        public static readonly CommentColor Yellow2 = new CommentColor("yellow2", ColorFromArgb("#FF000000"));
        public static readonly CommentColor Green2 = new CommentColor("green2", ColorFromArgb("#FF000000"));
        public static readonly CommentColor Cyan2 = new CommentColor("cyan2", ColorFromArgb("#FF000000"));
        public static readonly CommentColor Blue2 = new CommentColor("blue2", ColorFromArgb("#FF000000"));
        public static readonly CommentColor Purple2 = new CommentColor("purple2", ColorFromArgb("#FF000000"));
        public static readonly CommentColor Black2 = new CommentColor("black2", ColorFromArgb("#FF000000"));
        public static readonly CommentColor None = new CommentColor("undefined", Colors.White);

        private string _value;
        public Brush Color { get; }
        private CommentColor(string type, Color color)
        {
            _value = type;
            Color = new SolidColorBrush(color);
            Dict.Add(type, this);
        }
        public static CommentColor FromString(string type)
        {
            var upper = type.ToUpper();
            if (Dict.ContainsKey(upper))
                return Dict[upper];
            else
                return None;
        }
        public override string ToString()
        {
            return _value;
        }
        private static Color ColorFromArgb(string argb)
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
                return System.Windows.Media.Color.FromArgb(a, r, g, b);
            }
        }
        private string ColorToArgb(Color color)
        {
            var argb = color.ToString();
            return argb;
        }
    }
}
