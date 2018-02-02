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
using System.ComponentModel;
namespace YouTubeLiveSitePlugin.Old
{
    /// <summary>
    /// Interaction logic for YouTubeLiveOptionsPanel.xaml
    /// </summary>
    public partial class YouTubeLiveOptionsPanel : UserControl
    {
        public YouTubeLiveOptionsPanel()
        {
            InitializeComponent();
        }
        public void SetViewModel(YouTubeLiveOptionsViewModel vm)
        {
            this.DataContext = vm;
        }
        public YouTubeLiveOptionsViewModel GetViewModel()
        {
            return (YouTubeLiveOptionsViewModel)this.DataContext;
        }
    }
    public class YouTubeLiveOptionsViewModel : INotifyPropertyChanged
    {
        private readonly YouTubeSiteOptions _origin;
        private readonly YouTubeSiteOptions changed;
        internal YouTubeSiteOptions OriginOptions { get { return _origin; } }
        internal YouTubeSiteOptions ChangedOptions { get { return changed; } }

        internal YouTubeLiveOptionsViewModel(YouTubeSiteOptions siteOptions)
        {
            _origin = siteOptions;
            changed = siteOptions.Clone();
        }

        #region INotifyPropertyChanged
        [NonSerialized]
        private System.ComponentModel.PropertyChangedEventHandler _propertyChanged;
        /// <summary>
        /// 
        /// </summary>
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged
        {
            add { _propertyChanged += value; }
            remove { _propertyChanged -= value; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            _propertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
