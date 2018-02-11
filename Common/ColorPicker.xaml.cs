using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Common
{
    /// <summary>
    /// Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : UserControl
    {
        //TODO:PART_ColorArgbへ入力できる文字列の制限を加えたい
        public ColorPicker()
        {
            InitializeComponent();
            PART_ColorArgb.TextChanged += PART_ColorArgb_TextChanged;
        }

        private void PART_ColorArgb_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TryParse(PART_ColorArgb.Text, out Color color))
            {
                SelectedColor = color;
            }
        }
        private bool TryParse(string argb, out Color color)
        {
            if (argb == null)
            {
                color = Colors.White;
                return false;
            }
            const string pattern = "#(?<a>[0-9a-fA-F]{2})(?<r>[0-9a-fA-F]{2})(?<g>[0-9a-fA-F]{2})(?<b>[0-9a-fA-F]{2})";
            var match = System.Text.RegularExpressions.Regex.Match(argb, pattern, System.Text.RegularExpressions.RegexOptions.Compiled);
            if (!match.Success)
            {
                color = Colors.White;
                return false;
            }
            var a = byte.Parse(match.Groups["a"].Value, System.Globalization.NumberStyles.HexNumber);
            var r = byte.Parse(match.Groups["r"].Value, System.Globalization.NumberStyles.HexNumber);
            var g = byte.Parse(match.Groups["g"].Value, System.Globalization.NumberStyles.HexNumber);
            var b = byte.Parse(match.Groups["b"].Value, System.Globalization.NumberStyles.HexNumber);
            color = Color.FromArgb(a, r, g, b);
            return true;
        }

        #region SelectedColor
        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register(nameof(SelectedColor),
                                typeof(Color),
                                typeof(ColorPicker),
                                new FrameworkPropertyMetadata(Colors.White, new PropertyChangedCallback(OnTitleChanged)));
        
        public Color SelectedColor
        {
            get { return (Color)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }
        private static void OnTitleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is ColorPicker picker)
            {
                picker.PART_ToggleButton.Background = new SolidColorBrush(picker.SelectedColor);
                picker.SelectedColorArgb = picker.SelectedColor.ToString();
                picker.PART_ColorArgb.Text = picker.SelectedColorArgb;
            }
        }
        #endregion //SelectedColor

        #region SelectedColorArgb
        public static readonly DependencyProperty SelectedColorArgbProperty =
    DependencyProperty.Register(nameof(SelectedColorArgb),
                        typeof(string),
                        typeof(ColorPicker),
                        new FrameworkPropertyMetadata("#FF000000", new PropertyChangedCallback(OnSelectedColorArgbChanged)));

        public string SelectedColorArgb
        {
            get { return (string)GetValue(SelectedColorArgbProperty); }
            set { SetValue(SelectedColorArgbProperty, value); }
        }
        private static void OnSelectedColorArgbChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is ColorPicker picker)
            {
                if(picker.TryParse(picker.SelectedColorArgb, out Color color))
                {
                    picker.SelectedColor = color;
                    picker.PART_ColorArgb.Text = picker.SelectedColorArgb;
                }
            }
        }
        #endregion //SelectedColorArgb

        private string ColorToArgb(Color color)
        {
            var argb = color.ToString();
            return argb;
        }
        bool isOpen;
        private void PART_ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            isOpen = !isOpen;
            PART_POPUP.IsOpen = isOpen;
        }
    }
}
