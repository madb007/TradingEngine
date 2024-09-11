using TradingEngineServer.Instrument;

namespace TradingEngineServer.Core
{
    internal interface ITradingEngineServer
    {
        Task Run(CancellationToken token);
        void AddSecurity(Security security);
    }
}
