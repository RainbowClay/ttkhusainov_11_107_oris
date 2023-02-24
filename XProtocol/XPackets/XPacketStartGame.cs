using XProtocol.Serializer;

namespace XProtocol.XPackets
{
    public class XPacketStartGame
    {
        [XField(1)]
        public bool GameStarted = true;
    }
}
