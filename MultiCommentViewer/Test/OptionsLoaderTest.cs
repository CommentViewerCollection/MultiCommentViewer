using System.Windows;
using System.Windows.Media;
using SitePlugin;
using System.IO;
using System.Text;
using System.Xml;
using System;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

namespace MultiCommentViewer.Test
{
    public interface IIo
    {
        string ReadFile(string path);
        Task WriteFileAsync(string path, string s);
    }
    public class IOTest : IIo
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
                catch(FileNotFoundException)
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
        //public async Task<string> ReadFile(string path)
        //{
        //    await Task.Yield();
        //    var totalWaitTime = 0;
        //    string s = null;
        //    while (totalWaitTime < 5000)
        //    {
        //        try
        //        {
        //            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
        //            using (var sr = new StreamReader(fs))
        //            {
        //                s = sr.ReadToEnd();
        //            }
        //            break;
        //        }
        //        catch (IOException ex)
        //        {
        //            Debug.WriteLine(ex.Message);
        //            var waitTime = GetRandomLong(10, 500);
        //            Debug.WriteLine($"読み込みに失敗したため{waitTime}ミリ秒待ちます");
        //            await Task.Delay(waitTime);
        //            totalWaitTime += waitTime;
        //        }
        //    }
        //    return s;
        //}

        public async Task WriteFileAsync(string path, string s)
        {
            await Task.Yield();
            var bytes = Encoding.UTF8.GetBytes(s);

            var totalWaitTime = 0;
            while (totalWaitTime < 5000)
            {
                try
                {
                    using (var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
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
    public class OptionsLoaderTest:IOptionsSerializer
    {
        public IOptions DeserializeOptions(string optionsStr)
        {
            OptionsTest options = null;
            if (!string.IsNullOrEmpty(optionsStr))
            {
                try
                {
                    var ds = new DataContractSerializer(typeof(OptionsTest));
                    using (var sr = new StringReader(optionsStr))
                    {
                        var stream = XmlTextReader.Create(sr);
                        options = ds.ReadObject(stream) as OptionsTest;
                    }
                }
                catch (Exception ex)
                {

                }
            }
            if (options == null)
            {
                options = new OptionsTest();
            }
            return options;
        }
        public string SerializeOptions(IOptions options)
        {
            
            //BOMが付かないUTF-8で、書き込むファイルを開く
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Encoding = new System.Text.UTF8Encoding(false),
                Indent = true
            };
            var ms = new MemoryStream();
            var serializer = new DataContractSerializer(typeof(OptionsTest));
            using (XmlWriter xw = XmlWriter.Create(ms, settings))
            {
                serializer.WriteObject(xw, options);
            }
            
            var bytes = ms.GetBuffer();
            var s=Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            return s;
        }
    }

}
