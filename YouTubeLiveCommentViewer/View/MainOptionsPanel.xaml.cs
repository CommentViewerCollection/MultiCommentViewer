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
using SitePlugin;
namespace YouTubeLiveCommentViewer
{
    /// <summary>
    /// Interaction logic for MainOptionsPanel.xaml
    /// </summary>
    public partial class MainOptionsPanel : UserControl
    {
        public MainOptionsPanel()
        {
            InitializeComponent();
        }
        internal void SetViewModel(MainOptionsViewModel vm)
        {
            this.DataContext = vm;
        }
        internal MainOptionsViewModel GetViewModel()
        {
            return (MainOptionsViewModel)this.DataContext;
        }
    }
    class MainOptionsViewModel
    {
        public Color BackColor
        {
            get { return ChangedOptions.BackColor; }
            set { ChangedOptions.BackColor = value; }
        }
        public Color ForeColor
        {
            get { return ChangedOptions.ForeColor; }
            set { ChangedOptions.ForeColor = value; }
        }
        private readonly IOptions _origin;
        private readonly IOptions changed;
        public IOptions OriginOptions { get { return _origin; } }
        public IOptions ChangedOptions { get { return changed; } }
        public MainOptionsViewModel(IOptions options)
        {
            _origin = options;
            changed = options.Clone();
        }
        public MainOptionsViewModel()
        {
            if (GalaSoft.MvvmLight.ViewModelBase.IsInDesignModeStatic)
            {
                _origin = new DynamicOptionsTest
                {
                    ForeColor = Colors.Red,
                    BackColor = Colors.Black,
                };
                changed = _origin.Clone();
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
    class MainTabPage : IOptionsTabPage
    {
        public string HeaderText { get; }

        public UserControl TabPagePanel => _panel;

        public void Apply()
        {
            var optionsVm = _panel.GetViewModel();
            optionsVm.OriginOptions.Set(optionsVm.ChangedOptions);
        }

        public void Cancel()
        {
        }
        private readonly MainOptionsPanel _panel;
        public MainTabPage(string displayName, MainOptionsPanel panel)
        {
            HeaderText = displayName;
            _panel = panel;
        }
    }
}
