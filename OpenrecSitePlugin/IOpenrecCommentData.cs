using System;
using System.Collections.Generic;
using SitePlugin;

namespace OpenrecSitePlugin
{
    interface IOpenrecCommentData
    {
        bool IsYell { get; }
        string YellPoints { get; }
        string Id { get; }
        IMessageText Message { get; }
        string UserId { get; }
        string UserType { get; }
        string UserKey { get; }
        IMessageImage Stamp { get; }
        string Name { get; }
        List<IMessagePart> NameIcons { get; }
        DateTime PostTime { get; }
        TimeSpan Elapsed { get; }
    }
}