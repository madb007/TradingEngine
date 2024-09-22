using TradingEngineServer.Instrument;
using TradingEngineServer.Matching;

namespace TradingEngineServer.Core
{
    internal interface ITradingEngineServer
    {
        Task Run(CancellationToken token);
        void AddSecurity(Security security, IMatchingEngine matchingEngine);
    }
}
