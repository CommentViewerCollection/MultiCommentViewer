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
using Common;
namespace TwitchSitePlugin
{
    /// <summary>
    /// Interaction logic for TabPagePanel.xaml
    /// </summary>
    public partial class TabPagePanel : UserControl
    {
        internal TabPagePanel()
        {
            InitializeComponent();
        }
        internal void SetViewModel(TwitchSiteOptionsViewModel vm)
        {
            this.DataContext = vm;
        }
        internal TwitchSiteOptionsViewModel GetViewModel()
        {
            return (TwitchSiteOptionsViewModel)this.DataContext;
        }
        
    }
    class TwitchSiteOptionsViewModel : INotifyPropertyChanged
    {
        private readonly TwitchSiteOptions _origin;
        private readonly TwitchSiteOptions changed;
        internal TwitchSiteOptions OriginOptions { get { return _origin; } }
        internal TwitchSiteOptions ChangedOptions { get { return changed; } }

        internal TwitchSiteOptionsViewModel(TwitchSiteOptions siteOptions)
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
    internal class TwitchSiteOptions : DynamicOptionsBase
    {
        protected override void Init()
        {
        }
        internal TwitchSiteOptions Clone()
        {
            return (TwitchSiteOptions)this.MemberwiseClone();
        }
        internal void Set(TwitchSiteOptions changedOptions)
        {
            foreach (var src in changedOptions.Dict)
            {
                var v = src.Value;
                SetValue(v.Value, src.Key);
            }
        }
    }
}
