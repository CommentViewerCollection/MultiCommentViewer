using System;

namespace NicoSitePlugin
{
    public class SpecChangedException : Exception
    {
        public string Raw { get; }
        public SpecChangedException(string raw)
        {
            Raw = raw;
        }
        public SpecChangedException(string message, string raw)
            : base(message)
        {
            Raw = raw;
        }
    }
}
