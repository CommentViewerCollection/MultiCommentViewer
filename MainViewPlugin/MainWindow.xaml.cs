using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Mcv.MainViewPlugin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            WeakReferenceMessenger.Default.Register<ShowOptionsViewMessage>(this, (r, m) =>
            {
                try
                {
                    var optionsView = new OptionsView();
                    foreach (var tab in m.Value)
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
            //WeakReferenceMessenger.Default.Register<Settings.ShowSettingsViewMessage>(this, (r, m) =>
            //{
            //    //        var uvms = message.UserViewModels;
            //    //        var mainVm = message.MainVm;
            //    //        var options = message.Options;
            //    //        var vm = new ViewModels.UserListViewModel(uvms, mainVm, options);
            //    //        var userView = new View.UserListView
            //    //        {
            //    //            DataContext = vm,
            //    //        };
            //    //        userView.Show();

            //    var context = m.SettingsContext;
            //    //var strategy = m.Strategy;
            //    var v = new Settings.SettingsView();
            //    //foreach (var a in m.Strategy.CreatePanels(m.PluginSettingsList))
            //    //{
            //    //    v.AddTabPage(a);
            //    //}
            //    foreach (var tab in context.GetPanels())
            //    {
            //        v.AddTabPage(tab);
            //    }
            //    v.DataContext = m.SettingsContext.ViewModel;
            //    v.Owner = this;
            //    v.ShowDialog();
            //});
            //WeakReferenceMessenger.Default.Register<ShowUserInfoViewMessage>(this, (r, m) =>
            //{
            //    try
            //    {
            //        var v = new UserView();
            //        v.DataContext = m.UserInfoVm;
            //        //v.Owner = this;
            //        v.Show();
            //    }
            //    catch (Exception ex)
            //    {
            //        Debug.WriteLine(ex.Message);
            //    }
            //});
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
                scrollViewer = (ScrollViewer)sender;
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