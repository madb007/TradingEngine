using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using TradingEngineServer.Orders;
using TradingEngineServer.Instrument;
using System.Reflection.Metadata.Ecma335;

namespace TradingEngineServer.Orderbook
{
    public class Orderbook : IRetrievalOrderbook
    {
        //PRIVATE FIELDS//
        private readonly Security _instrument;
        private readonly Dictionary<long, OrderBookEntry> _orders = new Dictionary<long, OrderBookEntry>();
        private readonly SortedSet<Limit> _askLimits = new SortedSet<Limit>(AskLimitComparer.Comparer);
        private readonly SortedSet<Limit> _bidLimits = new SortedSet<Limit>(BidLimitComparer.Comparer);

        public Orderbook(Security instrument)
        {
            _instrument = instrument;
        }

        public int count => _orders.Count;

        public void AddOrder(Order order)
        {
            var baseLimit = new Limit(order.Price);
            AddOrder(order, baseLimit, order.IsBuySide ? _bidLimits : _askLimits, _orders);
        }

        private static void AddOrder(Order order, Limit baseLimit, SortedSet<Limit> limitLevels, Dictionary<long,OrderBookEntry> internalBook)
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
                AddOrder(modifyOrder.ToNewOrder(), orderBookEntry.ParentLimit, modifyOrder.IsBuySide ? _bidLimits : _askLimits, _orders);
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
