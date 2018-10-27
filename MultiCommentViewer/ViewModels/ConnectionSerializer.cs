using System.Text.RegularExpressions;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiCommentViewer
{
    class ConnectionSerializer
    {
        private ConnectionSerializer() { }
        public ConnectionSerializer(string name, string siteName, string url, string browserName)
        {
            //Guidは保存しないようにする。保存ファイルを弄ってGuidを重複させることも可能になってしまうのを防ぐ
            Name = name;
            SiteName = siteName;
            Url = url;
            BrowserName = browserName;
        }
        public string Serialize()
        {
            var list = new List<string>();
            var props = typeof(ConnectionSerializer).GetProperties();
            foreach(var prop in props)
            {
                var k = prop.Name;
                var v = prop.GetValue(this) as string;
                list.Add($"{k}={v}");
            }
            return list.Aggregate((a, b) => a + "\t" + b);
        }
        public static ConnectionSerializer Deserialize(string line)
        {
            var ret = new ConnectionSerializer();
            var kvs = line.Split(new char[] { '\t' },StringSplitOptions.RemoveEmptyEntries);
            var props = typeof(ConnectionSerializer).GetProperties();
            foreach (var kv in kvs)
            {
                var match = Regex.Match(kv, "(^[^=]+)=(.+)$");
                if (match.Success)
                {
                    var k = match.Groups[1].Value;
                    var v = match.Groups[2].Value;
                    foreach(var prop in props)
                    {
                        if(prop.Name == k)
                        {
                            prop.SetValue(ret, v);
                            continue;
                        }
                    }
                }
            }
            return ret;
        }

        public string Name { get; private set; }
        public string SiteName { get; private set; }
        public string Url { get; private set; }
        public string BrowserName { get; private set; }
    }
}

