using GameLogic;
using System.Net;
using System.Net.Sockets;

namespace TCPServer
{
    internal class XServer : IDisposable
    {
        internal readonly List<ConnectedClient> Clients;
        private int _currentId;

        private bool _listening;
        private bool _stopListening;
        private readonly Socket _socket;

        public XServer()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Clients = new List<ConnectedClient>();
        }

        public void Start()
        {
            if (_listening)
            {
                throw new Exception("Server is already listening incoming requests.");
            }

            _socket.Bind(new IPEndPoint(IPAddress.Any, 4910));
            _socket.Listen(10);

            _listening = true;
        }

        public void Stop()
        {
            if (!_listening)
            {
                throw new Exception("Server is already not listening incoming requests.");
            }

            _stopListening = true;
                
            _listening = false;
        }

        public void AcceptClients()
        {
            while (true)
            {
                if (_stopListening) return;

                Socket client;
                try { client = _socket.Accept(); }
                catch { return; }

                Console.WriteLine($"[!] Accepted client from {client.RemoteEndPoint as IPEndPoint}");
                var c = new ConnectedClient(client);
                Clients.Add(c);
            }
        }

        public void Dispose()
        {
            Stop();
            _socket.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
