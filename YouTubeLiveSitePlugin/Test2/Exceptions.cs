using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouTubeLiveSitePlugin.Test2
{
    [Serializable]
    public class ReloadException : Exception
    {
        public ReloadException() { }
    }
    [Serializable]
    public class ContinuationContentsNullException : Exception
    {
        public ContinuationContentsNullException() { }
    }

    [Serializable]
    public class ParseException : Exception
    {
        public ParseException() { }
        public ParseException(string message) : base(message) { }
        public ParseException(string message, Exception inner) : base(message, inner) { }
    }

    [Serializable]
    public class NoContinuationException : Exception
    {
        public NoContinuationException() { }
    }

    [Serializable]
    public class ChatUnavailableException : Exception
    {
        public ChatUnavailableException() { }
    }
}
