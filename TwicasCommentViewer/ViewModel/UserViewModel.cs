using SitePlugin;
using Common;
using System;
using System.ComponentModel;
using System.Windows.Media;
using System.Collections.Generic;

namespace TwicasCommentViewer.ViewModel
{
    public class UserViewModel : CommentDataGridViewModelBase
    {
        public string UserId { get { return User.UserId; } }
        public IEnumerable<IMessagePart> Name
        {
            get => _user.Name;
            set => _user.Name = value;
        }
        public string Nickname
        {
            get { return _user.Nickname; }
            set
            {
                if (_user.Nickname == value)
                    return;
                _user.Nickname = value;
            }
        }
        //public string Username { get; set; }
        public bool IsNgUser
        {
            get { return _user.IsNgUser; }
            set
            {
                _user.IsNgUser = value;
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
                    return ColorFromArgb("#00000000");
                }
                else
                {
                    return ColorFromArgb(str);
                }
            }
            set
            {
                var color = value;
                _user.BackColorArgb = ColorToArgb(color);
            }
        }
        public Color ForeColor
        {
            get
            {
                var str = _user.ForeColorArgb;
                if (string.IsNullOrEmpty(str))
                {
                    return ColorFromArgb("#00000000");
                }
                else
                {
                    return ColorFromArgb(str);
                }
            }
            set
            {
                var color = value;
                _user.ForeColorArgb = ColorToArgb(color);
            }
        }

        private readonly IUser _user;
        public override bool IsShowThumbnail { get => false; set { } }
        public override bool IsShowUsername { get => false; set { } }
        public IUser User { get { return _user; } }
        public UserViewModel(IUser user, IOptions option, ICollectionView comments)
            : base(option, comments)
        {
            _user = user;
            Name = user.Name;
            _user.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(_user.BackColorArgb):
                        RaisePropertyChanged(nameof(BackColor));
                        break;
                    case nameof(_user.ForeColorArgb):
                        RaisePropertyChanged(nameof(ForeColor));
                        break;
                    case nameof(_user.Nickname):
                        RaisePropertyChanged(nameof(Nickname));
                        break;
                }
            };
        }

        private void _user_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public UserViewModel() : base(new DynamicOptionsTest())
        {
            if (IsInDesignMode)
            {
                _user = new UserTest("userid_123456")
                {
                    Nickname = "NICKNAME",
                    BackColorArgb = "#FFCFCFCF",
                    ForeColorArgb = "#FF000000",
                };
                Name = new List<IMessagePart> { new MessageText("USERNAME") };
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        private Color ColorFromArgb(string argb)
        {
            if (argb == null)
                throw new ArgumentNullException("argb");
            var pattern = "#(?<a>[0-9a-fA-F]{2})(?<r>[0-9a-fA-F]{2})(?<g>[0-9a-fA-F]{2})(?<b>[0-9a-fA-F]{2})";
            var match = System.Text.RegularExpressions.Regex.Match(argb, pattern, System.Text.RegularExpressions.RegexOptions.Compiled);

            if (!match.Success)
            {
                throw new ArgumentException("形式が不正");
            }
            else
            {
                var a = byte.Parse(match.Groups["a"].Value, System.Globalization.NumberStyles.HexNumber);
                var r = byte.Parse(match.Groups["r"].Value, System.Globalization.NumberStyles.HexNumber);
                var g = byte.Parse(match.Groups["g"].Value, System.Globalization.NumberStyles.HexNumber);
                var b = byte.Parse(match.Groups["b"].Value, System.Globalization.NumberStyles.HexNumber);
                return Color.FromArgb(a, r, g, b);
            }
        }
        private string ColorToArgb(Color color)
        {
            var argb = color.ToString();
            return argb;
        }
    }
}
