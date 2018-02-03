using System;
using System.ComponentModel;
namespace BouyomiPlugin
{
    class Options : INotifyPropertyChanged
    {
        public bool IsEnabled { get; set; }
        private string _BouyomiChanPath;
        public string BouyomiChanPath
        {
            get { return _BouyomiChanPath; }
            set
            {
                if (_BouyomiChanPath == value)
                    return;
                _BouyomiChanPath = value;
                RaisePropertyChanged();
            }
        }
        public bool IsReadHandleName { get; set; }
        public bool IsReadComment { get; set; }
        public bool IsAppendNickTitle { get; set; }
        public string NickTitle { get; set; } = "さん";

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
