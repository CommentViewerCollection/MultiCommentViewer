using System;

namespace Common.AutoUpdate
{
    [Obsolete]
    public class LatestVersionInfo
    {
        public Version Version { get; }
        public string Url { get; }
        public LatestVersionInfo(string version, string url)
        {
            try
            {
                this.Version = new Version(version);
            }
            catch
            {
                this.Version = new Version(0, 0, 0, 0);
            }
            this.Url = url;
        }
    }
}
