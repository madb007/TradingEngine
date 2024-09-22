using System.Collections.Concurrent;
using TradingEngineServer.Instrument;
using TradingEngineServer.Matching;

namespace TradingEngineServer.Orderbook
{
    public interface IOrderbookManager
    {
        Security[] GetSecurities();
        IMatchingOrderbook GetOrderbook(Security security);
        void CreateOrderbook(Security security, IMatchingEngine matchingEngine);
        bool TryGetOrderbook(Security security, out IMatchingOrderbook orderbook);
    }
}