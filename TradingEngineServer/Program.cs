using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TradingEngineServer;
using TradingEngineServer.Core;

using var engine = TradingEngineServerHostBuilder.BuildTradingEngineServer();
TradingEngineServerServiceProvider.ServiceProvider = engine.Services;
{
    //likely won't used scoped services but just for practice
    using var scope = TradingEngineServerServiceProvider.ServiceProvider.CreateScope();
    await engine.RunAsync().ConfigureAwait(false);
}