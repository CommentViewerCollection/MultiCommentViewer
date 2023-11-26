using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace MixchSitePlugin
{
    class Item
    {
        private static readonly Dictionary<int, string> m;

        static Item()
        {
            m = new Dictionary<int, string>() { };

            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"settings\mixch_item.txt");
            using (var reader = new System.IO.StreamReader(path, Encoding.GetEncoding("UTF-8")))
            {
                while (reader.Peek() >= 0)
                {
                    var line = reader.ReadLine();
                    string[] cols = line.Split('=');
                    if (cols.Length == 2)
                    {
                        try
                        {
                            var id = int.Parse(cols[0]);
                            m[id] = cols[1];
                            Debug.WriteLine("ReadItemSuccess: " + id + "=" + cols[1]);
                        }
                        catch (FormatException e)
                        {
                            Debug.WriteLine("ReadItemError: " + e);
                        }
                    }
                    else
                    {
                        Debug.WriteLine("ReadItemIgnore: " + line);
                    }
                }
            }
        }

        public static string NameByResourceId(int id)
        {
            return m.ContainsKey(id) ? m[id] : "";
        }
    }
}
