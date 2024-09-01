using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingEngineServer.Orders
{
    public class Limit
    {
        public Limit(long price) 
        {
            Price = price;
        }

        public long Price {  get; private set; }
        public OrderBookEntry Head {  get; set; }
        public OrderBookEntry Tail { get; set; }

        public uint getLevelOrderCount()
        {
            uint orderCount = 0;
            OrderBookEntry dummyPointer = Head;
            while (dummyPointer != null)
            {
                if (dummyPointer.CurrentOrder.CurrentQuantity != 0)
                {
                    orderCount++;
                }
                dummyPointer = dummyPointer.Next;
            }
            return orderCount;
        }

        public uint getLevelOrderQuantity()
        {
            uint orderQuantity = 0;
            OrderBookEntry dummyPointer = Head;
            while (dummyPointer != null)
            {
                orderQuantity+=dummyPointer.CurrentOrder.CurrentQuantity;
                dummyPointer = dummyPointer.Next;
            }
            return orderQuantity;
        }

        public List<OrderRecord> getLevelOrderRecords()
        {
            List<OrderRecord> orderRecords = new List<OrderRecord>();
            OrderBookEntry dummyPointer = Head;
            uint theoreticalQueuePosition = 0;

            while (dummyPointer != null)
            {
                var currentOrder = dummyPointer.CurrentOrder;
                if (currentOrder.CurrentQuantity != 0)
                {
                    orderRecords.Add(new OrderRecord(currentOrder.OrderID, currentOrder.CurrentQuantity, Price,
                        currentOrder.IsBuySide, currentOrder.Username, currentOrder.SecurityID, theoreticalQueuePosition));

                    theoreticalQueuePosition++;
                    dummyPointer = dummyPointer.Next;
                }
            }
            return orderRecords;
        }
 
        public bool isEmpty
        {
            get
            {
                //if head is null tail should be null anyway but just for kicks
                return Head == null && Tail == null;
            }

        }

        public Side Side
        {
            get
            {
                if (isEmpty)
                {
                    return Side.Unknown;
                }
                else
                {
                    return Head.CurrentOrder.IsBuySide ? Side.Bid : Side.Ask;
                }
            }
        }
    }
}
