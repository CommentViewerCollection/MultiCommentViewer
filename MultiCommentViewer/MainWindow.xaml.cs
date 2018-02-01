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
using GalaSoft.MvvmLight.Messaging;
using System.Diagnostics;
namespace MultiCommentViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel _vm;
        public MainWindow()
        {
            InitializeComponent();
            
            dataGrid.MouseRightButtonUp += DataGrid_MouseRightButtonUp;
            Messenger.Default.Register<ShowOptionsViewMessage>(this, message =>
            {
                try
                {
                    var optionsView = new OptionsView();
                    foreach (var tab in message.Tabs)
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
            Messenger.Default.Register<ShowUserViewMessage>(this, message =>
            {
                try
                {
                    var uvm = message.Uvm;
                    var userView = new UserView();
                    userView.DataContext = uvm;
                    userView.Show();
                }catch(Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            });

            _vm = new MainViewModel();
            DataContext = _vm;
        }

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
                dataGrid.SelectedItem = row.DataContext;
            }
        }

        private bool _addingCommentToTop;
        private bool bottom = true;
        //private bool neverTouch = true;
        private void dataGrid_ScrollChanged(object sender, RoutedEventArgs e)
        {
            //if (_addingCommentToTop)
            //    return;
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
                bottom = scrollViewer.IsBottom();
                //neverTouch = false;
            }

            //2017/09/11全体の高さが表示部に収まる間はスクロールがBottomにあるとみなすと、表示部に収まらなくなった瞬間にもBottomにあると判定されて、最初のスクロールが上手くいくかも。

            //if (bottom && a.ExtentHeightChange != 0)
            if (bottom && Test(a))
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
