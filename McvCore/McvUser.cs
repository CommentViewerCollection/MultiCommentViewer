using Mcv.PluginV2;
using System.Collections.Generic;

namespace Mcv.Core;

class McvUser
{
    public string UserId { get; }
    public IEnumerable<IMessagePart>? Name { get; set; }
    public string? Nickname { get; set; }
    public bool IsNgUser { get; set; }
    public bool IsSiteNgUser { get; set; }
    public string? BackColorArgb { get; set; }
    public string? ForeColorArgb { get; set; }
    public McvUser(string userId)
    {
        UserId = userId;
    }
}
