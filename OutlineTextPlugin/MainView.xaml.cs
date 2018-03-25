using Common.Wpf;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace OutlineTextPlugin
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        OptionsView optionsView;
        public MainView()
        {
            InitializeComponent();
            _addingCommentToTop = true;
            _isForceClose = false;
            Messenger.Default.Register<ShowOptionsViewMessage>(this, message =>
            {
                try
                {
                    if (optionsView == null)
                    {
                        optionsView = new OptionsView
                        {
                            Owner = this
                        };
                    }
                    optionsView.DataContext = message.Vm;
                    var showPos = Tools.GetShowPos(Tools.GetMousePos(), optionsView);
                    optionsView.Left = showPos.X;
                    optionsView.Top = showPos.Y;
                    optionsView.Show();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debugger.Break();
                }
            });
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            if (!_isForceClose)
            {
                e.Cancel = true;
                Visibility = Visibility.Hidden;
            }
            base.OnClosing(e);
        }
        /// <summary>
        /// アプリの終了時にtrueにしてCloseを呼ぶとViewがCloseされる
        /// </summary>
        bool _isForceClose;
        /// <summary>
        /// Viewを閉じる。Close()は非表示になるようにしてある。
        /// </summary>
        public void ForceClose()
        {
            _isForceClose = true;
            this.Close();
        }
        private bool _addingCommentToTop;
        private bool bottom = true;
        //private bool neverTouch = true;
        private void dataGrid_ScrollChanged(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;
            ScrollViewer scrollViewer;
            if (sender is DataGrid dataGrid)
            {
                scrollViewer = dataGrid.GetScrollViewer();
            }
            else if (sender is ScrollViewer)
            {
                scrollViewer = sender as ScrollViewer;
            }
            else
            {
                return;
            }
            var a = e as ScrollChangedEventArgs;

            if (_addingCommentToTop)
            {
                if (a.VerticalOffset - a.VerticalChange <= 0 && a.ExtentHeightChange != 0)
                {
                    scrollViewer.ScrollToVerticalOffset(0);
                }
                else if (a.ExtentHeightChange != 0)
                {
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + a.ExtentHeightChange);
                }
                return;
            }
        }
    }
    public static class DataGridBehavior
    {
        public static ScrollViewer GetScrollViewer(this DataGrid dataGrid)
        {
            return dataGrid.Template.FindName("DG_ScrollViewer", dataGrid) as ScrollViewer;
        }
    }
    public static class ScrollViewerBehavior
    {
        public static bool IsBottom(this ScrollViewer sv)
        {
            //var b = (sv.VerticalOffset * 1.01) > sv.ScrollableHeight;
            var b = (sv.VerticalOffset >= sv.ScrollableHeight
                || sv.ExtentHeight < sv.ViewportHeight);
            return b;
        }
    }
}
