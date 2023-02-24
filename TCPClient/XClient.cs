using System.Net;
using System.Net.Sockets;

namespace TCPClient
{
    public class XClient : IDisposable
    {
        public Action<byte[]> OnPacketReceive { get; set; }
        public bool Connected => _socket?.Connected ?? false;

        private readonly Queue<byte[]> _packetSendingQueue = new Queue<byte[]>();

        private Socket _socket;
        private IPEndPoint _serverEndPoint;
        
        public void Connect(string ip, int port)
        {
            Connect(new IPEndPoint(IPAddress.Parse(ip), port));
        }

        private void Connect(IPEndPoint server)
        {
            _serverEndPoint = server;

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(_serverEndPoint);

            Task.Run(ReceivePacketsAsync);
            Task.Run(SendPacketsAsync);
        }

        public void QueuePacketSend(byte[] packet)
        {
            if (packet.Length > 256)
            {
                throw new Exception("Max packet size is 256 bytes.");
            }

            _packetSendingQueue.Enqueue(packet);
        }

        private async void ReceivePacketsAsync()
        {
            while (_socket.Connected)
            {
                var buff = new byte[256];
                try { await _socket.ReceiveAsync(buff); }
                catch { return; };

                buff = buff.TakeWhile((b, i) =>
                {
                    if (b != 0xFF) return true;
                    return buff[i + 1] != 0;
                }).Concat(new byte[] {0xFF, 0}).ToArray();

                OnPacketReceive?.Invoke(buff);
            }
        }

        private async void SendPacketsAsync()
        {
            while (_socket.Connected)
            {
                if (_packetSendingQueue.Count == 0)
                {
                    Thread.Sleep(50);
                    continue;
                }

                var packet = _packetSendingQueue.Dequeue();
                await _socket.SendAsync(packet);

                Thread.Sleep(50);
            }
        }
        
        public void Disconnect() => Dispose();

        public void Dispose()
        {
            _socket.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
