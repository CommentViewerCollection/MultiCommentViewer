using System;
namespace BLiveSitePlugin
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
}
