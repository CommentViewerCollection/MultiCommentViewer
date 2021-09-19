using System;
using System.Collections.Generic;
using SitePlugin;

namespace MixchSitePlugin
{
    interface IMixchCommentData
    {
        string UserIconUrl { get; }
        bool IsYell { get; }
        string YellPoints { get; }
        string Id { get; }
        string Message { get; }
        string UserId { get; }
        string UserType { get; }
        string UserKey { get; }
        IMessageImage Stamp { get; }
        string Name { get; }
        DateTime PostTime { get; }
        TimeSpan Elapsed { get; }
    }
}
