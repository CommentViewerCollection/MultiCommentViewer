using System;
using SitePlugin;
using Common;
using System.Xml.Serialization;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Reflection;
using System.Net;
using System.Diagnostics;

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
            public string Name { get; private set; }
            public string Message { get; private set; }
            public string StackTrace { get; private set; }
            public string Timestamp { get; private set; }
            public Error[] InnerError { get; private set; }
            public Dictionary<string, string> Properties { get; private set; } = new Dictionary<string, string>();
            public Error()
            {
                Timestamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            }
            public Error(Exception ex):this()
            {
                Name = ex.GetType().FullName;
                Message = ex.Message;
                StackTrace = ex.StackTrace;
                SetProperties(ex);

                if (ex.InnerException != null)
                {
                    InnerError = new Error[1];
                    InnerError[0] = new Error(ex.InnerException);
                }
            }
            public Error(WebException ex) : this((Exception)ex)
            {
                Properties.Add(nameof(ex.Status), ex.Status.ToString());
                if (ex.Response is HttpWebResponse http)
                {
                    Properties.Add(nameof(http.StatusCode), http.StatusCode.ToString());
                    using (var sr = new System.IO.StreamReader(http.GetResponseStream()))
                    {
                        var s = sr.ReadToEnd();
                        Properties.Add("Response", s.Replace("\"", "\\\""));
                    }
                }
            }
            private void SetProperties(Exception ex)
            {
                try
                {
                    var properties = ex.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    foreach (var property in properties)
                    {
                        if (property.PropertyType == typeof(string))
                        {
                            var get = property.GetGetMethod();
                            var name = property.Name;
                            var s = (string)get.Invoke(ex, null);
                            Properties.Add(name, s);
                        }
                    }
                }catch(Exception ex1)
                {
                    Debug.WriteLine(ex1.Message);
                }
            }
            public Error(AggregateException ex) : this()
            {
                Name = ex.GetType().FullName;
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

