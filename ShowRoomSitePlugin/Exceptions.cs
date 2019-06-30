using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowRoomSitePlugin
{
    [Serializable]
    public class ParseException : Exception
    {
        public string Raw { get; }
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
    public class LiveNotFoundException : Exception
    {
        public LiveNotFoundException() { }
    }
    [Serializable]
    public class InvalidUrlException : Exception
    {
        public InvalidUrlException() { }
    }
}
