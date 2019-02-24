using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
namespace YouTubeLiveSitePlugin.Test2
{
    public class YouTubeLiveOptionsViewModel : INotifyPropertyChanged
    {
        public Color PaidCommentBackColor
        {
            get { return ChangedOptions.PaidCommentBackColor; }
            set { ChangedOptions.PaidCommentBackColor = value; }
        }
        public Color PaidCommentForeColor
        {
            get { return ChangedOptions.PaidCommentForeColor; }
            set { ChangedOptions.PaidCommentForeColor = value; }
        }
        public bool IsAutoSetNickname
        {
            get { return ChangedOptions.IsAutoSetNickname; }
            set { ChangedOptions.IsAutoSetNickname = value; }
        }
        private readonly YouTubeLiveSiteOptions _origin;
        private readonly YouTubeLiveSiteOptions changed;
        internal YouTubeLiveSiteOptions OriginOptions { get { return _origin; } }
        internal YouTubeLiveSiteOptions ChangedOptions { get { return changed; } }

        internal YouTubeLiveOptionsViewModel(YouTubeLiveSiteOptions siteOptions)
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
