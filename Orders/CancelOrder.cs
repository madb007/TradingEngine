using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingEngineServer.Orders
{
    public class CancelOrder : IOrderCore
    {
        public CancelOrder(IOrderCore orderCore) 
        {
            _orderCore = orderCore;
        }

        // PROPERTIES //

        public long OrderID => _orderCore.OrderID;

        public string Username => _orderCore.Username;

        public int SecurityID => _orderCore.SecurityID;

        // FIELDS //

        private readonly IOrderCore _orderCore;

        
    }
}
