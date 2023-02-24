using GameLogic;
using System.Net.Sockets;
using XProtocol;
using XProtocol.Serializer;
using XProtocol.XPackets;

namespace TCPServer
{
    internal class ConnectedClient : IDisposable
    {
        private Socket Client { get; }

        private readonly Queue<byte[]> _packetSendingQueue = new();

        public ConnectedClient(Socket client)
        {
            Client = client;

            Task.Run(ProcessIncomingPacketsAsync);
            Task.Run(SendPacketsAsync);
        }

        private async void ProcessIncomingPacketsAsync()
        {
            while (Client.Connected) // Слушаем пакеты, пока клиент не отключится.
            {
                var buff = new byte[256]; // Максимальный размер пакета - 256 байт.
                try { await Client.ReceiveAsync(buff); }
                catch { return; }

                buff = buff.TakeWhile((b, i) =>
                {
                    if (b != 0xFF) return true;
                    return buff[i + 1] != 0;
                }).Concat(new byte[] {0xFF, 0}).ToArray();

                var parsed = XPacket.Parse(buff);

                if (parsed != null)
                {
                    ProcessIncomingPacket(parsed);
                }
            }
        }

        private void ProcessIncomingPacket(XPacket packet)
        {
            var type = XPacketTypeManager.GetTypeFromPacket(packet);

            switch (type)
            {
                case XPacketType.Handshake:
                    ProcessHandshake();
                    break;
                case XPacketType.Pause:
                    ProcessPause(packet);
                    break;
                case XPacketType.PauseEnded:
                    ProcessPauseEnded(packet);
                    break;
                case XPacketType.EndGame:
                    ProcessEndGame(packet);
                    break;
                case XPacketType.Unknown:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ProcessEndGame(XPacket packet)
        {
            var endGame = XPacketConverter.Deserialize<XPacketEndGame>(packet);
            if (_server.Clients.Count < 2) return;
            var winner = _server.Clients.First(c => c.Player.Id != endGame.PlayerId);
            winner.QueuePacketSend(XPacketConverter
                .Serialize(XPacketType.Winner, new XPacketWinner { IdWinner = winner.Player.Id }).ToPacket());
            _server.Clients.Remove(_server.Clients.First(c => c != winner));
            Dispose();
        }

        private void ProcessHandshake()
        {
            var successfulRegistration = new XPacketSuccessfulRegistration
            {
                Id = Player.Id
            };

            QueuePacketSend(XPacketConverter
                .Serialize(XPacketType.SuccessfulRegistration, successfulRegistration).ToPacket());
            
            if (_server.Clients.Count == 2)
                foreach (var client in _server.Clients)
                    client.QueuePacketSend(XPacketConverter
                .Serialize(XPacketType.StartGame, new XPacketStartGame()).ToPacket());
            Console.WriteLine($"Received Handshake from {Player.Id}");
        }

        private void ProcessPause(XPacket packet)
        {
            Console.WriteLine($"Received pause from {Player.Id}");
            var pause = XPacketConverter.Deserialize<XPacketPause>(packet);
            var opponent = _server.Clients.FirstOrDefault(c => c.Player.Id != Player.Id);
            if (opponent == null) throw new NullReferenceException("Opponent not found");

            opponent.QueuePacketSend(XPacketConverter.Serialize(XPacketType.Pause, pause).ToPacket());
        }

        private void ProcessPauseEnded(XPacket packet)
        {
            var pauseEnded = XPacketConverter.Deserialize<XPacketPauseEnded>(packet);
            Console.WriteLine($"Received pause ended from {Player.Id}");

            var opponent = _server.Clients.FirstOrDefault(c => c.Player.Id != Player.Id);
            if (opponent == null) throw new NullReferenceException("Opponent not found");

            opponent.QueuePacketSend(XPacketConverter.Serialize(XPacketType.PauseEnded, pauseEnded).ToPacket());
        }

        private void EndGameForAllPlayers()
        {
            foreach (var client in _server.Clients)
                client.QueuePacketSend(XPacketConverter
                    .Serialize(XPacketType.Winner, new XPacketWinner { IdWinner = _server.Gp.GetWinnerId() })
                    .ToPacket());
            Dispose();
        }

        private void QueuePacketSend(byte[] packet)
        {
            if (packet.Length > 256)
            {
                throw new Exception("Max packet size is 256 bytes.");
            }

            _packetSendingQueue.Enqueue(packet);
        }

        private async void SendPacketsAsync()
        {
            while (Client.Connected)
            {
                if (_packetSendingQueue.Count == 0)
                {
                    Thread.Sleep(50);
                    continue;
                }

                var packet = _packetSendingQueue.Dequeue();
                try { await Client.SendAsync(packet); }
                catch { return; }

                Thread.Sleep(50);
            }
        }

        public void Dispose()
        {
            Thread.Sleep(2000);
            _server.Clients.Remove(this);
            Client.Disconnect(false);
            Client.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
