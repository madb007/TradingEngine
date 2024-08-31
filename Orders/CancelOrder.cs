using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingEngineServer.Orders
{
    internal class CancelOrder : IOrderCore
    {
        public CancelOrder(IOrderCore orderCore) 
        {
            _orderCore = orderCore;
        }

        // PROPERTIES //

        public long OrderID => throw new NotImplementedException();

        public string Username => throw new NotImplementedException();

        public int SecurityID => throw new NotImplementedException();

        // FIELDS //

        private readonly IOrderCore _orderCore;

        
    }
}
