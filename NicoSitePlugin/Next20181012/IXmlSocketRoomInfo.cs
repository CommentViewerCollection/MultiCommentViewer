namespace NicoSitePlugin.Next20181012
{
    interface IXmlSocketRoomInfo
    {
        string XmlSocketAddr { get; }
        int XmlSocketPort { get; }
        string ThreadId { get; }
        /// <summary>
        /// 部屋名
        /// </summary>
        string Name { get; }
    }
}
