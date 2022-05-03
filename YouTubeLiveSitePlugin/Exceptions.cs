using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mcv.YouTubeLiveSitePlugin
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
    public class YtInitialDataNotFoundException : Exception
    {
        public string Url { get; }
        public string Html { get; }
        public YtInitialDataNotFoundException() { }
        public YtInitialDataNotFoundException(string url, string html)
        {
            Url = url;
            Html = html;
        }
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
        public ParseException(string raw, Exception inner) : base("", inner)
        {
            Raw = raw;
        }
    }

    [Serializable]
    public class ContinuationNotExistsException : Exception
    {
        public string Raw { get; }
        public ContinuationNotExistsException() { }
        public ContinuationNotExistsException(string raw)
        {
            Raw = raw;
        }
    }

    [Serializable]
    public class ChatUnavailableException : Exception
    {
        public string Raw { get; }
        public ChatUnavailableException() { }
        public ChatUnavailableException(string raw)
        {
            Raw = raw;
        }
    }
    [Serializable]
    public class YouTubeLiveServerErrorException : Exception
    {
        public YouTubeLiveServerErrorException() { }
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
