using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingEngineServer.Orders
{
    public class ModifyOrder : IOrderCore
    {
        public ModifyOrder(IOrderCore orderCore, long modifiedprice, uint modifiedQuantity, bool isBuySide)
        {
            Price = modifiedprice;
            Quantity = modifiedQuantity;
            IsBuySide = isBuySide;

            _orderCore = orderCore;
        }

        //PROPERTIES//
        public long Price { get; private set; }
        public uint Quantity { get; private set; }
        public bool IsBuySide { get; private set; }

        public long OrderID => _orderCore.OrderID;

        public string Username => _orderCore.Username;

        public int SecurityID => _orderCore.SecurityID;

        //METHODS//
        public CancelOrder ToCancelOrder()
        {
            return new CancelOrder(this);
        }

        public Order ToNewOrder()
        {
            return new Order(this);
        }


        //FIELDS//
        private readonly IOrderCore _orderCore;
    }
}
