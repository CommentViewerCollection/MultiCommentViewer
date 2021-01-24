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
    }
}
