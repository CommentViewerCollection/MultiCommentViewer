using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace BouyomiPlugin
{
    class TcpTalker : ITalker
    {
        private bool _disposedValue;

        const Int16 COMMAND = 0x0001;
        const byte CHARCODE_UTF8 = 0;

        public string IpAddr { get; set; }
        public int Port { get; set; }

        public void TalkText(string text)
        {
            //-1を指定すると棒読みちゃん画面上の設定を使用する
            TalkText(text, -1, -1, -1, FNF.Utility.VoiceType.Default);
        }
        public void TalkText(string text, Int16 voiceSpeed, Int16 voiceTone, Int16 voiceVolume, FNF.Utility.VoiceType voiceTypeIndex)
        {
            var ipAddr = "127.0.0.1";
            var port = 50001;
            var messageBytes = Encoding.UTF8.GetBytes(text);

            try
            {
                using (var tc = new TcpClient(ipAddr, port))
                using (NetworkStream ns = tc.GetStream())
                using (var bw = new BinaryWriter(ns))
                {
                    bw.Write(COMMAND); //コマンド（ 0:メッセージ読み上げ）
                    bw.Write((Int16)voiceSpeed);   //速度    （-1:棒読みちゃん画面上の設定）
                    bw.Write((Int16)voiceTone);    //音程    （-1:棒読みちゃん画面上の設定）
                    bw.Write((Int16)voiceVolume);  //音量    （-1:棒読みちゃん画面上の設定）
                    bw.Write((Int16)voiceTypeIndex);   //声質    （ 0:棒読みちゃん画面上の設定、1:女性1、2:女性2、3:男性1、4:男性2、5:中性、6:ロボット、7:機械1、8:機械2、10001～:SAPI5）
                    bw.Write(CHARCODE_UTF8);    //文字列のbyte配列の文字コード(0:UTF-8, 1:Unicode, 2:Shift-JIS)
                    bw.Write((Int32)messageBytes.Length);  //文字列のbyte配列の長さ
                    bw.Write(messageBytes); //文字列のbyte配列
                }
            }
            catch (Exception ex)
            {
                throw new TalkException("", ex);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~TcpTalker()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        public TcpTalker(string ipAddr, int port)
        {
            IpAddr = ipAddr;
            Port = port;
        }
    }
}
