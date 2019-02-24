using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineLiveSitePluginTests
{
    static class TestHelper
    {
        public static string GetSampleData(string filename)
        {
            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"SampleData\" + filename);
            string sample = "";

            using (var sr = new System.IO.StreamReader(path))
            {
                sample = sr.ReadToEnd();
            }
            return sample;
        }
    }
}
