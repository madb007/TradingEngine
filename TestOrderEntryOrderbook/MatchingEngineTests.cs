using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using TradingEngineServer.Matching;
using TradingEngineServer.Orders;
using TradingEngineServer.Orderbook;

namespace TradingEngineServer.Tests
{
    public class MatchingEngineTests
    {
        private readonly IMatchingEngine _fifoEngine;
        private readonly IMatchingEngine _proRataEngine;

        public MatchingEngineTests()
        {
            _fifoEngine = new FIFOMatchingEngine();
            _proRataEngine = new ProRataMatchingEngine();
        }

        [Fact]
        public void FIFO_MatchingSingleBuyAndSellOrder()
        {
            var buyOrder = CreateOrder(1, 100, 10, true);
            var sellOrder = CreateOrder(2, 100, 10, false);
            var limit = CreateLimitWithOrders(sellOrder);

            var result = _fifoEngine.Match(buyOrder, limit);

            Assert.Single(result.Trades);
            Assert.Equal(10, (decimal)result.Trades[0].Quantity);
            Assert.Equal(100, result.Trades[0].Price);
            Assert.Equal(0, (decimal)result.RemainingQuantity);
        }

        [Fact]
        public void FIFO_PartialMatchBuyOrder()
        {
            var buyOrder = CreateOrder(1, 100, 20, true);
            var sellOrder = CreateOrder(2, 100, 10, false);
            var limit = CreateLimitWithOrders(sellOrder);

            var result = _fifoEngine.Match(buyOrder, limit);

            Assert.Single(result.Trades);
            Assert.Equal(10, (decimal)result.Trades[0].Quantity);
            Assert.Equal(100, result.Trades[0].Price);
            Assert.Equal(10,  (decimal)result.RemainingQuantity);
        }

        [Fact]
        public void FIFO_NoMatchDueToPrice()
        {
            var buyOrder = CreateOrder(1, 90, 10, true);
            var sellOrder = CreateOrder(2, 100, 10, false);
            var limit = CreateLimitWithOrders(sellOrder);

            var result = _fifoEngine.Match(buyOrder, limit);

            Assert.Empty(result.Trades);
            Assert.Equal(10, (decimal)result.RemainingQuantity);
        }

        [Fact]
        public void FIFO_MatchMultipleOrders()
        {
            var buyOrder = CreateOrder(1, 100, 30, true);
            var sellOrder1 = CreateOrder(2, 100, 10, false);
            var sellOrder2 = CreateOrder(3, 100, 10, false);
            var sellOrder3 = CreateOrder(4, 100, 20, false);
            var limit = CreateLimitWithOrders(sellOrder1, sellOrder2, sellOrder3);

            var result = _fifoEngine.Match(buyOrder, limit);

            Assert.Equal(3, result.Trades.Count);
            Assert.Equal(30, result.Trades.Sum(t => t.Quantity));
            Assert.Equal(0, (decimal)result.RemainingQuantity);
        }

        [Fact]
        public void ProRata_MatchingSingleBuyAndSellOrder()
        {
            var buyOrder = CreateOrder(1, 100, 10, true);
            var sellOrder = CreateOrder(2, 100, 10, false);
            var limit = CreateLimitWithOrders(sellOrder);

            var result = _proRataEngine.Match(buyOrder, limit);

            Assert.Single(result.Trades);
            Assert.Equal(10, (decimal)result.Trades[0].Quantity);
            Assert.Equal(100, result.Trades[0].Price);
            Assert.Equal(0, (decimal)result.RemainingQuantity);
        }

        [Fact]
        public void ProRata_PartialMatchBuyOrder()
        {
            var buyOrder = CreateOrder(1, 100, 20, true);
            var sellOrder1 = CreateOrder(2, 100, 10, false);
            var sellOrder2 = CreateOrder(3, 100, 10, false);
            var limit = CreateLimitWithOrders(sellOrder1, sellOrder2);

            var result = _proRataEngine.Match(buyOrder, limit);

            Assert.Equal(2, result.Trades.Count);
            Assert.Equal(20, result.Trades.Sum(t => t.Quantity));
            Assert.Equal(0, (decimal)result.RemainingQuantity);
        }

