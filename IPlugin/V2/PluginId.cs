#nullable enable
using System;

namespace Mcv.PluginV2
{
    public class PluginId : IdBase
    {
        public PluginId(Guid guid) : base(guid) { }
        public static PluginId New()
        {
            return new PluginId(Guid.NewGuid());
        }
    }
}