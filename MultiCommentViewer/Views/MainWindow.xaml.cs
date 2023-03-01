using System;
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
using System.Diagnostics;
using Common.Wpf;
using CommunityToolkit.Mvvm.Messaging;

namespace MultiCommentViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            WeakReferenceMessenger.Default.Register<SetAddingCommentDirection>(this, (_, message) =>
            {
                var a = message.Value;
                _addingCommentToTop = a.IsTop;
            });
            WeakReferenceMessenger.Default.Register<SetPostCommentPanel>(this, (_, message) =>
            {
                PostCommentPanelPlaceHolder.Children.Clear();
                var a = message.Value;
                var newPanel = a.Panel;
                if (newPanel == null)
                {
                    PostCommentPanelPlaceHolder.IsEnabled = false;
                }
                else
                {
                    PostCommentPanelPlaceHolder.IsEnabled = true;
                    newPanel.Margin = new Thickness(0);
                    newPanel.VerticalAlignment = VerticalAlignment.Stretch;
                    newPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
                    newPanel.Width = double.NaN;
                    newPanel.Height = double.NaN;
                    PostCommentPanelPlaceHolder.Children.Add(newPanel);
                }
            });
            WeakReferenceMessenger.Default.Register<ShowOptionsViewMessage>(this, (_, message) =>
            {
                try
                {
                    var a = message.Value;
                    var optionsView = new OptionsView();
                    foreach (var tab in a.Tabs)
                    {
                        optionsView.AddTabPage(tab);
                    }
                    optionsView.Owner = this;
                    optionsView.ShowDialog();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debugger.Break();
                }
            });
            WeakReferenceMessenger.Default.Register<ShowUserViewMessage>(this, (_, message) =>
            {
                try
                {
                    var a = message.Value;
                    var uvm = a.Uvm;
                    var userView = new UserView
                    {
                        DataContext = uvm
                    };
                    userView.Show();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            });
            WeakReferenceMessenger.Default.Register<ShowUserListViewMessage>(this, (_, message) =>
            {
                try
                {
                    var a = message.Value;
                    var uvms = a.UserViewModels;
                    var mainVm = a.MainVm;
                    var options = a.Options;
                    var vm = new ViewModels.UserListViewModel(uvms, mainVm, options);
                    var userView = new View.UserListView
                    {
                        DataContext = vm,
                    };
                    userView.Show();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            });
            WeakReferenceMessenger.Default.Register<Common.AutoUpdate.ShowUpdateDialogMessage>(this, (_, message) =>
            {
                var a = message.Value;
                var logger = a.Logger;
                try
                {
                    var updateView = new Common.AutoUpdate.UpdateView();
                    var showPos = Tools.GetShowPos(Tools.GetMousePos(), updateView);
                    updateView.Left = showPos.X;
                    updateView.Top = showPos.Y;
                    //updateView.DataContext = new ViewModel.OptionsViewModel(message.Options);
                    updateView.IsUpdateExists = a.IsUpdateExists;
                    updateView.CurrentVersion = a.CurrentVersion;
                    updateView.LatestVersionInfo = a.LatestVersionInfo;
                    updateView.Logger = a.Logger;
                    updateView.UserAgent = a.UserAgent;
                    updateView.Owner = this;
                    updateView.ShowDialog();
                }
                catch (Exception ex)
                {
                    logger.LogException(ex);
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// https://social.msdn.microsoft.com/Forums/vstudio/en-US/63fa1e10-1050-4448-a2bc-62dfe0836f25/selecting-datagrid-row-when-right-mouse-button-is-pressed?forum=wpf
        private void DataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;

            //depがRunだと、VisualTreeHelper.GetParent()で下記の例外が投げられてしまう。
            //'System.Windows.Documents.Run' is not a Visual or Visual3D' InvalidOperationException
            if (e.OriginalSource is Run run)
            {
                dep = run.Parent;
            }
            while ((dep != null) && !(dep is DataGridCell))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }
            if (dep == null) return;

            if (dep is DataGridCell)
            {
                DataGridCell cell = dep as DataGridCell;
                cell.Focus();

                while ((dep != null) && !(dep is DataGridRow))
                {
                    dep = VisualTreeHelper.GetParent(dep);
                }
                DataGridRow row = dep as DataGridRow;
                //dataGrid.SelectedItem = row.DataContext;
            }
        }

        private bool _addingCommentToTop;
        private bool _bottom = true;
        //private bool neverTouch = true;
        private void DataGridScrollChanged(object sender, RoutedEventArgs e)
        {
            if (_addingCommentToTop)
                return;
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


            //2017/09/11
            //ExtentHeightは表示されていない部分も含めた全てのコンテントの高さ。
            //ScrollChangedが呼び出されたのにExtentHeightChangeが0ということはアイテムが追加されていないのにも関わらずスクロールがあった。
            //それはユーザが手動でスクロールした場合のみ起こること。
            if (a.ExtentHeightChange == 0)
            {
                //ユーザが手動でスクロールした
                _bottom = scrollViewer.IsBottom();
                //neverTouch = false;
            }

            //2017/09/11全体の高さが表示部に収まる間はスクロールがBottomにあるとみなすと、表示部に収まらなくなった瞬間にもBottomにあると判定されて、最初のスクロールが上手くいくかも。

            //if (bottom && a.ExtentHeightChange != 0)
            if (_bottom && Test(a))
            {
                scrollViewer.ScrollToBottom();
            }
        }
        private bool Test(ScrollChangedEventArgs e)
        {
            return e.ViewportHeightChange > 0 || e.ExtentHeightChange > 0 || e.ViewportHeightChange < 0 || e.ExtentHeightChange < 0;
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
