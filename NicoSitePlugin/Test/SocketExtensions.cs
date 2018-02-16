using System.Threading.Tasks;
using System.Net.Sockets;
namespace NicoSitePlugin.Old
{
    /// <summary>
    /// 
    /// </summary>
    public static class SocketExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static Task ConnectTaskAsync(this Socket socket, string host, int port)
        {
            return Task.Factory.FromAsync(socket.BeginConnect(host, port, null, null), socket.EndConnect);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static Task<int> ReceiveTaskAsync(this Socket socket, byte[] buffer, int offset, int size)
        {
            return Task.Factory.FromAsync(socket.BeginReceive(buffer, offset, size, SocketFlags.None, null, socket),
                socket.EndReceive);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static Task SendTaskAsync(this Socket socket, byte[] buffer)
        {
            return Task.Factory.FromAsync<int>(
                  socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, null, socket),
                  socket.EndSend);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="reuseSocket"></param>
        /// <returns></returns>
        public static Task DisconnectTaskAsync(this Socket socket, bool reuseSocket)
        {
            return Task.Factory.FromAsync(socket.BeginDisconnect(reuseSocket, null, null), socket.EndDisconnect);
        }
    }
}
