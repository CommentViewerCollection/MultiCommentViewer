using GalaSoft.MvvmLight;
using SitePlugin;

//TODO:過去コメントの取得


namespace MultiCommentViewer
{
    public class SiteViewModel : ViewModelBase
    {
        public string DisplayName { get { return _site.DisplayName; } }
        private readonly ISiteContext _site;
        public ISiteContext Site { get { return _site; } }
        public SiteViewModel(ISiteContext site)
        {
            _site = site;
        }
    }

}
