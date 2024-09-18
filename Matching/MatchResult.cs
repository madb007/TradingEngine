using TradingEngineServer.Rejects;

namespace TradingEngineServer.Matching
{
    public class MatchResult
    {
        public List<Trade> Trades { get;  }
        public uint RemainingQuantity { get; }
        public List<Rejection> Rejections { get; }

        public MatchResult(List<Trade> trades, uint remainingQuantity, List<Rejection> rejections = null)
        {
            Trades = trades;
            RemainingQuantity = remainingQuantity;
            Rejections = rejections ?? new List<Rejection>();
        }
    }
}
