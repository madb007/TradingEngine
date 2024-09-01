using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingEngineServer.Orders
{
    //SUMMARY//
    //Read-only order representation
    public record OrderRecord(long OrderID, uint Quantity, long Price, bool isBuySide, string Username, int SecurityID, uint TheoreticalQueuePosition);
}
