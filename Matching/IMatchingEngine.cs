using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingEngineServer.Orders;

namespace TradingEngineServer.Matching
{
    public interface IMatchingEngine
    {
        MatchResult Match(Order incomingOrder, Limit bestOppositeLimit);
    }
}
