namespace NicoSitePlugin
{
    interface IInput { }
    class LivePageUrl : IInput
    {
        public string LiveId { get; }
        public LivePageUrl(string liveId)
        {
            LiveId = liveId;
        }
    }
    class ChannelUrl : IInput
    {
        /// <summary>
        /// URLの一番後ろの文字列
        /// </summary>
        public string ChannelScreenName { get; }
        public string Url => "https://ch.nicovideo.jp/" + ChannelScreenName;
        public ChannelUrl(string channelScreenName)
        {
            ChannelScreenName = channelScreenName;
        }
    }
    class CommunityUrl : IInput
    {
        /// <summary>
        /// co\d+
        /// </summary>
        public string CommunityId { get; }
        public CommunityUrl(string communityId)
        {
            CommunityId = communityId;
        }
    }
    class LiveId : IInput
    {
        /// <summary>
        /// lv\d+
        /// </summary>
        public string Raw { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="liveId">lv\d+</param>
        public LiveId(string liveId)
        {
            Raw = liveId;
        }
    }
    class InvalidInput : IInput
    {
        public InvalidInput(string raw)
        {
            Raw = raw;
        }

        public string Raw { get; }
    }
}
