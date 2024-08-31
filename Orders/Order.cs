using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingEngineServer.Orders
{
    public class Order : IOrderCore
    {
        public Order(IOrderCore orderCore, long price, uint quantity, bool isBuySide) {
            Price = price;
            InitialQuantity = quantity;
            CurrentQuantity = quantity;
            IsBuySide = isBuySide;

            _orderCore = orderCore;

        }

        public Order(ModifyOrder modifyOrder) : this(modifyOrder, modifyOrder.Price, modifyOrder.Quantity, modifyOrder.IsBuySide) 
        {
        }
        //PUBLIC PROPERTIES//
        public long Price { get; private set; }
        public uint InitialQuantity { get; private set; }
        public uint CurrentQuantity {  get; private set; }
        public bool IsBuySide { get; private set; }

        public long OrderID => _orderCore.OrderID;

        public string Username => _orderCore.Username;

        public int SecurityID => _orderCore.SecurityID;

        //METHODS//
        public void IncreaseQuantity(uint quantityDelta)
        {
            CurrentQuantity += quantityDelta;
        }

        public void DecreaseQuantity(uint quantityDelta)
        {
            if (quantityDelta > CurrentQuantity)
            {
                throw new InvalidOperationException($"Quantity Delta > Current Quantity for Order {OrderID}");
            }
            CurrentQuantity -= quantityDelta;
        }

        //PRIVATE FIELDS//
        private readonly IOrderCore _orderCore;
    }
}
