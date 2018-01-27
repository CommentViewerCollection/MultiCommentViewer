using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using SitePlugin;
namespace MultiCommentViewer.Test
{
    public class OptionsTest : IOptions
    {
        public FontFamily FontFamily { get; set; }

        public FontStyle FontStyle { get; set; }

        public FontWeight FontWeight { get; set; }

        public int FontSize { get; set; }

        public FontFamily FirstCommentFontFamily { get; set; }

        public FontStyle FirstCommentFontStyle { get; set; }

        public FontWeight FirstCommentFontWeight { get; set; }

        public int FirstCommentFontSize { get; set; }
        /// <summary>
        /// 各種設定ファイルが置かれるフォルダ
        /// </summary>
        public string SettingsDirPath { get; set; }

        
        
        
        

        private const double Default_MainViewHeight = 532;
        public double MainViewHeight { get; set; }

        private const double Default_MainViewWidth = 490;
        public double MainViewWidth { get; set; }

        private const double DEFAULT_MainViewLeft = 0;
        public double MainViewLeft { get; set; }

        private const double DEFAULT_MainViewTop = 0;        
        public double MainViewTop { get; set; }


        private string _horizontalGridLineColor ="#FF000000";
        /// <summary>
        /// 行グリッド線の色
        /// </summary>
        public Color HorizontalGridLineColor
        {
            get { return ColorFromArgb(_horizontalGridLineColor); }
            set
            {
                _horizontalGridLineColor = ColorToArgb(value);
                RaisePropertyChanged();
            }
        }

        //以下のようなことをやりたいが共変性とか反変性的に無理そう。どうにかならないかなぁ。
        //var list =new List<Test<object>>
        //{
        //    new Test<Color>(nameof(ForeColor), Colors.Red, null),
        //    new Test<double>(nameof(MainViewWidth), Default_MainViewWidth, c=> c <=0),
        //};
        //public class Test<T>
        //{
        //    public void SetDefault()
        //    {
        //        var prop = this.GetType().GetProperty(_property);
        //        var current = (T)prop.GetValue(this);

        //        var b = _func == null ? true : _func(current);
        //        if (b)
        //        {
        //            prop.SetValue(this, _defaultVal);
        //        }

        //    }
        //    private readonly Func<T, bool> _func;
        //    private readonly string _property;
        //    private readonly T _defaultVal;
        //    public Test(string propertyName, T defaultVal, Func<T, bool> test)
        //    {
        //        _property = propertyName;
        //        _func = test;
        //        _defaultVal = defaultVal;
        //    }
        //}

        private string _verticalGridLineColor="#FF000000";
        /// <summary>
        /// 列グリッド線の色
        /// </summary>
        public Color VerticalGridLineColor
        {
            get { return ColorFromArgb(_verticalGridLineColor); }
            set
            {
                _verticalGridLineColor = ColorToArgb(value);
                RaisePropertyChanged();
            }
        }

        #region ForeColor
        private string _foreColorArgb = "#FF000000";
        public string ForeColorArgb
        {
            get { return _foreColorArgb; }
            set
            {
                _foreColorArgb = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(ForeColor));
            }
        }
        public Color ForeColor
        {
            get { return ColorFromArgb(ForeColorArgb); }
            set
            {
                ForeColorArgb = ColorToArgb(value);
                RaisePropertyChanged();
            }
        }
        #endregion

        #region BackColor
        private string _backColorArgb = "#FFFFFFFF";
        public string BackColorArgb
        {
            get { return _backColorArgb; }
            set
            {
                _backColorArgb = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(BackColor));
            }
        }
        public Color BackColor
        {
            get { return ColorFromArgb(BackColorArgb); }
            set
            {
                BackColorArgb = ColorToArgb(value);
                RaisePropertyChanged();
            }
        }
        #endregion
        
        public OptionsTest()
        {
            Init();
        }
        private void Init()
        {
            if (MainViewWidth <= 0)
                MainViewWidth = Default_MainViewWidth;
            if (MainViewHeight <= 0)
                MainViewHeight = Default_MainViewHeight;
        }
        public void Reset()
        {

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
        #region INotifyPropertyChanged
        [NonSerialized]
        private System.ComponentModel.PropertyChangedEventHandler _propertyChanged;
        /// <summary>
        /// 
        /// </summary>
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged
        {
            add { _propertyChanged += value; }
            remove { _propertyChanged -= value; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            _propertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
