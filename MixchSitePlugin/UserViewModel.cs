using CommunityToolkit.Mvvm.ComponentModel;
using SitePlugin;

namespace MixchSitePlugin
{
    class UserViewModel : ObservableObject
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
                OnPropertyChanged();
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
                OnPropertyChanged();
            }
        }
        public UserViewModel(IUser user)
        {
            _user = user;
        }
    }
}
