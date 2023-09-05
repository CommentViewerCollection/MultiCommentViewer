using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigoSitePlugin
{
    [Serializable]
    public class PostingCommentFailedException : Exception
    {
        public string Details { get; }
        public PostingCommentFailedException(string message, string details) : base(message)
        {
            Details = details;
        }
    }
    [Serializable]
    public class UnknownResponseReceivedException : Exception
    {
        public string Details { get; }
        public UnknownResponseReceivedException(string details) : base()
        {
            Details = details;
        }
    }
    [Serializable]
    public class FatalException : Exception
    {
        public FatalException() { }
        public FatalException(string message, Exception innterException) : base(message, innterException) { }
    }
    [Serializable]
    public class ReloadException : Exception
    {
        public ReloadException() { }
        public ReloadException(Exception innterException) : base("", innterException) { }
    }
    [Serializable]
    public class ContinuationContentsNullException : Exception
    {
        public ContinuationContentsNullException() { }
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
        public ParseException(string raw, Exception inner) : base("", inner) { }
    }
    [Serializable]
    public class BigoServerErrorException : Exception
    {
        public BigoServerErrorException() { }
    }
    [Serializable]
    public class SpecChangedException : Exception
    {
        public string Raw { get; }
        public SpecChangedException(string raw)
        {
            Raw = raw;
        }
        public SpecChangedException(string raw, Exception inner) : base("", inner) { }
    }
}
