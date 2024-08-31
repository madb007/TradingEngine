using TradingEngineServer.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TradingEngineServer.Core.Configuration;

namespace TradingEngineServer.Core
{
    public sealed class TradingEngineServerHostBuilder
    {
        public static IHost BuildTradingEngineServer() => Host.CreateDefaultBuilder().ConfigureServices((context,services) =>
            {
                //Begin with config 
                services.AddOptions();
                services.Configure<TradingEngineServerConfiguration>(context.Configuration.GetSection(nameof(TradingEngineServerConfiguration)));
                services.Configure<LoggerConfiguration>(context.Configuration.GetSection(nameof(LoggerConfiguration)));

                //Add singleton objects.
                services.AddSingleton<ITradingEngineServer, TradingEngineServer>();
                services.AddSingleton<ITextLogger,TextLogger>();
                //Add hosted service.
                services.AddHostedService<TradingEngineServer>();
            }).Build();
    }
}
