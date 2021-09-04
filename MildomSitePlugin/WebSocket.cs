using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using System.Text;
using System.IO;

namespace MildomSitePlugin
{
    public class SytemNetWebSockets : IWebSocket
    {
        public event EventHandler<string> Received;
        public event EventHandler Opened;

        public void Disconnect()
        {
            _cts?.Cancel();
        }
        ClientWebSocket _ws;
        private readonly string _url;
        private CancellationTokenSource _cts;
        public async Task ReceiveAsync()
        {
            if (_cts != null) return;
            _cts = new CancellationTokenSource();
            _ws = new ClientWebSocket();
            await _ws.ConnectAsync(new Uri(_url), _cts.Token);
            Opened?.Invoke(this, EventArgs.Empty);

            var buf = new byte[4096];
            var arr = new ArraySegment<byte>(buf);
            int count = 0;
            MemoryStream ms=new MemoryStream();
            while (true)
            {
                var result = await _ws.ReceiveAsync(arr, _cts.Token);
                if(result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }
                else if(result.MessageType == WebSocketMessageType.Text)
                {
                    if (result.EndOfMessage)
                    {
                        string s;
                        if (count == 0)
                        {
                            s = Encoding.UTF8.GetString(buf, 0, result.Count);
                        }
                        else
                        {
                            ms.Write(buf, 0, result.Count);
                            var k = ms.ToArray();
                            s = Encoding.UTF8.GetString(k, 0, k.Length);
                            ms = new MemoryStream();
                            count = 0;
                        }
                        Received?.Invoke(this, s);
                      
                    }
                    else
                    {
                        ms.Write(buf, 0, result.Count);
                        count++;
                    }
                }
            }

            _cts = null;
        }

        public async Task SendAsync(string s)
        {
            if (_ws == null || _cts == null) return;
            var buf = Encoding.UTF8.GetBytes(s);
            await _ws.SendAsync(new ArraySegment<byte>(buf), WebSocketMessageType.Text, true, _cts.Token);
        }
        public SytemNetWebSockets(string url)
        {
            _url = url;
        }
    }
}
