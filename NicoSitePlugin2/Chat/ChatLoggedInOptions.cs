namespace NicoSitePlugin.Chat
{
    /// <summary>
    /// ログイン済みの場合にChatProviderに渡すもの
    /// </summary>
    class ChatLoggedInOptions : IChatOptions
    {
        public string Thread { get; set; }
        public string UserId { get; set; }
        public string ThreadKey { get; set; }
    }
}
