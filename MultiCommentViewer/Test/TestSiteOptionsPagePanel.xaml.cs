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

namespace MultiCommentViewer.Test
{
    /// <summary>
    /// Interaction logic for TestSiteOptionsPagePanel.xaml
    /// </summary>
    public partial class TestSiteOptionsPagePanel : UserControl
    {
        public TestSiteOptionsPagePanel()
        {
            InitializeComponent();
        }
        public void SetViewModel(TestSiteOptionsViewModel vm)
        {
            this.DataContext = vm;
        }
        public TestSiteOptionsViewModel GetViewModel()
        {
            return (TestSiteOptionsViewModel)this.DataContext;
        }
    }
}
