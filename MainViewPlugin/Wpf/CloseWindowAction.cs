using Microsoft.Xaml.Behaviors;
using System.Windows;
namespace Mcv.MainViewPlugin.Wpf
{
    public class CloseWindowAction : TriggerAction<FrameworkElement>
    {
        protected override void Invoke(object parameter)
            => Window.GetWindow(AssociatedObject).Close();
    }
}

