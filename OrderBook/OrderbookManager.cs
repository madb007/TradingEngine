using System.Collections.Concurrent;
using TradingEngineServer.Instrument;

namespace TradingEngineServer.Orderbook
{
    public class OrderbookManager : IOrderbookManager
    {
        private readonly ConcurrentDictionary<Security, IRetrievalOrderbook> _orderbooks = new ConcurrentDictionary<Security, IRetrievalOrderbook>();

        public Security[] GetSecurities()
        {
            return _orderbooks.Keys.ToArray();
        }

        public IRetrievalOrderbook GetOrderbook(Security security)
        {
            if (_orderbooks.TryGetValue(security, out var orderbook))
            {
                return orderbook;
            }
            throw new KeyNotFoundException($"Orderbook for security ID {security.Id} not found.");
        }

        public void CreateOrderbook(Security security)
        {
            _orderbooks.TryAdd(security, new Orderbook(security));
        }

        public bool TryGetOrderbook(Security security, out IRetrievalOrderbook orderbook)
        {
            return _orderbooks.TryGetValue(security, out orderbook);
        }
    }
}