using System;
using SitePlugin;
using Common;
using System.Xml.Serialization;
using System.Text;
using Newtonsoft.Json;
namespace Common
{
    public class LoggerTest : ILogger
    {
        System.Collections.Concurrent.BlockingCollection<ExceptionContext> _exCollection = new System.Collections.Concurrent.BlockingCollection<ExceptionContext>();
        public void LogException(Exception ex, string title = "", string detail = "")
        {
            var error = new Error(ex);
            _exCollection.Add(new ExceptionContext { Ex = error, Title = title, Detail = detail });
        }
        public string GetExceptions()
        {
            var sb = new StringBuilder();
            foreach(var item in _exCollection)
            {
                var s = JsonConvert.SerializeObject(item);
                sb.Append(s);
                sb.Append(",\r\n");
            }
            return sb.ToString();
        }
        [Serializable]
        public class ExceptionContext
        {
            public Error Ex { get; set; }
            public string Title { get; set; }
            public string Detail { get; set; }
        }
        [Serializable]
        public class Error
        {
            public string Message { get; }
            public string StackTrace { get; }
            public string Timestamp { get; }
            public Error[] InnerError { get; }
            public Error()
            {
                Timestamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            }
            public Error(Exception ex):this()
            {
                Message = ex.Message;
                StackTrace = ex.StackTrace;

                if (ex.InnerException != null)
                {
                    InnerError = new Error[1];
                    InnerError[0] = new Error(ex.InnerException);
                }
            }
            public Error(AggregateException ex) : this()
            {
                Message = ex.Message;
                StackTrace = ex.StackTrace;
                var innerCount = ex.InnerExceptions.Count;
                InnerError = new Error[innerCount];
                for(int i = 0; i < innerCount; i++)
                {
                    InnerError[i] = new Error(ex.InnerExceptions[i]);
                }
            }
        }
    }
}

