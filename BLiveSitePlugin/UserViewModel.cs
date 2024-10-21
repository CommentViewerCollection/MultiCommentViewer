using SitePlugin;
using GalaSoft.MvvmLight;

namespace BLiveSitePlugin
{
    class UserViewModel :ViewModelBase
    {
        private readonly IUser _user;
        public string Name
        {
            get { return _user.Nickname; }
            set
            {
                if (_user.Nickname == value)
                    return;
                _user.Nickname = value;
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
        public UserViewModel(IUser user)
        {
            _user = user;
        }
    }
}
