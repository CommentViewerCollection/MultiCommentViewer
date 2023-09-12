﻿using Mcv.PluginV2;

namespace WhowatchSitePlugin
{
    internal class WhowatchMessageContext : IMessageContext
    {
        public ISiteMessage Message { get; }
        public string? UserId { get; }
        public string? NewNickname { get; }
        public bool IsInitialComment { get; }

        public WhowatchMessageContext(IWhowatchMessage message, string? userId, string? newNickname, bool isInitialComment)
        {
            Message = message;
            UserId = userId;
            NewNickname = newNickname;
            IsInitialComment = isInitialComment;
        }
    }
}
