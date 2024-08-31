using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingEngineServer.Orders;

namespace TradingEngineServer.Rejects
{
    public class Rejection : IOrderCore
    {
        public Rejection(IOrderCore rejectedOrder, RejectionReason rejectionReason )
        {
            RejectionReason = rejectionReason;
            _orderCore = rejectedOrder;
        }

        //PROPERTIES//
        public RejectionReason RejectionReason { get; private set; }
        public long OrderID => _orderCore.OrderID;
        public string Username => _orderCore.Username;
        public int SecurityID => _orderCore.SecurityID;

        //FIELDS//
        private readonly IOrderCore _orderCore;
    }

    
}
