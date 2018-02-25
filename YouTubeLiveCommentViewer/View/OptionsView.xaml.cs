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
using System.Windows.Shapes;
using SitePlugin;
using System.Diagnostics;
using GalaSoft.MvvmLight.Messaging;
using Common.Wpf;
using System.ComponentModel;

namespace YouTubeLiveCommentViewer
{
    /// <summary>
    /// Interaction logic for OptionsView.xaml
    /// </summary>
    public partial class OptionsView : Window
    {
        List<IOptionsTabPage> _pagePanels = new List<IOptionsTabPage>();
        FontSelectorView fontSelector;
        public OptionsView()
        {
            InitializeComponent();
            Messenger.Default.Register<ShowFontSelectorViewOkMessage>(this, _ =>
            {
                //FontSelectorView fontSelector = null;
                try
                {
                    if (fontSelector == null)
                    {
                        fontSelector = new FontSelectorView();
                        fontSelector.Owner = this;
                    }
                    var resource = Application.Current.Resources;
                    var locator = resource["Locator"] as ViewModel.ViewModelLocator;
                    fontSelector.DataContext = locator.Font;
                    var showPos = Tools.GetShowPos(Tools.GetMousePos(), fontSelector);
                    
                    //fontSelector.Owner = this;
                    fontSelector.Left = showPos.X;
                    fontSelector.Top = showPos.Y;
                    fontSelector.ShowDialog();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                finally
                {
                    fontSelector = null;
                }
            });
        }
        public void AddTabPage(IOptionsTabPage page)
        {
            var tabPage = new TabItem()
            {
                Header = page.HeaderText,
                Content = page.TabPagePanel,
            };
            tabControl.Items.Add(tabPage);
            _pagePanels.Add(page);
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
            base.OnClosing(e);
        }
        public void Clear()
        {
            tabControl.Items.Clear();
            _pagePanels.Clear();
        }
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _pagePanels.ForEach(panel => panel.Apply());
                Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _pagePanels.ForEach(panel => panel.Cancel());
                Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

    }
}
