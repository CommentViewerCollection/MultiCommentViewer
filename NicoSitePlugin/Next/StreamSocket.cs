using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NicoSitePlugin.Next
{
    class StreamSocket : IDisposable, IStreamSocket
    {
        public event EventHandler Connected;
        public event EventHandler<List<string>> Received;
        private NetworkStream _stream;
        public async Task ConnectAsync()
        {
            if (_client != null)
            {
                throw new InvalidOperationException();
            }
            _client = new TcpClient();

            await _client.ConnectAsync(_host, _port);
            _stream = _client.GetStream();
            Connected?.Invoke(this, EventArgs.Empty);
        }
        public async Task ReceiveAsync()
        {
            if (_client == null)
            {
                throw new InvalidOperationException();
            }
            while (true)
            {
                int n = 0;
                try
                {
                    n = await _stream.ReadAsync(_receiveBuffer, 0, _receiveBuffer.Length);
                }
                catch (ObjectDisposedException ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
                if (n <= 0)
                    break;
                _buffer.Add(_receiveBuffer, 0, n);
            }
        }
        public async Task SendAsync(string s)
        {
            if (_client == null)
            {
                throw new InvalidOperationException();
            }
            var bytes = System.Text.Encoding.UTF8.GetBytes(s);
            await _stream.WriteAsync(bytes, 0, bytes.Length);
        }
        public void Disconnect()
        {
            if (_client == null || _stream == null)
            {
                throw new InvalidOperationException();
            }
            _stream.Close();
            _stream = null;
        }
        private TcpClient _client;
        private readonly string _host;
        private readonly int _port;
        private byte[] _receiveBuffer;
        private readonly ISplitBuffer _buffer;
        public StreamSocket(string host, int port, int bufferSize, ISplitBuffer buffer)
        {
            _host = host;
            _port = port;

            _receiveBuffer = new byte[bufferSize];
            _buffer = buffer;
            _buffer.Added += _buffer_Added;
        }

        private void _buffer_Added(object sender, List<string> e)
        {
            Received?.Invoke(this, e);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _receiveBuffer = null;
                }
                if (_client != null)
                {
                    _client.Dispose();
                }

                disposedValue = true;
            }
        }

        ~StreamSocket()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
