namespace TradingEngineServer.Core.Configuration
{
    internal class TradingEngineServerConfiguration
    {
        public TradingEngineServerSettings TradingEngineServerSettings { get; set; }
    }

    internal class TradingEngineServerSettings
    {
        public int Port { get; set; }
    }
}
