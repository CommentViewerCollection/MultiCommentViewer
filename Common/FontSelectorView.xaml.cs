using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Common.Wpf
{
    /// <summary>
    /// Interaction logic for FontSelectorView.xaml
    /// </summary>
    public partial class FontSelectorView : Window
    {
        public FontSelectorView()
        {
            InitializeComponent();
            WeakReferenceMessenger.Default.Register<FontSelectorViewOkMessage>(this, (_, _) =>
            {
                //DialogResultに値を入れると一回目は正常に動作するが、2回目は例外が発生してしまう。DialogResultに2回以上値を入れてはいけないらしい。
                this.Close();
            });
            WeakReferenceMessenger.Default.Register<FontSelectorViewCancelMessage>(this, (_, _) =>
            {
                this.Close();
            });
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
            //Messenger.Default.Unregister<FontSelectorViewOkMessage>(this);
            //Messenger.Default.Unregister<FontSelectorViewCancelMessage>(this);
            base.OnClosing(e);
        }
    }
}
