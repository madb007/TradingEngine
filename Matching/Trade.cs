using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingEngineServer.Orders;

namespace TradingEngineServer.Matching
{
    public class Trade
    {
        public Order BuyOrder { get; }
        public Order SellOrder { get; }
        public uint Quantity { get; }
        public long Price { get; }
        public DateTime Timestamp { get; }

        public Trade(Order incomingOrder, Order bookOrder, uint quantity, long price)
        {
            BuyOrder = incomingOrder.IsBuySide ? incomingOrder : bookOrder;
            SellOrder = incomingOrder.IsBuySide ? bookOrder : incomingOrder;
            Quantity = quantity;
            Price = price;
            Timestamp = DateTime.UtcNow;
        }
    }
}