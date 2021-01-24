﻿using System.Collections.Generic;
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
using SitePlugin;

namespace NicoSitePlugin
{
    /// <summary>
    /// Interaction logic for NicoOptionsPanel.xaml
    /// </summary>
    public partial class NicoOptionsPanel : UserControl
    {
        public NicoOptionsPanel()
        {
            InitializeComponent();
        }
        public void SetViewModel(NicoSiteOptionsViewModel vm)
        {
            this.DataContext = vm;
        }
        public NicoSiteOptionsViewModel GetViewModel()
        {
            return (NicoSiteOptionsViewModel)this.DataContext;
        }
    }
}
