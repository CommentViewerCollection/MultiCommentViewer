using SitePlugin;
using MultiCommentViewer.Test;

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
        public string Username { get; set; }
        private readonly IUser _user;
        public override bool IsShowThumbnail { get => false; set { } }
        public override bool IsShowUsername { get => false; set { } }
        public IUser User { get { return _user; } }
        public UserViewModel(IUser user, IOptions option) : base(option)
        {
            _user = user;
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
            }
        }
    }
}
