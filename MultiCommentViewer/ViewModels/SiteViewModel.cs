using GalaSoft.MvvmLight;
using SitePlugin;
using System;

namespace MultiCommentViewer
{
    public class SiteViewModel : ViewModelBase
    {
        public string DisplayName { get; }
        public Guid Guid { get; }
        //private readonly ISiteContext _site;
        //public ISiteContext Site { get { return _site; } }
        //public SiteViewModel(ISiteContext site)
        //{
        //    _site = site;
        //}
        public SiteViewModel(string displayName, Guid guid)
        {
            DisplayName = displayName;
            Guid = guid;
        }
    }

}
