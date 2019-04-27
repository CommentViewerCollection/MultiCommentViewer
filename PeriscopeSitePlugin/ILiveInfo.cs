namespace PeriscopeSitePlugin
{
    internal interface ILiveInfo
    {
        string Title { get; }
        /// <summary>
        /// 
        /// LIVE
        /// FINISHED
        /// </summary>
        string LiveStatus { get; }
        string ChatUrl { get; }
    }
}
