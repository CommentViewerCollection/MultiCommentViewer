using SitePlugin;

namespace BLiveSitePlugin
{
    public class Metadata : IMetadata
    {
        public string Title { get; set; }
        public string Elapsed { get; set; }
        public string CurrentViewers { get; set; }
        public string Active { get; set; }
        public string TotalViewers { get; set; }
        public bool? IsLive { get; set; }
        public string Others { get; }
    }
}
