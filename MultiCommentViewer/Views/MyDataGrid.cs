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
    ///     <MyNamespace:MyDataGrid/>
    ///
    /// </summary>
    public class MyDataGrid : DataGrid
    {
        static MyDataGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MyDataGrid), new FrameworkPropertyMetadata(typeof(MyDataGrid)));
        }
        public Brush ScrollBarBackground
        {
            get { return (Brush)GetValue(ScrollBarBackgroundProperty); }
            set { SetValue(ScrollBarBackgroundProperty, value); }
        }
        public static readonly DependencyProperty ScrollBarBackgroundProperty =
            DependencyProperty.Register(nameof(ScrollBarBackground), typeof(Brush), typeof(MyDataGrid), new UIPropertyMetadata(new SolidColorBrush(Colors.Yellow)));

        public Brush ScrollBarBorderBrush
        {
            get { return (Brush)GetValue(ScrollBarBorderBrushProperty); }
            set { SetValue(ScrollBarBorderBrushProperty, value); }
        }
        public static readonly DependencyProperty ScrollBarBorderBrushProperty =
            DependencyProperty.Register(nameof(ScrollBarBorderBrush), typeof(Brush), typeof(MyDataGrid), new UIPropertyMetadata(new SolidColorBrush(Colors.Yellow)));

        public Brush ScrollBarThumbBackground
        {
            get { return (Brush)GetValue(ScrollBarThumbBackgroundProperty); }
            set { SetValue(ScrollBarThumbBackgroundProperty, value); }
        }
        public static readonly DependencyProperty ScrollBarThumbBackgroundProperty =
            DependencyProperty.Register(nameof(ScrollBarThumbBackground), typeof(Brush), typeof(MyDataGrid), new UIPropertyMetadata(new SolidColorBrush(Colors.Yellow)));

        public Brush ScrollBarThumbMouseOverBackground
        {
            get { return (Brush)GetValue(ScrollBarThumbMouseOverBackgroundProperty); }
            set { SetValue(ScrollBarThumbMouseOverBackgroundProperty, value); }
        }
        public static readonly DependencyProperty ScrollBarThumbMouseOverBackgroundProperty =
            DependencyProperty.Register(nameof(ScrollBarThumbMouseOverBackground), typeof(Brush), typeof(MyDataGrid), new UIPropertyMetadata(new SolidColorBrush(Colors.Yellow)));

        public Brush ScrollBarThumbPressedBackground
        {
            get { return (Brush)GetValue(ScrollBarThumbPressedBackgroundProperty); }
            set { SetValue(ScrollBarThumbPressedBackgroundProperty, value); }
        }
        public static readonly DependencyProperty ScrollBarThumbPressedBackgroundProperty =
            DependencyProperty.Register(nameof(ScrollBarThumbPressedBackground), typeof(Brush), typeof(MyDataGrid), new UIPropertyMetadata(new SolidColorBrush(Colors.Yellow)));

        public Brush ScrollBarButtonBackground
        {
            get { return (Brush)GetValue(ScrollBarButtonBackgroundProperty); }
            set { SetValue(ScrollBarButtonBackgroundProperty, value); }
        }
        public static readonly DependencyProperty ScrollBarButtonBackgroundProperty =
            DependencyProperty.Register(nameof(ScrollBarButtonBackground), typeof(Brush), typeof(MyDataGrid), new UIPropertyMetadata(new SolidColorBrush(Colors.Yellow)));

        public Brush ScrollBarButtonForeground
        {
            get { return (Brush)GetValue(ScrollBarButtonForegroundProperty); }
            set { SetValue(ScrollBarButtonForegroundProperty, value); }
        }
        public static readonly DependencyProperty ScrollBarButtonForegroundProperty =
            DependencyProperty.Register(nameof(ScrollBarButtonForeground), typeof(Brush), typeof(MyDataGrid), new UIPropertyMetadata(new SolidColorBrush(Colors.Yellow)));

        public Brush ScrollBarButtonBorderBrush
        {
            get { return (Brush)GetValue(ScrollBarButtonBorderBrushProperty); }
            set { SetValue(ScrollBarButtonBorderBrushProperty, value); }
        }
        public static readonly DependencyProperty ScrollBarButtonBorderBrushProperty =
            DependencyProperty.Register(nameof(ScrollBarButtonBorderBrush), typeof(Brush), typeof(MyDataGrid), new UIPropertyMetadata(new SolidColorBrush(Colors.Yellow)));

        public Brush ScrollBarButtonDisabledBackground
        {
            get { return (Brush)GetValue(ScrollBarButtonDisabledBackgroundProperty); }
            set { SetValue(ScrollBarButtonDisabledBackgroundProperty, value); }
        }
        public static readonly DependencyProperty ScrollBarButtonDisabledBackgroundProperty =
            DependencyProperty.Register(nameof(ScrollBarButtonDisabledBackground), typeof(Brush), typeof(MyDataGrid), new UIPropertyMetadata(new SolidColorBrush(Colors.Yellow)));

        public Brush ScrollBarButtonDisabledForeground
        {
            get { return (Brush)GetValue(ScrollBarButtonDisabledForegroundProperty); }
            set { SetValue(ScrollBarButtonDisabledForegroundProperty, value); }
        }
        public static readonly DependencyProperty ScrollBarButtonDisabledForegroundProperty =
            DependencyProperty.Register(nameof(ScrollBarButtonDisabledForeground), typeof(Brush), typeof(MyDataGrid), new UIPropertyMetadata(new SolidColorBrush(Colors.Yellow)));

        public Brush ScrollBarButtonDisabledBorderBrush
        {
            get { return (Brush)GetValue(ScrollBarButtonDisabledBorderBrushProperty); }
            set { SetValue(ScrollBarButtonDisabledBorderBrushProperty, value); }
        }
        public static readonly DependencyProperty ScrollBarButtonDisabledBorderBrushProperty =
            DependencyProperty.Register(nameof(ScrollBarButtonDisabledBorderBrush), typeof(Brush), typeof(MyDataGrid), new UIPropertyMetadata(new SolidColorBrush(Colors.Yellow)));

        public Brush ScrollBarButtonMouseOverBackground
        {
            get { return (Brush)GetValue(ScrollBarButtonMouseOverBackgroundProperty); }
            set { SetValue(ScrollBarButtonMouseOverBackgroundProperty, value); }
        }
        public static readonly DependencyProperty ScrollBarButtonMouseOverBackgroundProperty =
            DependencyProperty.Register(nameof(ScrollBarButtonMouseOverBackground), typeof(Brush), typeof(MyDataGrid), new UIPropertyMetadata(new SolidColorBrush(Colors.Yellow)));

        public Brush ScrollBarButtonMouseOverForeground
        {
            get { return (Brush)GetValue(ScrollBarButtonMouseOverForegroundProperty); }
            set { SetValue(ScrollBarButtonMouseOverForegroundProperty, value); }
        }
        public static readonly DependencyProperty ScrollBarButtonMouseOverForegroundProperty =
            DependencyProperty.Register(nameof(ScrollBarButtonMouseOverForeground), typeof(Brush), typeof(MyDataGrid), new UIPropertyMetadata(new SolidColorBrush(Colors.Yellow)));

        public Brush ScrollBarButtonMouseOverBorderBrush
        {
            get { return (Brush)GetValue(ScrollBarButtonMouseOverBorderBrushProperty); }
            set { SetValue(ScrollBarButtonMouseOverBorderBrushProperty, value); }
        }
        public static readonly DependencyProperty ScrollBarButtonMouseOverBorderBrushProperty =
            DependencyProperty.Register(nameof(ScrollBarButtonMouseOverBorderBrush), typeof(Brush), typeof(MyDataGrid), new UIPropertyMetadata(new SolidColorBrush(Colors.Yellow)));

        public Brush ScrollBarButtonPressedBackground
        {
            get { return (Brush)GetValue(ScrollBarButtonPressedBackgroundProperty); }
            set { SetValue(ScrollBarButtonPressedBackgroundProperty, value); }
        }
        public static readonly DependencyProperty ScrollBarButtonPressedBackgroundProperty =
            DependencyProperty.Register(nameof(ScrollBarButtonPressedBackground), typeof(Brush), typeof(MyDataGrid), new UIPropertyMetadata(new SolidColorBrush(Colors.Yellow)));

        public Brush ScrollBarButtonPressedForeground
        {
            get { return (Brush)GetValue(ScrollBarButtonPressedForegroundProperty); }
            set { SetValue(ScrollBarButtonPressedForegroundProperty, value); }
        }
        public static readonly DependencyProperty ScrollBarButtonPressedForegroundProperty =
            DependencyProperty.Register(nameof(ScrollBarButtonPressedForeground), typeof(Brush), typeof(MyDataGrid), new UIPropertyMetadata(new SolidColorBrush(Colors.Yellow)));
        
        public Brush ScrollBarButtonPressedBorderBrush
        {
            get { return (Brush)GetValue(ScrollBarButtonPressedBorderBrushProperty); }
            set { SetValue(ScrollBarButtonPressedBorderBrushProperty, value); }
        }
        public static readonly DependencyProperty ScrollBarButtonPressedBorderBrushProperty =
            DependencyProperty.Register(nameof(ScrollBarButtonPressedBorderBrush), typeof(Brush), typeof(MyDataGrid), new UIPropertyMetadata(new SolidColorBrush(Colors.Yellow)));


        public Brush DataGridColumnHeaderBackground
        {
            get { return (Brush)GetValue(DataGridColumnHeaderBackgroundProperty); }
            set { SetValue(DataGridColumnHeaderBackgroundProperty, value); }
        }
        public static readonly DependencyProperty DataGridColumnHeaderBackgroundProperty =
            DependencyProperty.Register(nameof(DataGridColumnHeaderBackground), typeof(Brush), typeof(MyDataGrid), new UIPropertyMetadata(new SolidColorBrush(Colors.Yellow)));

        public Brush DataGridColumnHeaderForeground
        {
            get { return (Brush)GetValue(DataGridColumnHeaderForegroundProperty); }
            set { SetValue(DataGridColumnHeaderForegroundProperty, value); }
        }
        public static readonly DependencyProperty DataGridColumnHeaderForegroundProperty =
            DependencyProperty.Register(nameof(DataGridColumnHeaderForeground), typeof(Brush), typeof(MyDataGrid), new UIPropertyMetadata(new SolidColorBrush(Colors.Yellow)));

        public Brush DataGridColumnHeaderBorderBrush
        {
            get { return (Brush)GetValue(DataGridColumnHeaderBorderBrushProperty); }
            set { SetValue(DataGridColumnHeaderBorderBrushProperty, value); }
        }
        public static readonly DependencyProperty DataGridColumnHeaderBorderBrushProperty =
            DependencyProperty.Register(nameof(DataGridColumnHeaderBorderBrush), typeof(Brush), typeof(MyDataGrid), new UIPropertyMetadata(new SolidColorBrush(Colors.Yellow)));


        public ContextMenu DataGridColumnHeaderContextMenu
        {
            get { return (ContextMenu)GetValue(DataGridColumnHeaderContextMenuProperty); }
            set { SetValue(DataGridColumnHeaderContextMenuProperty, value); }
        }
        public static readonly DependencyProperty DataGridColumnHeaderContextMenuProperty =
            DependencyProperty.Register(nameof(DataGridColumnHeaderContextMenu), typeof(ContextMenu), typeof(MyDataGrid), new UIPropertyMetadata(default));

    }
}
