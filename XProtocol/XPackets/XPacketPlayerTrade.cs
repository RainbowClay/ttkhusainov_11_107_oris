using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XProtocol.XPackets
{
    class XPacketPlayerTrade
    {
        public int idTrader { get; set; }
        public int idBuyer { get; set; }
        public int idTraderResource { get; set; }
        public int idBuyerResource { get; set; }
        public int countTraderResource { get; set; }
        public int countBuyerResource { get; set; }
    }
}
