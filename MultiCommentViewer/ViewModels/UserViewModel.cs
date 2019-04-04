using SitePlugin;
using MultiCommentViewer.Test;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Common;
using System.Windows.Media;
using System.Collections.Generic;

namespace MultiCommentViewer
{
    public class UserViewModel : CommentDataGridViewModelBase
    {
        public string UserId { get { return User.UserId; } }
        public string Nickname
        {
            get { return _user.Nickname; }
            set
            {
                _user.Nickname = value;
            }
        }
        public IEnumerable<IMessagePart> UsernameItems
        {
            get => _user.Name;
        }
        public bool IsNgUser
        {
            get { return _user.IsNgUser; }
            set
            {
                _user.IsNgUser = value;
            }
        }
        public bool IsSiteNgUser
        {
            get => false;
            set
            {

            }
        }
        public bool IsEnabledUserBackColor
        {
            get { return !string.IsNullOrEmpty(_user.BackColorArgb); }
            set
            {
                var newValue = value;
                if (newValue == false)
                {
                    _user.BackColorArgb = null;
                }
                else
                {
                    if (_user.BackColorArgb == null)
                    {
                        _user.BackColorArgb = "#FFFFFFFF";
                    }
                }
            }
        }
        public bool IsEnabledUserForeColor
        {
            get { return !string.IsNullOrEmpty(_user.ForeColorArgb); }
            set
            {
                var newValue = value;
                if (newValue == false)
                {
                    _user.ForeColorArgb = null;
                }
                else
                {
                    if (_user.ForeColorArgb == null)
                    {
                        _user.ForeColorArgb = "#FF000000";
                    }
                }
            }
        }
        public Color BackColor
        {
            get
            {
                var str = _user.BackColorArgb;
                if (string.IsNullOrEmpty(str))
                {
                    return Common.Utils.ColorFromArgb("#00000000");
                }
                else
                {
                    return Common.Utils.ColorFromArgb(str);
                }
            }
            set
            {
                var color = value;
                _user.BackColorArgb = Common.Utils.ColorToArgb(color);
            }
        }
        public Color ForeColor
        {
            get
            {
                var str = _user.ForeColorArgb;
                if (string.IsNullOrEmpty(str))
                {
                    return Common.Utils.ColorFromArgb("#00000000");
                }
                else
                {
                    return Common.Utils.ColorFromArgb(str);
                }
            }
            set
            {
                var color = value;
                _user.ForeColorArgb = Common.Utils.ColorToArgb(color);
            }
        }
        private readonly IUser _user;
        public override bool IsShowThumbnail { get => false; set { } }
        public override bool IsShowUsername { get => false; set { } }
        public IUser User { get { return _user; } }
        public UserViewModel(IUser user, IOptions option) : base(option)
        {
            _user = user;
            user.PropertyChanged += User_PropertyChanged;
        }
        public UserViewModel(IUser user, IOptions option, ICollectionView comments) 
            : base(option, comments)
        {
            _user = user;
            user.PropertyChanged += User_PropertyChanged;
        }

        private void User_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_user.IsNgUser):
                    RaisePropertyChanged(nameof(IsNgUser));
                    break;
                case nameof(_user.BackColorArgb):
                    RaisePropertyChanged(nameof(IsEnabledUserBackColor));
                    RaisePropertyChanged(nameof(BackColor));
                    break;
                case nameof(_user.ForeColorArgb):
                    RaisePropertyChanged(nameof(IsEnabledUserForeColor));
                    RaisePropertyChanged(nameof(ForeColor));
                    break;
                case nameof(_user.Nickname):
                    RaisePropertyChanged(nameof(Nickname));
                    break;
                case nameof(_user.Name):
                    RaisePropertyChanged(nameof(UsernameItems));
                    break;
            }
        }

        public UserViewModel() : base(new DynamicOptionsTest())
        {
            if ((bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(System.Windows.DependencyObject)).DefaultValue))
            {
                _user = new UserTest("userid_123456")
                {
                    IsNgUser = true,
                    IsSiteNgUser = true,
                    Name = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText("USERNAME") },
                    Nickname = "NICKNAME",
                    BackColorArgb = "#FFCFCFCF",
                    ForeColorArgb = "#FF000000",
                };
            }
        }
    }
}
