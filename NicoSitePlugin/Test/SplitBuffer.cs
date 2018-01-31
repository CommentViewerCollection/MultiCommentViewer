using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
namespace NicoSitePlugin.Test
{
    public interface ISplitBuffer
    {
        event EventHandler<List<string>> Added;
        void Add(string s);
    }
    public interface ISplitBuffer2
    {
        event EventHandler<List<string>> Added;
        void Add(byte[] data, int start, int length);
    }
    /// <summary>
    /// データをsplitString毎に区切って格納
    /// </summary>
    /// <remarks>
    /// サーバからクライアントに対してデータが分割されて送られてくることを想定。
    /// "abc\r\ndef\r\nghi\r\n"という文字列が最終的に送られてくるとする。区切り文字は"\r\n"
    /// まず"ab"が送られてきた。区切り文字が無いためバッファに追加するのみ。
    /// "c\r\nd"が送られてきた。まずはバッファに追加する。"abc\r\nd"
    /// 区切り文字が含まれているため、イベントが発生する。イベントのリスナは"abc"を受け取る。
    /// バッファには"d"が残る。
    /// </remarks>
    public class SplitBuffer :ISplitBuffer
    {
        public event EventHandler<List<string>> Added;
        private readonly StringBuilder sb=new StringBuilder();
        private readonly object lockObj = new object();
        public void Add(string s)
        {
            //複数のスレッドから同時にアクセスがあるとArgumentOutOfRangeExceptionを投げるっぽい
            lock (lockObj)
            {
                sb.Append(s);

                var arr = sb.ToString().Split(new[] { _splitter }, StringSplitOptions.None);
                if (arr.Length > 1)
                {
                    if (Added != null)
                    {
                        //最後の要素を取り除くもっと綺麗な方法は無いんだろうか。                   
                        var list = new List<string>(arr);

                        if (list.Count > 0)
                            list.RemoveAt(list.Count - 1);
                        Added?.Invoke(this, list);
                    }
                    sb.Clear();
                    sb.Append(arr[arr.Length - 1]);
                }
            }
        }
        private readonly string _splitter;
        public SplitBuffer(string splitter)
        {
            _splitter = splitter;
        }
    }
    public class SplitBuffer2 : ISplitBuffer2
    {
        public event EventHandler<List<string>> Added;
        private readonly StringBuilder sb = new StringBuilder();
        MemoryStream _ms = new MemoryStream();
        private readonly object lockObj = new object();
        public void Add(byte[] data, int start, int length)
        {
            //複数のスレッドから同時にアクセスがあるとArgumentOutOfRangeExceptionを投げるっぽい
            lock (lockObj)
            {
                //パフォーマンスを考えると追加分の中を検索すればいい。
                //とりあえず動くものを作るために効率は無視。

                //探しているものが[2,1,1]で、既に[0,2]があったとする。追加分の先頭が[1,1]だったらmatchするように考慮しないといけない。
                //要は、取得済みの配列の後ろからsplitter.Lnegth-1の位置から検索を開始すればいい。

                _ms.Write(data, start, length);

                var list = new List<string>();
                var arr = _ms.ToArray();
                var len = arr.Length;
                var strStart = 0;
                for(int i = 0; i < arr.Length; i++)
                {
                    if(i + _splitter.Length > arr.Length)
                    {
                        break;
                    }
                    var sub = arr.Skip(i).Take(_splitter.Length).ToArray();
                    if (sub.SequenceEqual(_splitter))
                    {
                        list.Add(_enc.GetString(arr, strStart, i- strStart));
                        strStart = i + _splitter.Length;
                    }
                }

                //マッチしなかった部分だけ残す
                _ms.SetLength(0);
                _ms.Write(arr, strStart, arr.Length - strStart);

                if(list.Count > 0)
                {
                    Added?.Invoke(this, list);
                }
                //var readByteCount = _ms.Read(_buf, 0, _buf.Length);
                //if(readByteCount != _splitter.Length)
                //{
                //    //無い
                //    return;
                //}
                //if (_buf.Equals(_splitter))
                //{

                //}

                //if(_ms.)
                //var arr = sb.ToString().Split(new[] { _splitter }, StringSplitOptions.None);
                //if (arr.Length > 1)
                //{
                //    if (Added != null)
                //    {
                //        //最後の要素を取り除くもっと綺麗な方法は無いんだろうか。                   
                //        var list = new List<string>(arr);

                //        if (list.Count > 0)
                //            list.RemoveAt(list.Count - 1);
                //        Added?.Invoke(this, list);
                //    }
                //    sb.Clear();
                //    sb.Append(arr[arr.Length - 1]);
                //}
            }
        }
        private readonly byte[] _buf;
        private readonly byte[] _splitter;
        Encoding _enc;
        public SplitBuffer2(string splitter)
        {
            _enc = Encoding.UTF8;
            _splitter = _enc.GetBytes(splitter);
            _buf = new byte[_splitter.Length];
        }
    }
}
