namespace TradingEngineServer.Orders
{
    public interface IOrderCore
    {
        //user's metadata should not be allowed to be set
        public long OrderID { get; }

        public string Username { get; }
        public int SecurityID { get; }
    }
}
