using System;
using Common;
using GalaSoft.MvvmLight;
using SitePlugin;

namespace OpenrecYoyakuPlugin
{
    public class User : ViewModelBase
    {
        private void SetName()
        {
            if (string.IsNullOrEmpty(_user.Nickname))
            {
                this.Name = _user.Name.ToText();
            }
            else
            {
                this.Name = _user.Nickname;
            }
        }
        public User(IUser user)
        {
            _user = user;
            user.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(user.Name):
                    case nameof(user.Nickname):
                        SetName();
                        break;
                }
            };
        }
        private DateTime _date;
        public DateTime Date
        {
            get { return _date; }
            set
            {
                _date = value;
                RaisePropertyChanged();
            }
        }
        private string _id;
        public string Id
        {
            get { return _id; }
            set
            {
                _id = value;
                RaisePropertyChanged();
            }
        }
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged();
            }
        }
        public Guid SitePluginGuid { get; set; }
        private bool _hasCalled;
        private readonly IUser _user;

        /// <summary>
        /// 呼び出し済みか
        /// </summary>
        public bool HadCalled
        {
            get { return _hasCalled; }
            set
            {
                _hasCalled = value;
                RaisePropertyChanged();
            }
        }
        public override string ToString()
        {
            return $"{Name} id={Id}";
        }
        public override bool Equals(object obj)
        {
            if (!(obj is User user))
                return false;
            if (this.Id == null) return false;
            return Id.Equals(user.Id);
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
