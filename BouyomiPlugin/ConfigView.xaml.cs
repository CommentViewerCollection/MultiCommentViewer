using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            DataObject.AddPastingHandler(TwitchMaxEmotes, TextBoxPastingEventHandler);
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

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 数字のみ入力可
            if (!Regex.IsMatch(e.Text, "[0-9]"))
            {
                e.Handled = true;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.Source is TextBox t)
            {
                if ((0 < t.MaxLength) && (t.MaxLength < t.Text.Length))
                {
                    int start = t.SelectionStart;
                    t.Text = t.Text.Substring(0, 3);
                    t.SelectionStart = start;
                }
            }
        }

        private static void TextBoxPastingEventHandler(object sender, DataObjectPastingEventArgs e)
        {
            if (e.Source is TextBox t)
            {
                if (e.DataObject.GetData(typeof(string)) is string s)
                {
                    // 数字のみ貼り付け可
                    t.SelectedText = Regex.Replace(s, "[^0-9]", "");
                    t.SelectionStart += t.SelectionLength;
                    t.SelectionLength = 0;
                    e.CancelCommand();
                    e.Handled = true;
                }
            }
        }
    }
}
