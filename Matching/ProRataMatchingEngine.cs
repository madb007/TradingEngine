using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using TradingEngineServer.Orders;

namespace TradingEngineServer.Matching
{
    public class ProRataMatchingEngine : IMatchingEngine
    {
        public MatchResult Match(Order incomingOrder, Limit bestOppositeLimit)
        {
            if (incomingOrder.CurrentQuantity == 0 || bestOppositeLimit == null || bestOppositeLimit.Head == null)
            {
                return new MatchResult(new List<Trade>(), incomingOrder.CurrentQuantity);
            }

            var trades = new List<Trade>();
            var remainingQuantity = incomingOrder.CurrentQuantity;
            ulong oppositeQuantity = bestOppositeLimit.getLevelOrderQuantity();
            if(oppositeQuantity == 0)
            { 
                return new MatchResult(trades, remainingQuantity);
            }

            var currentEntry = bestOppositeLimit.Head;

            while (currentEntry != null && remainingQuantity > 0)
            {
                if(isMatchPossible(incomingOrder,currentEntry.CurrentOrder))
                {
                    decimal ratio = (decimal)currentEntry.CurrentOrder.CurrentQuantity / oppositeQuantity;
                    uint matchQuantity = (uint)Math.Min(remainingQuantity, Math.Floor(incomingOrder.CurrentQuantity * ratio));

                    if (matchQuantity > 0)
                    {
                        trades.Add(new Trade(incomingOrder, currentEntry.CurrentOrder, matchQuantity, bestOppositeLimit.Price));

                        remainingQuantity -= matchQuantity;
                        currentEntry.CurrentOrder.DecreaseQuantity(matchQuantity);
                    }
                }
                currentEntry = currentEntry.Next;
            }
            
            // RemainingQuantity could exist due to rounding
            if(remainingQuantity > 0)
            {
                currentEntry = bestOppositeLimit.Head;
                while(currentEntry != null && remainingQuantity > 0)
                {
                    if(currentEntry.CurrentOrder.CurrentQuantity > 0)
                    {
                        var finalMatchQuantity = Math.Min(currentEntry.CurrentOrder.CurrentQuantity, remainingQuantity);

                        trades.Add(new Trade(incomingOrder, currentEntry.CurrentOrder, (uint)finalMatchQuantity, bestOppositeLimit.Price));

                        remainingQuantity -= finalMatchQuantity;
                        currentEntry.CurrentOrder.DecreaseQuantity(remainingQuantity);
                    }
                    currentEntry = currentEntry.Next;
                }
            }
            return new MatchResult(trades, (uint)remainingQuantity);
        }

        private bool isMatchPossible(Order incomingOrder, Order oppositeOrder)
        {
            return (incomingOrder.IsBuySide && incomingOrder.Price >= oppositeOrder.Price) ||
                (!incomingOrder.IsBuySide && incomingOrder.Price <= oppositeOrder.Price);
        }
    }
}
