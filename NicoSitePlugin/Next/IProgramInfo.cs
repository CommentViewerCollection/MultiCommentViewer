using System.Collections.Generic;

namespace NicoSitePlugin.Next
{
    interface IProgramInfo
    {
        long BeginAt { get; }
        List<string> Categories { get; }
        string Description { get; }
        long EndAt { get; }
        bool IsMemberOnly { get; }
        string ProviderId { get; }
        string ProviderName { get; }
        List<Room> Rooms { get; }
        string Status { get; }
        string Title { get; }
        ProviderType Type { get; }
        long VposBaseAt { get; }
    }
}