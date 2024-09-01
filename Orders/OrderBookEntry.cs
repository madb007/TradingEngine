using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace TradingEngineServer.Orders
{
    public class OrderBookEntry
    {
        
        public OrderBookEntry(Order currentOrder, Limit parentLimit) 
        {
            CreationTime = DateTime.Now;
            ParentLimit = parentLimit;
            CurrentOrder = currentOrder;
        }

        public DateTime CreationTime { get; private set; }
        public Order CurrentOrder { get; init; }
        public Limit ParentLimit { get; private set; }
        public OrderBookEntry Next  { get; set; }
        public OrderBookEntry Previous { get; set; }
    }
}
