namespace XProtocol
{
    public static class XPacketTypeManager
    {
        private static readonly Dictionary<XPacketType, Tuple<byte, byte>> TypeDictionary =
            new Dictionary<XPacketType, Tuple<byte, byte>>();

        static XPacketTypeManager()
        {
            RegisterType(XPacketType.Build, 0, 0);
            RegisterType(XPacketType.EndGame, 0, 1);
            RegisterType(XPacketType.EndStep, 0, 2);
            RegisterType(XPacketType.Handshake, 0, 3);
            RegisterType(XPacketType.MoveBandit, 0, 4);
            RegisterType(XPacketType.Pause, 0, 5);
            RegisterType(XPacketType.PauseEnded, 0, 6);
            RegisterType(XPacketType.PlayerTrade, 0, 7);
            RegisterType(XPacketType.ReceivedKnightCard, 0, 8);
            RegisterType(XPacketType.ReceiveResource, 0, 9);
            RegisterType(XPacketType.ResultOfThrow, 0, 10);
            RegisterType(XPacketType.StartGame, 0, 11);
            RegisterType(XPacketType.StepResult, 0, 12);
            RegisterType(XPacketType.SuccessfulRegistration, 0, 13);
            RegisterType(XPacketType.ThrowDice, 0, 14);
            RegisterType(XPacketType.Winner, 0, 15);
        }

        public static void RegisterType(XPacketType type, byte btype, byte bsubtype)
        {
            if (TypeDictionary.ContainsKey(type))
            {
                throw new Exception($"Packet type {type:G} is already registered.");
            }

            TypeDictionary.Add(type, Tuple.Create(btype, bsubtype));
        }

        public static Tuple<byte, byte> GetType(XPacketType type)
        {
            if (!TypeDictionary.ContainsKey(type))
            {
                throw new Exception($"Packet type {type:G} is not registered.");
            }

            return TypeDictionary[type];
        }

        public static XPacketType GetTypeFromPacket(XPacket packet)
        {
            var type = packet.PacketType;
            var subtype = packet.PacketSubtype;

            foreach (var tuple in TypeDictionary)
            {
                var value = tuple.Value;

                if (value.Item1 == type && value.Item2 == subtype)
                {
                    return tuple.Key;
                }
            }

            return XPacketType.Unknown;
        }
    }
}
