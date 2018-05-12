using System;
using System.Collections.Generic;
using SitePlugin;
namespace Common
{
    public class UserTest : IUser
    {
        public string UserId { get { return _userid; } }

        private string _foreColorArgb;
        public string ForeColorArgb
        {
            get { return _foreColorArgb; }
            set
            {
                if (_foreColorArgb == value) return;
                _foreColorArgb = value;
                RaisePropertyChanged();
            }
        }

        private string _backColorArgb;
        public string BackColorArgb
        {
            get { return _backColorArgb; }
            set
            {
                if (_backColorArgb == value) return;
                _backColorArgb = value;
                RaisePropertyChanged();
            }
        }

        private bool _isNgUser;
        public bool IsNgUser
        {
            get { return _isNgUser; }
            set
            {
                if (_isNgUser == value) return;
                _isNgUser = value;
                RaisePropertyChanged();
            }
        }
        private string _nickname;
        public string Nickname
        {
            get { return _nickname; }
            set
            {
                if (_nickname == value)
                    return;
                _nickname = value;
                RaisePropertyChanged();
            }
        }

        private IEnumerable<IMessagePart> _name;
        public IEnumerable<IMessagePart> Name
        {
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value;
                RaisePropertyChanged();
            }
        }
        private readonly string _userid;
        public UserTest(string userId)
        {
            _userid = userId;
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
