namespace TradingEngineServer.Core
{
    internal interface ITradingEngineServer
    {
        Task Run(CancellationToken token);
    }
}
