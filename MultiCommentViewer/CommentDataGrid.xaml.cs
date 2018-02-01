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
    /// Interaction logic for CommentDataGrid.xaml
    /// </summary>
    public partial class CommentDataGrid : UserControl
    {
        public CommentDataGrid()
        {
            InitializeComponent();
            dataGrid.MouseRightButtonUp += DataGrid_MouseRightButtonUp;
            
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

        //protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        //{
        //    dataGrid.get
        //    //dataGrid.ItemContainerGenerator.ContainerFromIndex(e.)
        //    base.OnMouseRightButtonUp(e);
        //}
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
}
