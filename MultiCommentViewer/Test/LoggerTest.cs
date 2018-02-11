using System;
using SitePlugin;
using Common;
namespace MultiCommentViewer.Test
{
    public class LoggerTest : ILogger
    {
        System.Collections.Concurrent.BlockingCollection<ExceptionContext> _exCollection = new System.Collections.Concurrent.BlockingCollection<ExceptionContext>();
        public void LogException(Exception ex, string title = "", string detail = "")
        {
            _exCollection.Add(new ExceptionContext { Ex = ex, Title = title, Detail = detail });
        }
        public string GetExceptions()
        {
            return "test";
        }
        class ExceptionContext
        {
            public Exception Ex { get; set; }
            public string Title { get; set; }
            public string Detail { get; set; }
        }
    }
}

