using TradingEngineServer.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TradingEngineServer.Core.Configuration;
using TradingEngineServer.Orderbook;
using Microsoft.Extensions.Options;
using TradingEngineServer.Matching;

namespace TradingEngineServer.Core
{
    public sealed class TradingEngineServerHostBuilder
    {
        public static IHost BuildTradingEngineServer(bool registerAsHostedService = true)
        {
            return Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Begin with config 
                    services.AddOptions();
                    services.Configure<TradingEngineServerConfiguration>(context.Configuration.GetSection(nameof(TradingEngineServerConfiguration)));
                    services.Configure<LoggerConfiguration>(context.Configuration.GetSection(nameof(LoggerConfiguration)));
                    services.Configure<MatchingConfiguration>(context.Configuration.GetSection(nameof(MatchingConfiguration)));

                    // Add singleton objects.
                    services.AddSingleton<ITradingEngineServer, TradingEngineServer>();

                    services.AddSingleton<ILogger>(serviceProvider =>
                    {
                        var loggerConfig = serviceProvider.GetRequiredService<IOptions<LoggerConfiguration>>().Value;

                        switch (loggerConfig.LoggerType)
                        {
                            case LoggerType.Text:
                                return new TextLogger(serviceProvider.GetRequiredService<IOptions<LoggerConfiguration>>());

                            case LoggerType.Console:
                                return new ConsoleLogger(serviceProvider.GetRequiredService<IOptions<LoggerConfiguration>>());

                            default:
                                throw new ArgumentException($"Unsupported logger type: {loggerConfig.LoggerType}");
                        }
                    });

                    services.AddSingleton<IOrderbookManager, OrderbookManager>();

                    services.AddSingleton<IMatchingEngine>(serviceProvider =>
                    {
                        var config = serviceProvider.GetRequiredService<IOptions<MatchingConfiguration>>().Value;
                        Console.WriteLine(config.MatchingType);
                        return config.MatchingType switch
                        {
                            MatchingType.FIFO => new FIFOMatchingEngine(),
                            MatchingType.ProRata => new ProRataMatchingEngine(),
                            _ => throw new ArgumentException($"Unsupported matching engine type: {config.MatchingType}")
                        };
                    });
                    // Add hosted service only if specified
                    if (registerAsHostedService)
                    {
                        services.AddHostedService<TradingEngineServer>();
                    }
                })
                .Build();
        }
    }
}