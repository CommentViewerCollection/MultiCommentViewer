namespace NicoSitePlugin.Chat
{
    /// <summary>
    /// 未ログインの場合にChatProviderに渡すもの
    /// </summary>
    class ChatGuestOptions : IChatOptions
    {
        public string Thread { get; set; }
        public string UserId { get; set; }
    }
}
