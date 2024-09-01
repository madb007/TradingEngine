namespace TradingEngineServer.Orderbook
{
    public interface IReadOnlyOrderbook
    {
        bool ContainsOrder(long orderId);
        OrderBookSpread GetSpread();
        int count { get; }
    }
}
