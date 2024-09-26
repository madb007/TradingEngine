using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using TradingEngineServer.Orders;
using TradingEngineServer.Instrument;
using TradingEngineServer.Matching;
using TradingEngineServer.Rejects;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;

namespace TradingEngineServer.Orderbook
{
    public class Orderbook : IMatchingOrderbook
    {
        //PRIVATE FIELDS//
        private readonly Security _instrument;
        private readonly Dictionary<long, OrderBookEntry> _orders = new Dictionary<long, OrderBookEntry>();
        private readonly SortedSet<Limit> _askLimits = new SortedSet<Limit>(AskLimitComparer.Comparer);
        private readonly SortedSet<Limit> _bidLimits = new SortedSet<Limit>(BidLimitComparer.Comparer);
        private readonly Queue<Order> _orderQueue = new Queue<Order>();
        private readonly IMatchingEngine _matchingEngine;
        public Orderbook(Security instrument, IMatchingEngine matchingEngine)
        {
            _instrument = instrument;
            _matchingEngine = matchingEngine;
        }

        public MatchResult Match()
        {
            var allTrades = new List<Trade>();
            var allRejections = new List<Rejection>();
      
            while (_orderQueue.Count > 0)
            {
                var order = _orderQueue.Dequeue();
                var oppositeLimit = order.IsBuySide ? _askLimits.FirstOrDefault() : _bidLimits.FirstOrDefault();
                
                if (oppositeLimit == null || !isMatchPossible(order, oppositeLimit))
                {
                    AddOrderToBook(order);
                    continue;
                }

               
                var matchResult = _matchingEngine.Match(order, oppositeLimit);
                Console.WriteLine(order.CurrentQuantity);
                allTrades.AddRange(matchResult.Trades);
                allRejections.AddRange(matchResult.Rejections);

                UpdateOrderBook(matchResult.Trades);

                if(matchResult.RemainingQuantity > 0)
                {
                    order.DecreaseQuantity(matchResult.RemainingQuantity);
                    AddOrderToBook(order);
                }

            }
            return new MatchResult(allTrades, 0, allRejections);
        }

        //Debugging Function
        private void PrintOrderBookState()
        {
            Console.WriteLine("Current Order Book State:");
            Console.WriteLine("Ask Orders:");
            foreach (var order in GetAskOrders())
            {
                Console.WriteLine($"  ID: {order.CurrentOrder.OrderID}, Price: {order.CurrentOrder.Price}, Quantity: {order.CurrentOrder.CurrentQuantity}");
            }
            Console.WriteLine("Bid Orders:");
            foreach (var order in GetBidOrders())
            {
                Console.WriteLine($"  ID: {order.CurrentOrder.OrderID}, Price: {order.CurrentOrder.Price}, Quantity: {order.CurrentOrder.CurrentQuantity}");
            }
        }

        private bool isMatchPossible(Order order, Limit oppositeLimit)
        {
            return (order.IsBuySide && order.Price >= oppositeLimit.Price) ||
                (!order.IsBuySide && order.Price <= oppositeLimit.Price);
        }

        private void UpdateOrderBook(List<Trade> trades)
        {
            foreach (var trade in trades)
            {
                UpdateOrderAfterTrade(trade.BuyOrder, trade.Quantity);
                UpdateOrderAfterTrade(trade.SellOrder, trade.Quantity);
            }
        }

        private void UpdateOrderAfterTrade(Order order, uint tradedQuantity)
        {
            if (_orders.TryGetValue(order.OrderID, out var bookEntry))
            {
                //Would be necessary if we didn't reduce quantity in each Matching Engine individually
                //order.DecreaseQuantity(tradedQuantity);
                if(order.CurrentQuantity == 0)
                {
                    RemoveOrder(new CancelOrder(order));
                }
            }
        }

        public int count => _orders.Count;

        public void AddOrder(Order order)
        {
            _orderQueue.Enqueue(order);
        }

        private void AddOrderToBook(Order order)
        {
            var baseLimit = new Limit(order.Price);
            AddOrderToBookInternal(order, baseLimit, order.IsBuySide ? _bidLimits : _askLimits, _orders);
        }

