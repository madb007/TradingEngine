using System.Collections.Concurrent;
using TradingEngineServer.Instrument;

namespace TradingEngineServer.Orderbook
{
    public interface IOrderbookManager
    {
        Security[] GetSecurities();
        IRetrievalOrderbook GetOrderbook(Security security);
        void CreateOrderbook(Security security);
        bool TryGetOrderbook(Security security, out IRetrievalOrderbook orderbook);
    }
}