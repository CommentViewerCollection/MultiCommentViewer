namespace SitePlugin
{
    /// <summary>
    /// 
    /// </summary>
    /// プラグインにはどのサイトなのか伝える必要があると判断。そのためにはこれが必要。
    /// ただしコメビュ内では使いたくない。抽象化が薄れてしまう。
    public enum SiteType
    {
        NicoLive,
        YouTubeLive,
        Openrec,
        /// <summary>
        /// ふわっち
        /// </summary>
        Whowatch,
        LineLive,
        Mirrativ,
        Twicas,
        Twitch,
    }
}
