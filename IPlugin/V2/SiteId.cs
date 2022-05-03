#nullable enable
using System;

namespace Mcv.PluginV2
{
    //現状、SitePluginのIDとして使用していて、配信サイトのIDのつもりでSiteIdを使用している。
    //でも配信サイトにIDを振る意味はあるのだろうか。
    //同じ配信サイトのコメントを取得するプラグインを複数種類、並行して使用するような状況では必要になる思うけど、想像つかない。
    //いずれPluginIdに置き換えても良い気がする。
    public class SiteId
    {
        private readonly Guid _guid;

        public SiteId(Guid guid)
        {
            _guid = guid;
        }
        public override bool Equals(object? obj)
        {
            if (!(obj is SiteId b))
            {
                return false;
            }
            return _guid.Equals(b._guid);
        }
        public override int GetHashCode()
        {
            return _guid.GetHashCode();
        }
        public override string ToString()
        {
            return $"{{\"_guid\":\"{_guid}\"}}";
        }
    }
}
