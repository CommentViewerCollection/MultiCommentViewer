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

namespace BouyomiPlugin
{
    /// <summary>
    /// Interaction logic for ConfigView.xaml
    /// </summary>
    public partial class ConfigView : Window
    {
        public ConfigView()
        {
            InitializeComponent();
            _isForceClose = false;
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
    }
}
