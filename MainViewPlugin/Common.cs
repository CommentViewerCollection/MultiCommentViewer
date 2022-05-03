////using Mcv.Core;
//using Mcv.PluginV2;
//using Newtonsoft.Json;
////using System.Text.Json;
////using Plugin;
//using System;
//using System.Collections.Generic;
////using YtMessage = ryu_s.YouTubeLive.Message;
using Mcv.PluginV2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common
{
    //    public interface IMessagePart { }
    //    public interface IMessageText : IMessagePart
    //    {
    //        string Text { get; }        
    //    }
    //    public static class MessageTextExtensions
    //    {
    //        static IMessageText Parse(this string text)
    //        {
    //            return new MessageTextImpl(text);
    //        }
    //    }

    class MessageTextImpl : IMessageText
    {
        public string Text { get; }
        public MessageTextImpl(string text)
        {
            Text = text;
        }
    }
    //    public interface IMessageImage : IMessagePart
    //    {
    //        int? X { get; }
    //        int? Y { get; }
    //        int? Width { get; }
    //        int? Height { get; }
    //        string Url { get; }

    //        string Alt { get; }
    //    }
    //    class MessageImage : IMessageImage
    //    {
    //        public int? X { get; }
    //        public int? Y { get; }
    //        public int? Width { get; }
    //        public int? Height { get; }
    //        public string Url { get; }
    //        public string Alt { get; }
    //        public MessageImage(string url, string alt, int? width, int? height)
    //        {
    //            Url = url;
    //            Alt = alt;
    //            Width = width;
    //            Height = height;
    //        }
    //    }
    //    /// <summary>
    //    /// 指定された画像の一部を描画する用
    //    /// </summary>
    //    public interface IMessageImagePortion : IMessagePart
    //    {
    //        int SrcX { get; }
    //        int SrcY { get; }
    //        int SrcWidth { get; }
    //        int SrcHeight { get; }
    //        /// <summary>
    //        /// 表示時の幅
    //        /// </summary>
    //        int Width { get; }
    //        /// <summary>
    //        /// 表示時の高さ
    //        /// </summary>
    //        int Height { get; }
    //        System.Drawing.Image Image { get; }
    //        string Alt { get; }
    //    }
    //    public interface IMessageSvg : IMessagePart
    //    {
    //        string Data { get; }
    //    }
    //    public class MessageSvg : IMessageSvg
    //    {
    //        public string Data { get; }
    //        public MessageSvg(string data)
    //        {
    //            Data = data;
    //        }
    //    }
    //    public interface IMessageRemoteSvg : IMessagePart
    //    {
    //        int? Width { get; }
    //        int? Height { get; }
    //        string Url { get; }
    //        string Alt { get; }
    //    }
    //    public class RemoteSvg : IMessageRemoteSvg
    //    {
    //        public int? Width { get; }
    //        public int? Height { get; }
    //        public string Url { get; }
    //        public string Alt { get; }
    //        public RemoteSvg(string url, string alt, int? width, int? height)
    //        {
    //            Url = url;
    //            Alt = alt;
    //            Width = width;
    //            Height = height;
    //        }
    //    }
    //    public interface IMessageLink : IMessageText
    //    {
    //        string Url { get; }
    //    }
    //    public static class Extensions
    //    {
    //        public static string ToText(this IEnumerable<IMessagePart> items)
    //        {
    //            throw new NotImplementedException();
    //        }
    //        public static string ConnStToString(this IConnectionStatus connSt)
    //        {
    //            var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(connSt);
    //            //TODO:.NET6以降で以下のように実装したい
    //            //var jsonString = JsonSerializer.Serialize(connSt);
    //            return jsonString;
    //        }
    //        public static string ConnStDiffToString(this IConnectionStatusDiff connSt)
    //        {
    //            var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(connSt);
    //            //TODO:.NET6以降で以下のように実装したい
    //            //var jsonString = JsonSerializer.Serialize(connSt);
    //            return jsonString;
    //        }
    //    }
    //    //static class Factory
    //    //{
    //    //    public static Common.IMessagePart? Parse(YtMessage.IBadge? badge)
    //    //    {
    //    //        if (badge is YtMessage.RemoteIconPart remote)
    //    //        {
    //    //            return new MessageImage(remote.Url, remote.Alt, remote.Width, remote.Height);
    //    //        }
    //    //        else if (badge is YtMessage.SvgIconPart svg)
    //    //        {
    //    //            return new MessageSvg(svg.Data);
    //    //        }
    //    //        else
    //    //        {
    //    //            return null;
    //    //        }
    //    //    }
    //    //}
    public static class MessagePartFactory
    {
        public static IMessageText CreateMessageText(string text)
        {
            return new MessageTextImpl(text);
        }

        public static IEnumerable<IMessagePart> CreateMessageItems(string text)
        {
            return new List<IMessagePart>
            {
                CreateMessageText(text)
            };
        }
    }
    public class IOTest
    {
        public string ReadFile(string path)
        {
            var totalWaitTime = 0;
            string s = null;
            while (totalWaitTime < 5000)
            {
                try
                {
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                    using (var sr = new StreamReader(fs))
                    {
                        s = sr.ReadToEnd();
                    }
                    break;
                }
                catch (DirectoryNotFoundException)
                {
                    throw;
                }
                catch (FileNotFoundException)
                {
                    break;
                }
                catch (IOException ex)
                {
                    Debug.WriteLine(ex.Message);
                    var waitTime = GetRandomLong(10, 500);
                    Debug.WriteLine($"読み込みに失敗したため{waitTime}ミリ秒待ちます");
                    Thread.Sleep(waitTime);
                    totalWaitTime += waitTime;
                }
            }
            return s;
        }
        public async Task<string> ReadFileAsync(string path)
        {
            var totalWaitTime = 0;
            string s = null;
            while (totalWaitTime < 5000)
            {
                try
                {
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                    using (var sr = new StreamReader(fs))
                    {
                        s = await sr.ReadToEndAsync();
                    }
                    break;
                }
                catch (FileNotFoundException)
                {
                    break;
                }
                catch (IOException ex)
                {
                    Debug.WriteLine(ex.Message);
                    var waitTime = GetRandomLong(10, 500);
                    Debug.WriteLine($"読み込みに失敗したため{waitTime}ミリ秒待ちます");
                    await Task.Delay(waitTime);
                    totalWaitTime += waitTime;
                }
            }
            return s;
        }
        public void WriteFile(string path, string s)
        {
            var bytes = Encoding.UTF8.GetBytes(s);

            var totalWaitTime = 0;
            while (totalWaitTime < 5000)
            {
                try
                {
                    using (var fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                    {
                        fs.Write(bytes, 0, bytes.Length);
                    }
                    break;
                }
                catch (IOException ex)
                {
                    Debug.WriteLine(ex.Message);
                    var waitTime = GetRandomLong(10, 500);
                    Debug.WriteLine($"書き込みに失敗したため{waitTime}ミリ秒待ちます");
                    Thread.Sleep(waitTime);
                    totalWaitTime += waitTime;
                }
            }
        }
        public async Task WriteFileAsync(string path, string s)
        {
            await Task.Yield();
            var bytes = Encoding.UTF8.GetBytes(s);

            var totalWaitTime = 0;
            while (totalWaitTime < 5000)
            {
                try
                {
                    using (var fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                    {
                        fs.Write(bytes, 0, bytes.Length);
                    }
                    break;
                }
                catch (IOException ex)
                {
                    Debug.WriteLine(ex.Message);
                    var waitTime = GetRandomLong(10, 500);
                    Debug.WriteLine($"書き込みに失敗したため{waitTime}ミリ秒待ちます");
                    await Task.Delay(waitTime);
                    totalWaitTime += waitTime;
                }
            }
        }
        protected virtual int GetRandomLong(int min, int max)
        {
            byte[] bs = new byte[sizeof(int)];
            System.Security.Cryptography.RNGCryptoServiceProvider rng =
                new System.Security.Cryptography.RNGCryptoServiceProvider();
            rng.GetBytes(bs);

            int i = System.BitConverter.ToInt32(bs, 0);
            return Math.Abs(i % max) + min;
        }
    }
}