using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NicoSitePlugin.Next
{
    class SplitBuffer : ISplitBuffer
    {
        public event EventHandler<List<string>> Added;
        private readonly StringBuilder _sb = new StringBuilder();
        MemoryStream _ms = new MemoryStream();
        private readonly object _lockObj = new object();
        public void Add(byte[] data, int start, int length)
        {
            //複数のスレッドから同時にアクセスがあるとArgumentOutOfRangeExceptionを投げるっぽい
            lock (_lockObj)
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
                for (int i = 0; i < arr.Length; i++)
                {
                    if (i + _splitter.Length > arr.Length)
                    {
                        break;
                    }
                    var sub = arr.Skip(i).Take(_splitter.Length).ToArray();
                    if (sub.SequenceEqual(_splitter))
                    {
                        list.Add(_enc.GetString(arr, strStart, i - strStart));
                        strStart = i + _splitter.Length;
                    }
                }

                //マッチしなかった部分だけ残す
                _ms.SetLength(0);
                _ms.Write(arr, strStart, arr.Length - strStart);

                if (list.Count > 0)
                {
                    Added?.Invoke(this, list);
                }
            }
        }
        private readonly byte[] _buf;
        private readonly byte[] _splitter;
        Encoding _enc;
        public SplitBuffer(string splitter)
        {
            _enc = Encoding.UTF8;
            _splitter = _enc.GetBytes(splitter);
            _buf = new byte[_splitter.Length];
        }
    }
}
