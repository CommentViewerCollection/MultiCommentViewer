using CommunityToolkit.Mvvm.ComponentModel;
using SitePlugin;
using System;

namespace MultiCommentViewer
{
    public class SiteViewModel : ObservableObject
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