        [Fact]
        public void ProRata_MatchWithUnequalProportions()
        {
            var buyOrder = CreateOrder(1, 100, 100, true);
            var sellOrder1 = CreateOrder(2, 100, 60, false);
            var sellOrder2 = CreateOrder(3, 100, 40, false);
            var limit = CreateLimitWithOrders(sellOrder1, sellOrder2);

            var result = _proRataEngine.Match(buyOrder, limit);

            Assert.Equal(2, result.Trades.Count);
            Assert.Equal(60, (decimal)result.Trades.First(t => t.SellOrder.OrderID == 2).Quantity);
            Assert.Equal(40, (decimal)result.Trades.First(t => t.SellOrder.OrderID == 3).Quantity);
            Assert.Equal(0,  (decimal)result.RemainingQuantity);
        }

        [Fact]
        public void MatchWithEmptyLimitLevel()
        {
            var buyOrder = CreateOrder(1, 100, 10, true);
            var limit = new Limit(100);

            var fifoResult = _fifoEngine.Match(buyOrder, limit);
            var proRataResult = _proRataEngine.Match(buyOrder, limit);

            Assert.Empty(fifoResult.Trades);
            Assert.Empty(proRataResult.Trades);
            Assert.Equal(10u, fifoResult.RemainingQuantity);
            Assert.Equal(10, (decimal)proRataResult.RemainingQuantity);
        }

        [Fact]
        public void MatchWithZeroQuantityOrder()
        {
            var buyOrder = CreateOrder(1, 100, 0, true);
            var sellOrder = CreateOrder(2, 100, 10, false);
            var limit = CreateLimitWithOrders(sellOrder);

            var fifoResult = _fifoEngine.Match(buyOrder, limit);
            var proRataResult = _proRataEngine.Match(buyOrder, limit);

            Assert.Empty(fifoResult.Trades);
            Assert.Empty(proRataResult.Trades);
            Assert.Equal(0u, fifoResult.RemainingQuantity);
            Assert.Equal(0u, proRataResult.RemainingQuantity);
        }

        [Fact]
        public void MatchWithVeryLargeQuantity()
        {
            var buyOrder = CreateOrder(1, 100, uint.MaxValue-1, true);
            var sellOrder = CreateOrder(2, 100, uint.MaxValue-1, false);
            var limit = CreateLimitWithOrders(sellOrder);
            Console.WriteLine($"Limit Quantity after creation: {limit.getLevelOrderQuantity()}");


            var fifoResult = _fifoEngine.Match(buyOrder, limit);


            buyOrder = CreateOrder(1, 100, uint.MaxValue - 1, true);
            sellOrder = CreateOrder(2, 100, uint.MaxValue - 1, false);
            limit = CreateLimitWithOrders(sellOrder);
            var proRataResult = _proRataEngine.Match(buyOrder, limit);

            Assert.Single(fifoResult.Trades);
            Assert.Single(proRataResult.Trades);
            Assert.Equal(uint.MaxValue - 1, fifoResult.Trades[0].Quantity);
            Assert.Equal(uint.MaxValue - 1, proRataResult.Trades[0].Quantity);
            Assert.Equal(0u, fifoResult.RemainingQuantity);
            Assert.Equal(0u, proRataResult.RemainingQuantity);
            Assert.Equal(0u, sellOrder.CurrentQuantity);
        }

        private Order CreateOrder(long orderId, long price, uint quantity, bool isBuySide)
        {
            var orderCore = new OrderCore(orderId, "user" + orderId, 1);
            return new Order(orderCore, price, quantity, isBuySide);
        }

        private Limit CreateLimitWithOrders(params Order[] orders)
        {
            var limit = new Limit(orders[0].Price);
            OrderBookEntry previous = null;
            foreach (var order in orders)
            {
                var entry = new OrderBookEntry(order, limit);
                if (previous == null)
                {
                    Console.WriteLine(entry.CurrentOrder.CurrentQuantity);
                    limit.Head = entry;
                }
                else
                {
                    previous.Next = entry;
                    entry.Previous = previous;
                }
                previous = entry;
            }
            limit.Tail = previous;
            return limit;
        }
    }
}
