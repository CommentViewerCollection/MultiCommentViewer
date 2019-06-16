using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MultiCommentViewer
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:WPFTest"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:WPFTest;assembly=WPFTest"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:MyScrollBar/>
    ///
    /// </summary>
    public class MyScrollBar : ScrollBar
    {
        static MyScrollBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MyScrollBar), new FrameworkPropertyMetadata(typeof(MyScrollBar)));
        }

        public Brush ThumbBackground
        {
            get { return (Brush)GetValue(ThumbBackgroundProperty); }
            set { SetValue(ThumbBackgroundProperty, value); }
        }
        public static readonly DependencyProperty ThumbBackgroundProperty =
            DependencyProperty.Register(nameof(ThumbBackground), typeof(Brush), typeof(MyScrollBar), new UIPropertyMetadata(new SolidColorBrush(Colors.Yellow)));

        public Brush ThumbPressedBackground
        {
            get { return (Brush)GetValue(ThumbPressedBackgroundProperty); }
            set { SetValue(ThumbPressedBackgroundProperty, value); }
        }
        public static readonly DependencyProperty ThumbPressedBackgroundProperty =
            DependencyProperty.Register(nameof(ThumbPressedBackground), typeof(Brush), typeof(MyScrollBar), new UIPropertyMetadata(new SolidColorBrush(Colors.Yellow)));

        public Brush ThumbMouseOverBackground
        {
            get { return (Brush)GetValue(ThumbMouseOverBackgroundProperty); }
            set { SetValue(ThumbMouseOverBackgroundProperty, value); }
        }
        public static readonly DependencyProperty ThumbMouseOverBackgroundProperty =
            DependencyProperty.Register(nameof(ThumbMouseOverBackground), typeof(Brush), typeof(MyScrollBar), new UIPropertyMetadata(new SolidColorBrush(Colors.Yellow)));

        /// <summary>
        /// 上下ボタンの背景色
        /// </summary>
        public Brush ButtonBackground
        {
            get { return (Brush)GetValue(ButtonBackgroundProperty); }
            set { SetValue(ButtonBackgroundProperty, value); }
        }
        public static readonly DependencyProperty ButtonBackgroundProperty =
            DependencyProperty.Register(nameof(ButtonBackground), typeof(Brush), typeof(MyScrollBar), new UIPropertyMetadata(new SolidColorBrush(Colors.Yellow)));

        /// <summary>
        /// 上下ボタンの矢印の色
        /// </summary>
        public Brush ButtonForeground
        {
            get { return (Brush)GetValue(ButtonForegroundProperty); }
            set { SetValue(ButtonForegroundProperty, value); }
        }
        public static readonly DependencyProperty ButtonForegroundProperty =
            DependencyProperty.Register(nameof(ButtonForeground), typeof(Brush), typeof(MyScrollBar), new UIPropertyMetadata(new SolidColorBrush(Colors.Yellow)));

        public Brush ButtonBorderBrush
        {
            get { return (Brush)GetValue(ButtonBorderBrushProperty); }
            set { SetValue(ButtonBorderBrushProperty, value); }
        }
        public static readonly DependencyProperty ButtonBorderBrushProperty =
            DependencyProperty.Register(nameof(ButtonBorderBrush), typeof(Brush), typeof(MyScrollBar), new UIPropertyMetadata(new SolidColorBrush(Colors.Yellow)));

        public Brush ButtonDisabledBackground
        {
            get { return (Brush)GetValue(ButtonDisabledBackgroundProperty); }
            set { SetValue(ButtonDisabledBackgroundProperty, value); }
        }
        public static readonly DependencyProperty ButtonDisabledBackgroundProperty =
            DependencyProperty.Register(nameof(ButtonDisabledBackground), typeof(Brush), typeof(MyScrollBar), new UIPropertyMetadata(new SolidColorBrush(Colors.Yellow)));

        public Brush ButtonDisabledForeground
        {
            get { return (Brush)GetValue(ButtonDisabledForegroundProperty); }
            set { SetValue(ButtonDisabledForegroundProperty, value); }
        }
        public static readonly DependencyProperty ButtonDisabledForegroundProperty =
            DependencyProperty.Register(nameof(ButtonDisabledForeground), typeof(Brush), typeof(MyScrollBar), new UIPropertyMetadata(new SolidColorBrush(Colors.Yellow)));

        public Brush ButtonDisabledBorderBrush
        {
            get { return (Brush)GetValue(ButtonDisabledBorderBrushProperty); }
            set { SetValue(ButtonDisabledBorderBrushProperty, value); }
        }
        public static readonly DependencyProperty ButtonDisabledBorderBrushProperty =
            DependencyProperty.Register(nameof(ButtonDisabledBorderBrush), typeof(Brush), typeof(MyScrollBar), new UIPropertyMetadata(new SolidColorBrush(Colors.Yellow)));

        public Brush ButtonMouseOverBackground
        {
            get { return (Brush)GetValue(ButtonMouseOverBackgroundProperty); }
            set { SetValue(ButtonMouseOverBackgroundProperty, value); }
        }
        public static readonly DependencyProperty ButtonMouseOverBackgroundProperty =
            DependencyProperty.Register(nameof(ButtonMouseOverBackground), typeof(Brush), typeof(MyScrollBar), new UIPropertyMetadata(new SolidColorBrush(Colors.Yellow)));
        public Brush ButtonMouseOverForeground
        {
            get { return (Brush)GetValue(ButtonMouseOverForegroundProperty); }
            set { SetValue(ButtonMouseOverForegroundProperty, value); }
        }
        public static readonly DependencyProperty ButtonMouseOverForegroundProperty =
            DependencyProperty.Register(nameof(ButtonMouseOverForeground), typeof(Brush), typeof(MyScrollBar), new UIPropertyMetadata(new SolidColorBrush(Colors.Yellow)));
        public Brush ButtonMouseOverBorderBrush
        {
            get { return (Brush)GetValue(ButtonMouseOverBorderBrushProperty); }
            set { SetValue(ButtonMouseOverBorderBrushProperty, value); }
        }
        public static readonly DependencyProperty ButtonMouseOverBorderBrushProperty =
            DependencyProperty.Register(nameof(ButtonMouseOverBorderBrush), typeof(Brush), typeof(MyScrollBar), new UIPropertyMetadata(new SolidColorBrush(Colors.Yellow)));

        public Brush ButtonPressedBackground
        {
            get { return (Brush)GetValue(ButtonPressedBackgroundProperty); }
            set { SetValue(ButtonPressedBackgroundProperty, value); }
        }
        public static readonly DependencyProperty ButtonPressedBackgroundProperty =
            DependencyProperty.Register(nameof(ButtonPressedBackground), typeof(Brush), typeof(MyScrollBar), new UIPropertyMetadata(new SolidColorBrush(Colors.Yellow)));
        public Brush ButtonPressedForeground
        {
            get { return (Brush)GetValue(ButtonPressedForegroundProperty); }
            set { SetValue(ButtonPressedForegroundProperty, value); }
        }
        public static readonly DependencyProperty ButtonPressedForegroundProperty =
            DependencyProperty.Register(nameof(ButtonPressedForeground), typeof(Brush), typeof(MyScrollBar), new UIPropertyMetadata(new SolidColorBrush(Colors.Yellow)));
        public Brush ButtonPressedBorderBrush
        {
            get { return (Brush)GetValue(ButtonPressedBorderBrushProperty); }
            set { SetValue(ButtonPressedBorderBrushProperty, value); }
        }
        public static readonly DependencyProperty ButtonPressedBorderBrushProperty =
            DependencyProperty.Register(nameof(ButtonPressedBorderBrush), typeof(Brush), typeof(MyScrollBar), new UIPropertyMetadata(new SolidColorBrush(Colors.Yellow)));

    }
}
