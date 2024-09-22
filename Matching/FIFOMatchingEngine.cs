using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingEngineServer.Orders;

namespace TradingEngineServer.Matching
{
    public class FIFOMatchingEngine : IMatchingEngine
    {
        public MatchResult Match(Order incomingOrder, Limit bestOppositeLimit)
        {
            if(incomingOrder.CurrentQuantity == 0 || bestOppositeLimit == null || bestOppositeLimit.Head == null)
            {
                return new MatchResult(new List<Trade>(), incomingOrder.CurrentQuantity);
            }
            var trades = new List<Trade>();
            var remainingQuantity = incomingOrder.CurrentQuantity;

            var currentEntry = bestOppositeLimit.Head;

            while (currentEntry != null && remainingQuantity > 0)
            {
                if (isMatchPossible(incomingOrder, currentEntry.CurrentOrder))
                {
                    var matchQuantity = Math.Min(remainingQuantity, currentEntry.CurrentOrder.CurrentQuantity);
                    var matchPrice = bestOppositeLimit.Price;

                    trades.Add(new Trade(incomingOrder, currentEntry.CurrentOrder, (uint)matchQuantity, matchPrice));

                    remainingQuantity -= matchQuantity;
                    currentEntry.CurrentOrder.DecreaseQuantity(matchQuantity);
                }

                else
                {
                    break;
                }

                currentEntry = currentEntry.Next;
            }

            return  new MatchResult(trades, remainingQuantity);
        }

        private bool isMatchPossible(Order incomingOrder, Order oppositeOrder)
        {
            return (incomingOrder.IsBuySide && incomingOrder.Price >= oppositeOrder.Price) ||
                (!incomingOrder.IsBuySide && incomingOrder.Price <= oppositeOrder.Price);
        }
    }
}
