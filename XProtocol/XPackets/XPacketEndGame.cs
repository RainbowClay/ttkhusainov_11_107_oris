using XProtocol.Serializer;

namespace XProtocol.XPackets
{
    public class XPacketEndGame
    {
        [XField(1)]
        public int PlayerId;
    }
}
