using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NicoSitePlugin
{
    [Serializable]
    public class GetPostKeyFailedException : Exception
    {
        public string Response { get; }
        public GetPostKeyFailedException(string res)
        {
            Response = res;
        }
    }
    [Serializable]
    public class MembersOnlyCommunityException : Exception
    {
        public string LiveId { get; set; }
        public MembersOnlyCommunityException()
        {
        }
    }
    [Serializable]
    public class JikkyoInfoFailedException : Exception
    {
        public string Reason { get; }
        public string Url { get; }
        public string Raw { get; }
        public JikkyoInfoFailedException(string raw, string url, string reason)
        {
            Raw = raw;
            Url = url;
            Reason = reason;
        }
    }
    [Serializable]
    public class ParseException : Exception
    {
        public string Raw { get; }
        public ParseException() { }
        public ParseException(string raw)
        {
            Raw = raw;
        }
        public ParseException(string raw, Exception inner) : base("", inner)
        {
            Raw = raw;
        }
    }

    [Serializable]
    public class NicoException : Exception
    {
        public string Env { get; set; }
        public NicoException(string message, string env) : base(message)
        {
            Env = env;
        }
        public NicoException(string message, string env, Exception inner) : base(message, inner)
        {
            Env = env;
        }
        protected NicoException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
