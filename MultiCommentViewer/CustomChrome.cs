namespace CustomWindow
{
    using System;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Media.Effects;
    using System.Windows.Shell;
    //public class MainViewModel
    //{
    //    public string Title
    //    {
    //        get => "Title";
    //    }
    //    public Brush ButtonForeground
    //    {
    //        get
    //        {
    //            return new SolidColorBrush(Colors.White);
    //        }
    //    }
    //    public Brush ButtonBackground
    //    {
    //        get
    //        {
    //            return new SolidColorBrush(_myColor);
    //        }
    //    }
    //    public Brush ButtonBorderBrush
    //    {
    //        get
    //        {
    //            return new SolidColorBrush(_myColor);
    //        }
    //    }
    //    public Brush Test
    //    {
    //        get
    //        {
    //            return new SolidColorBrush(Colors.Yellow);
    //        }
    //    }
    //    public Brush TitleForeground
    //    {
    //        get
    //        {
    //            return new SolidColorBrush(Colors.White);
    //        }
    //    }
    //    public Brush TitleBackground
    //    {
    //        get
    //        {
    //            return new SolidColorBrush(_myColor);
    //        }
    //    }
    //    public Brush TitleBarBackground
    //    {
    //        get
    //        {
    //            return new SolidColorBrush(_myColor);
    //        }
    //    }
    //    public Brush ButtonMouseOverForeground
    //    {
    //        get
    //        {
    //            return new SolidColorBrush(Colors.White);
    //        }
    //    }
    //    public Brush ButtonMouseOverBackground
    //    {
    //        get
    //        {
    //            return new SolidColorBrush(_myColor);
    //        }
    //    }
    //    public Brush ButtonMouseOverBorderBrush
    //    {
    //        get
    //        {
    //            return new SolidColorBrush(Colors.White);
    //        }
    //    }
    //    public Visibility SystemButtonToolTipVisibility => Visibility.Collapsed;
    //    private readonly Color _myColor = new Color { A = 0xFF, R = 45, G = 45, B = 48 };
    //}
    [TemplatePart(Name = "PART_ResizeBorder", Type = typeof(Border))]
    [TemplatePart(Name = "PART_WindowBorder", Type = typeof(Border))]
    [TemplatePart(Name = "PART_LayoutGrid", Type = typeof(Grid))]
    [TemplatePart(Name = "PART_HeaderRowDefinition", Type = typeof(RowDefinition))]
    [TemplatePart(Name = "PART_HeaderColumnDefinition", Type = typeof(ColumnDefinition))]
    [TemplatePart(Name = "PART_HeaderBorder", Type = typeof(Border))]
    [TemplatePart(Name = "PART_ContentBorder", Type = typeof(Border))]    
    [TemplatePart(Name = "PART_TitleBar", Type = typeof(Panel))]
    [TemplatePart(Name = "PART_Close", Type = typeof(Button))]
    [TemplatePart(Name = "PART_Minimize", Type = typeof(Button))]
    [TemplatePart(Name = "PART_Maximize", Type = typeof(Button))]
    [TemplatePart(Name = "PART_Icon", Type = typeof(Image))]
    [TemplatePart(Name = "PART_Title", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_Drag", Type = typeof(Control))]
    public partial class CustomChrome : Window
    {
        private Button minimizeButton;
        private Button maximizeButton;
        private Button closeButton;
        private Image iconImage;
        private Control dragControl;

        static CustomChrome()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomChrome),
                new FrameworkPropertyMetadata(typeof(CustomChrome)));
        }

        #region Dependency Properties

        private abstract class DependencyPropertyDefault
        {
            public const int ResizeBorderWidth = 6;
            public const bool EnableDropShadow = false;
            public const int DropShadowBlurRadius = 10;
            public const double DropShadowOpacity = 1.0;
            public const bool IsDragging = false;
        }

        public int ResizeBorderWidth
        {
            get { return (int)GetValue(ResizeBorderWidthProperty); }
            set { SetValue(ResizeBorderWidthProperty, value); }
        }

        public static readonly DependencyProperty ResizeBorderWidthProperty =
            DependencyProperty.Register("ResizeBorderWidth", typeof(int), typeof(CustomChrome), 
                new PropertyMetadata(DependencyPropertyDefault.ResizeBorderWidth));

        public bool EnableDropShadow
        {
            get { return (bool)GetValue(EnableDropShadowProperty); }
            set { SetValue(EnableDropShadowProperty, value); }
        }

        public static readonly DependencyProperty EnableDropShadowProperty =
            DependencyProperty.Register("EnableDropShadow", typeof(bool), typeof(CustomChrome),
                new PropertyMetadata(DependencyPropertyDefault.EnableDropShadow));

        public int DropShadowBlurRadius
        {
            get { return (int)GetValue(DropShadowBlurRadiusProperty); }
            set { SetValue(DropShadowBlurRadiusProperty, value); }
        }

        public static readonly DependencyProperty DropShadowBlurRadiusProperty =
            DependencyProperty.Register("DropShadowBlurRadius", typeof(int), typeof(CustomChrome), 
                new PropertyMetadata(DependencyPropertyDefault.DropShadowBlurRadius));

        public double DropShadowOpacity
        {
            get { return (double)GetValue(DropShadowOpacityProperty); }
            set { SetValue(DropShadowOpacityProperty, value); }
        }

        public static readonly DependencyProperty DropShadowOpacityProperty =
            DependencyProperty.Register("DropShadowOpacity", typeof(double), typeof(CustomChrome), 
                new PropertyMetadata(DependencyPropertyDefault.DropShadowOpacity));

        public Color DropShadowColor
        {
            get { return (Color)GetValue(DropShadowColorProperty); }
            set { SetValue(DropShadowColorProperty, value); }
        }
        
        public static readonly DependencyProperty DropShadowColorProperty =
            DependencyProperty.Register("DropShadowColor", typeof(Color), typeof(CustomChrome),
                new PropertyMetadata(Colors.Black));

        public bool IsDragging
        {
            get { return (bool)GetValue(IsDraggingProperty); }
            set { SetValue(IsDraggingProperty, value); }
        }

        public static readonly DependencyProperty IsDraggingProperty =
            DependencyProperty.Register("IsDragging", typeof(bool), typeof(CustomChrome), 
                new PropertyMetadata(DependencyPropertyDefault.IsDragging));

        #endregion Dependency Properties

        public override void OnApplyTemplate()
        {
            // Implementers should always call the base 
            // implementation before their own implementation.
            base.OnApplyTemplate();

            DetachFromVisualTree();

            AttachToVisualTree();            
        }

        private void DetachFromVisualTree()
        {
            if (closeButton != null)
            {
                closeButton.Click -= OnCloseButtonClick;
            }

            if (minimizeButton != null)
            {
                minimizeButton.Click -= OnMinimizeButtonClick;
            }

            if (maximizeButton != null)
            {
                maximizeButton.Click -= OnMaximizeButtonClick;
            }

            if (iconImage != null)
            {
                iconImage.MouseDown -= OnTitleAreaMouseDown;
            }

            if (dragControl != null)
            {
                dragControl.MouseMove -= OnDragControlMouseMove;
                dragControl.MouseRightButtonDown -= OnTitleAreaMouseDown;
                dragControl.MouseDoubleClick -= OnDragControlMouseDoubleClick;
            }
        }

        private void AttachToVisualTree()
        {
            closeButton = GetChildControl<Button>("PART_Close");
            if (closeButton != null)
            {
                closeButton.Click += OnCloseButtonClick;
            }

            minimizeButton = GetChildControl<Button>("PART_Minimize");
            if (minimizeButton != null)
            {
                minimizeButton.Click += OnMinimizeButtonClick;
            }

            maximizeButton = GetChildControl<Button>("PART_Maximize");
            if (maximizeButton != null)
            {
                maximizeButton.Click += OnMaximizeButtonClick;
            }

            iconImage = GetChildControl<Image>("PART_Icon");
            if (iconImage != null)
            {
                iconImage.MouseDown += OnTitleAreaMouseDown;
            }

            dragControl = GetChildControl<Control>("PART_Drag");
            if (dragControl != null)
            {
                dragControl.MouseMove += OnDragControlMouseMove;
                dragControl.MouseRightButtonDown += OnTitleAreaMouseDown;
                dragControl.MouseDoubleClick += OnDragControlMouseDoubleClick;
            }
            
            var windowBorder = GetChildControl<Border>("PART_WindowBorder");        

            // Set a minimum window height if not specified
            var headerRowDefinition = GetChildControl<RowDefinition>("PART_HeaderRowDefinition");
            if (headerRowDefinition != null)
            {                
                double top = windowBorder != null ? windowBorder.BorderThickness.Top : 0;
                double bottom = windowBorder != null ? windowBorder.BorderThickness.Bottom : 0;

                // Take into account: the resize border (Top and Bottom), the first row height 
                // and the top and bottom border thickeness               
                double defaultMinHeight = GetResizeBorderWidth(this) * 2 + headerRowDefinition.MinHeight + top + bottom;
                this.MinHeight = Math.Max(this.MinHeight, defaultMinHeight);
            }

            // Set a minimum window width if not specified
            var headerColumnDefinition = GetChildControl<ColumnDefinition>("PART_HeaderColumnDefinition");
            if (headerColumnDefinition != null)
            {
                double left = windowBorder != null ? windowBorder.BorderThickness.Left : 0;
                double right = windowBorder != null ? windowBorder.BorderThickness.Right : 0;

                // Take into account: the resize border (Left and Right), the first row width 
                // and the left and right border thickeness
                double defaultMinWidth = GetResizeBorderWidth(this) * 2 + headerColumnDefinition.MinWidth + left + right;
                this.MinWidth = Math.Max(this.MinWidth, defaultMinWidth);
            }
        }

        /// <summary>
        /// Look up the template for the corresponding control instance
        /// </summary>
        /// <typeparam name="T">The expected control type</typeparam>
        /// <param name="name">The control name</param>
        /// <returns>The instance of the control if found</returns>
        protected T GetChildControl<T>(string name) where T : DependencyObject
        {
            return this.GetTemplateChild(name) as T;
        }

        private void OnDragControlMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.ResizeMode != ResizeMode.NoResize && 
                this.ResizeMode != ResizeMode.CanMinimize)
            {
                if (this.IsSnapped)
                {
                    // Restore on screen containing the left/top coordinate                    
                    this.Top = this.RestoreBounds.Top;
                    this.Left = this.RestoreBounds.Left;
                    this.Width = this.RestoreBounds.Width;
                    this.Height = this.RestoreBounds.Height;
                }
                else
                {
                    ToggleMaximize();

                    // Adding guard to fix issue with OnMouseMove handler 
                    // being called on a double click maximize action 
                    this.IsMaximizing = this.WindowState == WindowState.Maximized;                    
                }
            }
        }

        /// <summary>
        /// Indicates if the window was snapped on one side
        /// to allow it to be restored correctly
        /// </summary>
        private bool IsSnapped { get; set; }

        /// <summary>
        /// Indicates that the window was maximized
        /// from a mouse double click action to
        /// avoid an unexpected window restore
        /// </summary>
        private bool IsMaximizing { get; set; }

        /// <summary>
        // Indicates that the system menu was shown 
        // to avoid a window restore glitch from a 
        // maximized window state
        /// </summary>
        private bool SystemMenuShown { get; set; }

        #region Window menu command handlers

        public void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void OnMinimizeButtonClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        public void OnMaximizeButtonClick(object sender, RoutedEventArgs e)
        {
            ToggleMaximize();
        }

        private void ToggleMaximize()
        {
            // Reset snapped flag 
            IsSnapped = false;

            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                DisableResizeBorder(this);

                WindowState = WindowState.Maximized;
            }              
        }

        #endregion Window menu command handlers

        public void OnTitleAreaMouseDown(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(null);

            if (e.ChangedButton == MouseButton.Right)
            {
                ShowSystemMenu(this, position);

                this.SystemMenuShown = true;
            }
            else if (e.ChangedButton == MouseButton.Left)
            {
                // The default behavior on a mouse double click over the
                // window icon is to close the application
                if (e.ClickCount == 2)
                {
                    this.Close();
                }
                else
                {
                    ShowSystemMenu(this, position);

                    this.SystemMenuShown = true;
                }
            }
        }

        /// <summary>
        /// Support for custom window drag move and 
        /// restore from maximized window state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnDragControlMouseMove(object sender, MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.LeftButton == MouseButtonState.Pressed && !this.IsMaximizing && !this.SystemMenuShown)
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    // When passing null to GetPosition, we get the mouse position 
                    // relative to the containing window (on current screen)
                    // Remarks: Mouse.GetPosition(this), e.GetPosition(null);
                    // and e.MouseDevice.GetPosition(null) seems to get the 
                    // same coordinates

                    // Local (within window) and global (screen) coordinates
                    Point local = e.GetPosition(null);
                    Point global = this.PointToScreen(local);

                    IntPtr hwnd = new WindowInteropHelper(this).EnsureHandle();
                    MonitorArea monitorArea = GetMonitorArea(hwnd);

                    // Coordinates relative to the drag control
                    Point position = e.GetPosition(dragControl);
                    Point relative = this.PointToScreen(position);

                    double dragAreaWidth = dragControl.ActualWidth;                    

                    double leftMargin = global.X - relative.X;
                    double rightMargin = monitorArea.Work.Width - dragAreaWidth - leftMargin; 
                                                       
                    double restoreDragAreaWidth = this.RestoreBounds.Width - leftMargin - rightMargin;

                    // New X coordinate relative to restored window
                    double x = Math.Round(restoreDragAreaWidth * local.X / dragAreaWidth);

                    // Compensate for the resize border (uniform) width
                    double resizeBorderWidth = GetResizeBorderWidth(this);

                    double left = monitorArea.Work.Left - resizeBorderWidth;
                    double right = monitorArea.Work.Left + monitorArea.Work.Width + resizeBorderWidth;

                    double leftBound = left + this.RestoreBounds.Width - rightMargin;
                    double rightBound = right - this.RestoreBounds.Width + leftMargin;

                    Point offset = new Point(0, resizeBorderWidth);

                    // The restore bounds width is within the left bound region
                    if (global.X < leftBound)
                    {
                        // Align restored window to left of screen
                        this.Left = left;

                        offset.X = resizeBorderWidth;
                    }                    
                    // The restore bounds width is within the right bound region
                    else if (global.X > rightBound)
                    {
                        // Align restored window to right of screen
                        this.Left = right - this.RestoreBounds.Width;

                        offset.X = -resizeBorderWidth;
                    }
                    // The restore bounds width is shorter than the left / right bound region width
                    else if ((global.X > leftBound) && (global.X < rightBound))
                    {
                        this.Left = global.X - x - leftMargin - resizeBorderWidth;
                    }
                    
                    this.Top = global.Y - local.Y;

                    // Set and remove the horizontal offset to avoid the restored window's border
                    // to momentarily get outside the current monitor space creating a short flicker.

                    this.Left += offset.X;
                    
                    this.WindowState = WindowState.Normal;

                    this.Left -= offset.X;
                    this.Top -= offset.Y;
                }

                this.IsDragging = true;

                HideDropShadow(this);

                this.DragMove();                

                this.IsDragging = false;

                ShowDropShadow(this);
            }

            // Reset special condition guard flags
            this.IsMaximizing = false;
            this.SystemMenuShown = false;

            e.Handled = true;            
        }

        public CustomChrome()
        {
            //DataContext = new MainViewModel();
            // The InitializeComponent method is being created at compile time 
            // by the XAML Parser and needs to be called at runtime to load the
            // compiled XAML page of a component.            
            var initializer = this.GetType().GetMethod("InitializeComponent", 
                                             BindingFlags.Public | BindingFlags.Instance);

            // The method exists at runtime but not at design-time, so
            // make sure it is not called during design time (it won't compile)
            // Can also use -> if (!DesignerProperties.GetIsInDesignMode(this))
            if (initializer != null)
            {
                initializer.Invoke(this, null);
            }

            // The following methods must be called after the initializer
            // to get the values set from the XAML markup (ex. the ResizeMode)
            SetChromeWindow(this);
            SetCustomWindow(this);

            // Adding hook to WndProc
            IntPtr hwnd = new WindowInteropHelper(this).EnsureHandle();
            HwndSource source = HwndSource.FromHwnd(hwnd);
            source.AddHook(new HwndSourceHook(WindowHookProc));
        }

        private static void SetCustomWindow(CustomChrome window)
        {
            window.WindowStyle = WindowStyle.None;
            window.BorderBrush = Brushes.Transparent;
            window.AllowsTransparency = true;
            window.SnapsToDevicePixels = true;
            
            //TextOptions.SetTextFormattingMode(window, TextFormattingMode.Display);
            //RenderOptions.SetBitmapScalingMode(window, BitmapScalingMode.HighQuality);                 
        }

        protected override void OnActivated(EventArgs e)
        {
            ShowDropShadow(this);

            base.OnActivated(e);
        }

        protected override void OnDeactivated(EventArgs e)
        {
            HideDropShadow(this);

            base.OnDeactivated(e);
        }

        private static void SetChromeWindow(Window window)
        {
            var chrome = new WindowChrome();

            // Disable glass frame (slightly faster rendering)
            chrome.GlassFrameThickness = new Thickness(0);            
            chrome.CornerRadius = new CornerRadius(0);

            // Required to enable custom title bar commands
            chrome.CaptionHeight = 0;

            WindowChrome.SetWindowChrome(window, chrome);            
        }

        /// <summary>
        /// Removing the WndProc hook when window is closing
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            IntPtr hwnd = (new WindowInteropHelper(this)).Handle;
            HwndSource src = HwndSource.FromHwnd(hwnd);
            src.RemoveHook(new HwndSourceHook(WindowHookProc));

            DetachFromVisualTree();

            base.OnClosing(e);
        }

        private class MonitorArea
        {
            public struct Region
            {
                public int Left;
                public int Right;
                public int Top;
                public int Bottom;
                public int Width;
                public int Height;
            }

            public Region Work;
            public Region Display;

            public POINT Offset;

            public MonitorArea(RECT display, RECT work)
            {
                Display.Left = display.left;
                Display.Right = display.right;
                Display.Top = display.top;
                Display.Bottom = display.bottom;
                Display.Width = Math.Abs(display.right - display.left);
                Display.Height = Math.Abs(display.bottom - display.top);

                Work.Left = work.left;
                Work.Right = work.right;
                Work.Top = work.top;
                Work.Bottom = work.bottom;
                Work.Width = Math.Abs(work.right - work.left);
                Work.Height = Math.Abs(work.bottom - work.top);

                Offset = new POINT(Math.Abs(work.left - display.left),
                                   Math.Abs(work.top - display.top));
            }
        }

        [DllImport("user32")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        [DllImport("user32")]
        private static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
        private class MONITORINFO
        {
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
            public RECT rcMonitor = new RECT();
            public RECT rcWork = new RECT();
            public int dwFlags = 0;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
            public POINT(int x, int y) { this.x = x; this.y = y; }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WINDOWPOS
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public uint flags;
        }

        private const Int32 WM_WINDOWPOSCHANGING = 0x0046;
        private const Int32 WM_WINDOWPOSCHANGED = 0x0047;
        private const Int32 SWP_NOSIZE = 0x0001;
        private const Int32 SWP_NOMOVE = 0x0002;
        private const Int32 WM_GETMINMAXINFO = 0x0024;
        private const Int32 MONITOR_DEFAULTTONEAREST = 0x00000002;
        private const Int32 WM_SIZE = 0x0005;
        private const Int32 SIZE_RESTORED = 0;
        private const Int32 SIZE_MINIMIZED = 1;
        private const Int32 SIZE_MAXIMIZED = 2;
        private const Int32 WM_MOUSEMOVE = 0x200;

        private static IntPtr WindowHookProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            CustomChrome window = GetWindow(hWnd) as CustomChrome;

            switch (msg)
            {
                // Toggle the DropShadowEffect when window is snapped or maximized
                case WM_SIZE:
                {
                    int resizing = (int)wParam;

                    if (resizing == SIZE_RESTORED)
                    {
                        MonitorArea monitorArea = GetMonitorArea(hWnd);

                        if (monitorArea != null)
                        {
                            // LOWORD
                            int width = ((int)lParam & 0x0000ffff); 
                                
                            // HIWORD
                            int height = (int)((int)lParam & 0xffff0000) >> 16;

                            // Detect if window was snapped to screen side of current monitor
                            // or if spanning width on multiple monitors (to avoid unsnapping)
                            if (height == monitorArea.Work.Height || 
                                width >= SystemParameters.VirtualScreenWidth)
                            {
                                window.IsSnapped = true;

                                UpdateResizeBorder(window, monitorArea, window.Left, window.Left + width);
                            }
                            else
                            {
                                window.IsSnapped = false;

                                ShowDropShadow(window);

                                EnableResizeBorder(window);
                            }                           
                        }
                    }
                    else if (resizing == SIZE_MAXIMIZED)
                    {
                        // Required when maximized from dragging window
                        DisableResizeBorder(window);
                    }
                }
                break;


                // To handle proper resizing of the custom window
                case WM_GETMINMAXINFO:
                {
                    MonitorArea monitorArea = GetMonitorArea(hWnd);

                    if (monitorArea != null)
                    {
                        MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));
                            
                        mmi.ptMaxPosition.x  = monitorArea.Offset.x;
                        mmi.ptMaxPosition.y  = monitorArea.Offset.y;
                        mmi.ptMaxSize.x      = monitorArea.Work.Width;
                        mmi.ptMaxSize.y      = monitorArea.Work.Height;

                        // To support minimum window size
                        mmi.ptMinTrackSize.x = (int) window.MinWidth;
                        mmi.ptMinTrackSize.y = (int) window.MinHeight;

                        Marshal.StructureToPtr(mmi, lParam, true);
                        handled = true;
                    }
                }
                break;

                // To activate/deactivate border resize handles from window position
                case WM_WINDOWPOSCHANGED:
                {
                    WINDOWPOS windowPos = (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));

                    // When window is snapped and position changes
                    if ((windowPos.flags & SWP_NOMOVE) != SWP_NOMOVE)
                    {
                        if (window.IsSnapped)
                        {
                            MonitorArea monitorArea = GetMonitorArea(hWnd);

                            if (monitorArea != null)
                            {
                                UpdateResizeBorder(window, monitorArea, windowPos.x, windowPos.x + windowPos.cx);
                            }
                        }
                    }
                }
                break;
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// Get the window reference given a window handle
        /// </summary>
        /// <param name="hWnd">The Window handle</param>
        /// <returns>A Window instance or null if no match</returns>
        private static Window GetWindow(IntPtr hWnd)
        {
            HwndSource hwndSource = HwndSource.FromHwnd(hWnd);
            return hwndSource.RootVisual as Window;
        }

        /// <summary>
        /// Get the current monitor area of the Window         
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        private static MonitorArea GetMonitorArea(IntPtr hWnd)
        {
            var monitor = MonitorFromWindow(hWnd, MONITOR_DEFAULTTONEAREST);

            if (monitor != IntPtr.Zero)
            {
                var monitorInfo = new MONITORINFO();
                GetMonitorInfo(monitor, monitorInfo);

                return new MonitorArea(monitorInfo.rcMonitor, monitorInfo.rcWork);
            }

            return null;
        }

        /// <summary>
        /// Activate or deactivate left/right resize handles given 
        /// the current window position and size
        /// </summary>
        /// <remarks>
        /// Applies to snapped window only
        /// The maximum window size is limited by the (multi) screen size
        /// </remarks>
        /// <param name="window"></param>
        /// <param name="monitorArea"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        private static void UpdateResizeBorder(Window window, MonitorArea monitorArea, double left, double right)
        {
            double borderWidth = GetResizeBorderWidth(window as CustomChrome);

            double virtualLeft = SystemParameters.VirtualScreenLeft;
            double virtualRight = SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth;

            double leftBorder  = left <= virtualLeft + monitorArea.Offset.x ? 0 : borderWidth;            
            double rightBorder = right >= virtualRight ? 0 : borderWidth;

            EnableResizeBorder(window, leftBorder, 0, rightBorder, 0);            
        }

        private static void EnableResizeBorder(Window window)
        {
            double borderWidth = GetResizeBorderWidth(window as CustomChrome);
            
            EnableResizeBorder(window, borderWidth, borderWidth, borderWidth, borderWidth);
        }

        /// <summary>
        /// Get the highest value between the current resize border width and
        /// the drop shadow blur radius 
        /// </summary>
        /// <param name="window"></param>
        /// <returns>
        /// The highest value between the resize border width and the drop shadown blur radius
        /// </returns>
        private static double GetResizeBorderWidth(CustomChrome window)
        {
            // Override the resize border width if the blur radius is higher
            double borderWidth = Math.Max(window.ResizeBorderWidth,
                                          window.DropShadowBlurRadius);

            // Guard against lower than default resize width
            return borderWidth > DependencyPropertyDefault.ResizeBorderWidth ? 
                borderWidth : DependencyPropertyDefault.ResizeBorderWidth;
        }

        private static void EnableResizeBorder(Window window, double left, double top, double right, double bottom)
        {
            var chrome = WindowChrome.GetWindowChrome(window);

            var thickness = new Thickness(left, top, right, bottom);

            // The chrome resize border needs the actual window border to
            // be non zero otherwise it won't allow resizing
            chrome.ResizeBorderThickness = thickness;
            window.BorderThickness = thickness;

            WindowChrome.SetWindowChrome(window, chrome);            
        }

        private static void DisableResizeBorder(Window window)
        {
            var chrome = WindowChrome.GetWindowChrome(window);
            var thickness = new Thickness(0);

            // Synch the chrome resize border width 
            // and the window border width
            window.BorderThickness = thickness;
            chrome.ResizeBorderThickness = thickness;
            
            WindowChrome.SetWindowChrome(window, chrome);                        
        }

        /// <summary>
        /// Create and show (if enabled) the drop shadow effect 
        /// around the window border
        /// </summary>
        /// <param name="window"></param>
        /// <remarks>
        /// We need to always create the drop shadow effect
        /// even if not enabled to be able to support resizing        
        /// </remarks>
        private static void ShowDropShadow(CustomChrome window)
        {            
            var dropShadowEffect = window.Effect as DropShadowEffect;
                
            if (dropShadowEffect == null)                     
            {
                window.Effect = CreateDropShadowEffect(
                    window.DropShadowColor, 
                    window.DropShadowOpacity, 
                    window.DropShadowBlurRadius);
            }
            else
            {                    
                dropShadowEffect.Color = window.DropShadowColor;
                dropShadowEffect.Opacity = window.DropShadowOpacity;
                dropShadowEffect.BlurRadius = Math.Max(window.DropShadowBlurRadius, 2);                                        
            }

            // Set the border to make the effect visible when window 
            // cannot resize or can only minimize
            if (window.ResizeMode == ResizeMode.NoResize ||
                window.ResizeMode == ResizeMode.CanMinimize)
            {
                double borderWidth = GetResizeBorderWidth(window);
                window.BorderThickness = new Thickness(borderWidth);
            }
                        
            if (!window.EnableDropShadow)
            { 
                HideDropShadow(window);
            }   
        }

        private static void HideDropShadow(CustomChrome window)
        {            
            var dropShadowEffect = window.Effect as DropShadowEffect;

            if (dropShadowEffect != null)
            {
                // By making the effect nearly invisible but yet still enough
                // visible this hack allow to "hide" the drop shadow but still 
                // be able to show the resizing handles when AllowTransparency 
                // is set to "true"
                if (window.ResizeMode != ResizeMode.NoResize &&
                    window.ResizeMode != ResizeMode.CanMinimize)
                {
                    // To "disable" drop shadow effet, we must use an opacity > 0
                    // otherwise the resizing border will not work (default is 1) 
                    // 0.05 was tested to be visible "enough" to make it work
                    dropShadowEffect.Opacity = 1;// 0.05;

                    // Force a blur radius to allow resizing handle to work
                    // When the drop shadow opacity is nearly 0, the blur radius 
                    // also shrink, a value of 10 seems to be large enough
                    dropShadowEffect.BlurRadius = 10;
                }
                else
                {
                    dropShadowEffect.Opacity = 1;
                    dropShadowEffect.BlurRadius = 10;
                }
            }
        }

        private static DropShadowEffect CreateDropShadowEffect(Color color, double opacity, double blurRadius)
        {
            var dropShadowEffect = new DropShadowEffect();

            dropShadowEffect.Direction = 315;  // Default is 315              
            dropShadowEffect.ShadowDepth = 5; // Default is 5                    
            dropShadowEffect.RenderingBias = RenderingBias.Quality; // Default is Performance
            dropShadowEffect.Color = color;
            dropShadowEffect.Opacity = opacity;

            // Minimum allowed blur radius is 2, if less than 2, 
            // the resize border wont work (default is 5)
            dropShadowEffect.BlurRadius = Math.Max(blurRadius, 2);

            return dropShadowEffect;
        }

        #region System Menu

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        private void ShowSystemMenu(Window window, Point? point = null)
        {
            const int WM_POPUPSYSTEMMENU = 0x0313;

            IntPtr hWnd = new WindowInteropHelper(window).Handle;

            IntPtr lParam;

            if (point == null)
            {
                POINT ptScreen;
                GetCursorPos(out ptScreen);
                lParam = new IntPtr((ptScreen.y << 16) + ptScreen.x + 1);
            }
            else
            {
                Point ptScreen = PointToScreen((Point)point);
                lParam = new IntPtr(((int)ptScreen.Y << 16) + (int)ptScreen.X + 1);
            }

            SendMessage(hWnd, WM_POPUPSYSTEMMENU, IntPtr.Zero, lParam);            
        }

        #endregion System Menu
    }
}
