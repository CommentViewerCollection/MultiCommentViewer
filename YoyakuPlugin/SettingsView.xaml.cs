using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OpenrecYoyakuPlugin
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : Window
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private bool IsTheMouseOnTargetRow(Visual target, GetDragDropPosition pos)
        {
            try
            {
                var posBounds = VisualTreeHelper.GetDescendantBounds(target);
                var theMousePos = pos((IInputElement)target);
                return posBounds.Contains(theMousePos);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private DataGridRow GetDataGridRowItem(int index)
        {
            if (RegisteredUserDataGrid.ItemContainerGenerator.Status != System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                return null;

            return RegisteredUserDataGrid.ItemContainerGenerator.ContainerFromIndex(index) as DataGridRow;
        }

        private int GetDataGridItemCurrentRowIndex(GetDragDropPosition pos)
        {
            int curIndex = -1;
            for (int i = 0; i < RegisteredUserDataGrid.Items.Count; i++)
            {
                var item = GetDataGridRowItem(i);
                if (IsTheMouseOnTargetRow(item, pos))
                {
                    curIndex = i;
                    break;
                }
            }
            return curIndex;
        }
        delegate Point GetDragDropPosition(IInputElement pos);
        private int _prevRowIndex = -1;
        bool _isClick = false;
        private void DataGrid_Drop(object sender, DragEventArgs e)
        {
            if (_prevRowIndex < 0)
                return;

            int index = this.GetDataGridItemCurrentRowIndex(e.GetPosition);

            if (index < 0)
                return;

            if (index == _prevRowIndex)
                return;

            var item = RegisteredUserDataGrid.ItemContainerGenerator.Items[_prevRowIndex];

            if(RegisteredUserDataGrid.ItemsSource is ObservableCollection<User> collection)
            {
                //ItemsSourceのコレクションの要素を移動させる。
                collection.Move(_prevRowIndex, index);
            }
        }

        private void DataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _prevRowIndex = GetDataGridItemCurrentRowIndex(e.GetPosition);

            if (_prevRowIndex < 0)
                return;

            RegisteredUserDataGrid.SelectedIndex = _prevRowIndex;

            //var selected_positionInfo = PositionsGrid.Items[_prevRowIndex];

            //if (selected_positionInfo == null)
            //    return;

            _isClick = true;
        }

        private void PositionsGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isClick)
            {
                var item = RegisteredUserDataGrid.Items[_prevRowIndex];

                if (item == null)
                    return;

                var dragdropeffects = DragDropEffects.Move;

                if (DragDrop.DoDragDrop(RegisteredUserDataGrid, item, dragdropeffects) != DragDropEffects.None)
                {
                    RegisteredUserDataGrid.SelectedItem = item;
                    _isClick = false;
                }
            }
        }

        private void PositionsGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isClick)
            {
                _isClick = false;
            }
        }
    }
}
