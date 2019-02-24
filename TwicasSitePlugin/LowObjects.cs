using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TwicasSitePlugin.LowObject
{
    public class LiveContext
    {
        public long MovieId { get; set; }
        public int MovieCnum { get; set; }
        public string AudienceId { get; internal set; }
        public string CsSessionId { get; internal set; }
    }
    public class ListUpdate
    {
        public List<Comment> comment { get; set; }
        public int cnum { get; set; }
        public string edit { get; set; }
    }
    public class Comment
    {
        public long id { get; set; }
        //other
        //other oldcomment
        //owner
        public string @class { get; set; }
        public string html { get; set; }
        public string date { get; set; }
        public string dur { get; set; }
        public string uid { get; set; }
        public string screen { get; set; }
        public string statusid { get; set; }
        public int lat { get; set; }
        public int lng { get; set; }
        public bool show { get; set; }
        public string yomi { get; set; }
    }
}
