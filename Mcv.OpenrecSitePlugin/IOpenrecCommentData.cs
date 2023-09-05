using System;
using System.Collections.Generic;
using Mcv.PluginV2;

namespace OpenrecSitePlugin
{
    interface IOpenrecCommentData
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
        List<IMessagePart> NameIcons { get; }
        DateTime PostTime { get; }
        TimeSpan Elapsed { get; }
    }
}