        private static void AddOrderToBookInternal(Order order, Limit baseLimit, SortedSet<Limit> limitLevels, Dictionary<long,OrderBookEntry> internalBook)
        {
            
            if (limitLevels.TryGetValue(baseLimit, out Limit limit))
            {
                OrderBookEntry orderBookEntry = new OrderBookEntry(order, baseLimit);
                if(limit.Head == null)
                {
                    limit.Head = orderBookEntry;
                    limit.Tail = orderBookEntry;
                }
                else
                {
                    OrderBookEntry tailPointer = limit.Tail;
                    tailPointer.Next = orderBookEntry;
                    orderBookEntry.Previous = tailPointer;
                    limit.Tail = orderBookEntry;
                }
                internalBook.Add(order.OrderID, orderBookEntry);
            }
            else
            {
                limitLevels.Add(baseLimit);
                OrderBookEntry orderBookEntry = new OrderBookEntry(order, baseLimit);
                baseLimit.Head = orderBookEntry;
                baseLimit.Tail = orderBookEntry;
                internalBook.Add(order.OrderID,orderBookEntry);
            }

        }

        public void ChangeOrder(ModifyOrder modifyOrder)
        {
            if(_orders.TryGetValue(modifyOrder.OrderID, out OrderBookEntry orderBookEntry))
            {
                RemoveOrder(modifyOrder.ToCancelOrder());
                AddOrder(modifyOrder.ToNewOrder());
            }
        }

        public bool ContainsOrder(long orderId)
        {
            return _orders.ContainsKey(orderId);
        }

        public List<OrderBookEntry> GetAskOrders()
        {
            List<OrderBookEntry> orderBookEntries = new List<OrderBookEntry>();
            foreach (var askLimit in _askLimits)
            {
                if (askLimit.isEmpty)
                {
                    continue;
                }
                else
                {
                    OrderBookEntry askLimitPointer = askLimit.Head;
                    while (askLimitPointer != null)
                    {
                        orderBookEntries.Add(askLimitPointer);
                        askLimitPointer = askLimitPointer.Next;
                    }
                }
            }
            return orderBookEntries;
        }

        public List<OrderBookEntry> GetBidOrders()
        {
            List<OrderBookEntry> orderBookEntries = new List<OrderBookEntry>();
            foreach (var bidLimit in _bidLimits)
            {
                if (bidLimit.isEmpty)
                {
                    continue;
                }
                else
                {
                    OrderBookEntry bidLimitPointer = bidLimit.Head;
                    while (bidLimitPointer != null)
                    {
                        orderBookEntries.Add(bidLimitPointer);
                        bidLimitPointer = bidLimitPointer.Next;
                    }
                }
            }
            return orderBookEntries;
        }

        public OrderBookSpread GetSpread()
        {
            long? bestAsk = null, bestBid = null;
            if(_askLimits.Any() && !_askLimits.Min.isEmpty)
            {
                bestAsk = _askLimits.Min.Price;
            }
            if(_bidLimits.Any() && !_bidLimits.Min.isEmpty)
            {
                bestBid = _bidLimits.Max.Price;
            }
            return new OrderBookSpread(bestBid, bestAsk);

        }

        public void RemoveOrder(CancelOrder cancelOrder)
        {
            if(_orders.TryGetValue(cancelOrder.OrderID, out OrderBookEntry orderBookEntry))
            {
                RemoveOrder(cancelOrder.OrderID, orderBookEntry, _orders);
            }
        }

        private static void RemoveOrder(long orderID, OrderBookEntry orderBookEntry, Dictionary<long, OrderBookEntry> internalBook )
        {
            //Remove orderBookEntry from linked list 
            if(orderBookEntry.Previous != null && orderBookEntry.Next != null)
            {
                orderBookEntry.Previous.Next = orderBookEntry.Next;
                orderBookEntry.Next.Previous = orderBookEntry.Previous;
            }
            else if(orderBookEntry.Previous != null)
            {
                orderBookEntry.Previous.Next = null;
            }
            else if(orderBookEntry.Next != null)
            {
                orderBookEntry.Next.Previous = null;
            }

            // Deal with orderBookEntry on Limit-level
            if(orderBookEntry.ParentLimit.Head == orderBookEntry && orderBookEntry.ParentLimit.Tail == orderBookEntry)
            {
                //only one order
                orderBookEntry.ParentLimit.Head = null;
                orderBookEntry.ParentLimit.Tail = null;
            }
            else if (orderBookEntry.ParentLimit.Head == orderBookEntry)
            {
                //orderBookEntry is first order
                orderBookEntry.ParentLimit.Head = orderBookEntry.Next;
            }
            else if (orderBookEntry.ParentLimit.Tail == orderBookEntry)
            {
                //orderBookEntry is last order
                orderBookEntry.ParentLimit.Tail = orderBookEntry;
            }

            internalBook.Remove(orderID);
        }
    }
}
