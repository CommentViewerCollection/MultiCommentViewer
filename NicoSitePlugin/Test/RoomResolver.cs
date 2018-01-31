using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NicoSitePlugin.Test
{
    public abstract class RoomResolverBase
    {
        public abstract RoomInfo[] GetRooms();
        public RoomResolverBase()
        {

        }
    }
    public class ChannelRoomResolver : RoomResolverBase
    {
        public override RoomInfo[] GetRooms()
        {
            return new[] { _current };
        }
        private readonly RoomInfo _current;
        public ChannelRoomResolver(RoomInfo current, INicoSiteOptions siteOptions)
        {
            _current = current;
        }
    }
    public class CommunityRoomResolver : RoomResolverBase
    {
        public override RoomInfo[] GetRooms()
        {
            return new[] { _current };
        }
        private readonly RoomInfo _current;
        public CommunityRoomResolver(RoomInfo current, INicoSiteOptions siteOptions)
        {
            _current = current;
        }
    }
    public class OfficialRoomResolver : RoomResolverBase
    {
        public override RoomInfo[] GetRooms()
        {
            return new[] { _current };
        }
        private readonly RoomInfo _current;
        public OfficialRoomResolver(RoomInfo current, INicoSiteOptions siteOptions)
        {
            _current = current;
        }
    }
}
