using SitePlugin;
using System.Windows.Media;

namespace WhowatchSitePlugin
{
    internal class ItemMessageMetadata : MessageMetadataBase
    {
        public override Color BackColor => _siteOptions.ItemBackColor;
        public override Color ForeColor => _siteOptions.ItemForeColor;
        public ItemMessageMetadata(IWhowatchItem message, ICommentOptions options, IWhowatchSiteOptions siteOptions, IUser user, ICommentProvider cp)
            : base(options, siteOptions, cp)
        {
            User = user;
            user.PropertyChanged += User_PropertyChanged;
        }
        private void User_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(User.IsNgUser):
                    //case nameof(User.IsSiteNgUser):
                    RaisePropertyChanged(nameof(IsVisible));
                    break;
            }
        }
    }
}
