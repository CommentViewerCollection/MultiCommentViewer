using System.Threading.Tasks;
using System.Net;
using System.Text;

namespace YouTubeLiveSitePlugin.Test2
{
    internal class YouTubeLiveServer : IYouTubeLibeServer
    {
        public async Task<string> GetAsync(string url)
        {
            var wc = new WebClient();
            wc.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:59.0) Gecko/20100101 Firefox/59.0";
            wc.Headers["origin"] = "https://www.youtube.com";
            var bytes = await wc.DownloadDataTaskAsync(url);
            return Encoding.UTF8.GetString(bytes);
        }

        public async Task<string> GetEnAsync(string url)
        {
            var wc = new WebClient();
            wc.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:59.0) Gecko/20100101 Firefox/59.0";
            wc.Headers["origin"] = "https://www.youtube.com";
            wc.Headers[HttpRequestHeader.AcceptLanguage] = "en-US,en;q=0.5";
            var bytes = await wc.DownloadDataTaskAsync(url);
            return Encoding.UTF8.GetString(bytes);
        }

        public async Task<string> PostAsync(string url, string data, CookieContainer cc)
        {
            var wc = new MyWebClient(cc);
            wc.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:59.0) Gecko/20100101 Firefox/59.0";
            wc.Headers["origin"] = "https://www.youtube.com";
            wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            wc.Headers[HttpRequestHeader.Accept] = "*/*";
            var payload = Encoding.UTF8.GetBytes(data);
            var bytes = await wc.UploadDataTaskAsync(url, payload);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
