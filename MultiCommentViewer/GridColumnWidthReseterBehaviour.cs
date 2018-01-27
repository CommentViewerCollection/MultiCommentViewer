using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace MultiCommentViewer
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// https://stackoverflow.com/questions/28095177/wpf-expander-when-gridsplitter-used-to-manually-resize-row-row-does-not-prope
    /// ExpanderとGridSplitterを配置した時にGridSplitterを手動で動かしてからExpanderを閉じる(IsExpandedをFalseにする)と意図したとおりに動作しない問題に対処できる
    /// </remarks>
    public class GridColumnWidthReseterBehaviour : Behavior<Expander>
    {
        private Grid _parentGrid;
        public int TargetGridRowIndex { get; set; }
        protected override void OnAttached()
        {
            AssociatedObject.Expanded += AssociatedObject_Expanded;
            AssociatedObject.Collapsed += AssociatedObject_Collapsed;
            FindParentGrid();
        }

        private void FindParentGrid()
        {
            DependencyObject parent = LogicalTreeHelper.GetParent(AssociatedObject);
            while (parent != null)
            {
                if (parent is Grid)
                {
                    _parentGrid = parent as Grid;
                    return;
                }
                parent = LogicalTreeHelper.GetParent(AssociatedObject);
            }
        }

        void AssociatedObject_Collapsed(object sender, System.Windows.RoutedEventArgs e)
        {
            _parentGrid.RowDefinitions[TargetGridRowIndex].Height = GridLength.Auto;
        }

        void AssociatedObject_Expanded(object sender, System.Windows.RoutedEventArgs e)
        {
            _parentGrid.RowDefinitions[TargetGridRowIndex].Height = GridLength.Auto;
        }
    }
}
