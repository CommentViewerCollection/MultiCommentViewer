using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Common.Wpf
{
    public class BindableTextBlock : TextBlock
    {
        public ObservableCollection<Inline> InlineList
        {
            get { return (ObservableCollection<Inline>)GetValue(InlineListProperty); }
            set { SetValue(InlineListProperty, value); }
        }

        public static readonly DependencyProperty InlineListProperty =
            DependencyProperty.Register(nameof(InlineList), typeof(ObservableCollection<Inline>), typeof(BindableTextBlock), new UIPropertyMetadata(null, OnPropertyChanged));

        private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            BindableTextBlock textBlock = sender as BindableTextBlock;
            ObservableCollection<Inline> list = e.NewValue as ObservableCollection<Inline>;
            if (textBlock == null || list == null) { return; };
            textBlock.Inlines.Clear();
            foreach (Inline inline in list)
            {
                textBlock.Inlines.Add(inline);
            }
        }
    }
}
