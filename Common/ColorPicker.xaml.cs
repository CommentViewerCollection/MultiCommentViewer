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
        public ColorPicker()
        {
            InitializeComponent();
            txtColorArgb.TextChanged += TxtColorArgb_TextChanged;
        }

        private void TxtColorArgb_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TryParse(txtColorArgb.Text, out Color color))
            {
                SelectedColor = color;
                PART_ToggleButton.Background = new SolidColorBrush(SelectedColor);
                Debug.WriteLine($"text={SelectedColor}");
            }
        }
        private bool TryParse(string argb, out Color color)
        {
            if (argb == null)
            {
                color = Colors.White;
                return false;
            }
            var pattern = "#(?<a>[0-9a-fA-F]{2})(?<r>[0-9a-fA-F]{2})(?<g>[0-9a-fA-F]{2})(?<b>[0-9a-fA-F]{2})";
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
        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register(nameof(SelectedColor),
                                typeof(Color),
                                typeof(ColorPicker),
                                new FrameworkPropertyMetadata(Colors.White, new PropertyChangedCallback(OnTitleChanged)));

        // 2. CLI用プロパティを提供するラッパー
        public Color SelectedColor
        {
            get { return (Color)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }
        private static void OnTitleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is ColorPicker picker)
            {

            }
        }
    }
}
