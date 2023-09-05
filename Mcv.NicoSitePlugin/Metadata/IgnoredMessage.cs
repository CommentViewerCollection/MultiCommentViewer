namespace NicoSitePlugin.Metadata
{
    /// <summary>
    /// 不要だから無視するメッセージ
    /// </summary>
    class IgnoredMessage : IMetaMessage
    {
        public IgnoredMessage(string raw)
        {
            Raw = raw;
        }

        public string Raw { get; }
    }
}