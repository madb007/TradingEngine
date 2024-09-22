using System.Collections.Concurrent;
using TradingEngineServer.Instrument;
using TradingEngineServer.Matching;

namespace TradingEngineServer.Orderbook
{
    public class OrderbookManager : IOrderbookManager
    {
        private readonly ConcurrentDictionary<Security, IMatchingOrderbook> _orderbooks = new ConcurrentDictionary<Security, IMatchingOrderbook>();

        public Security[] GetSecurities()
        {
            return _orderbooks.Keys.ToArray();
        }

        public IMatchingOrderbook GetOrderbook(Security security)
        {
            if (_orderbooks.TryGetValue(security, out var orderbook))
            {
                return orderbook;
            }
            throw new KeyNotFoundException($"Orderbook for security ID {security.Id} not found.");
        }

        public void CreateOrderbook(Security security, IMatchingEngine matchingEngine)
        {
            _orderbooks.TryAdd(security, new Orderbook(security, matchingEngine));
        }

        public bool TryGetOrderbook(Security security, out IMatchingOrderbook orderbook)
        {
            return _orderbooks.TryGetValue(security, out orderbook);
        }
    }
}