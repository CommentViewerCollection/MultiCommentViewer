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

namespace Common.AutoUpdate
{
    /// <summary>
    /// Interaction logic for UpdateNotExistsPage.xaml
    /// </summary>
    public partial class UpdateNotExistsPage : Page
    {
        public UpdateNotExistsPage()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var view = Window.GetWindow(this);
            view?.Close();
        }
    }
}
