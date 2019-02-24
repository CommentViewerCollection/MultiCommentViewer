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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Common
{
    /// <summary>
    /// Interaction logic for NumericUpDown.xaml
    /// </summary>
    public partial class NumericUpDown : UserControl
    {
        public NumericUpDown()
        {
            InitializeComponent();
            Minimum = int.MinValue;
            Maximum = int.MaxValue;
            Value = Maximum - 1;
        }

        private void upButton_Click(object sender, RoutedEventArgs e)
        {
            if (Value < int.MaxValue)
            {
                Value++;
            }
        }

        private void downButton_Click(object sender, RoutedEventArgs e)
        {
            if (Value > int.MinValue)
            {
                Value--;
            }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
        "Value",
        typeof(int),
        typeof(NumericUpDown),
        new FrameworkPropertyMetadata(0, PropertyChanged) { BindsTwoWayByDefault = true, DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(
                "Minimum",
                typeof(int),
                typeof(NumericUpDown),
                new PropertyMetadata(0));
        public int Minimum
        {
            get { return (int)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(
                "Maximum",
                typeof(int),
                typeof(NumericUpDown),
                new PropertyMetadata(0));

        public int Maximum
        {
            get { return (int)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        private static void PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NumericUpDown)
            {
                var nud = d as NumericUpDown;
                if (nud.Value <= nud.Minimum)
                {
                    nud.downButton.IsEnabled = false;
                }
                else
                {
                    nud.downButton.IsEnabled = true;
                }
                if (nud.Value >= nud.Maximum)
                {
                    nud.upButton.IsEnabled = false;
                }
                else
                {
                    nud.upButton.IsEnabled = true;
                }
            }
        }
    }
}
