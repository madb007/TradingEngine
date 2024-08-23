using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TradingEngineServer.Core.Configuration;

namespace TradingEngineServer.Core
{
    internal sealed class TradingEngineServer : BackgroundService, ITradingEngineServer
    {
        private readonly ILogger<TradingEngineServer> _logger;
        private readonly TradingEngineServerConfiguration _configuration;
        public TradingEngineServer(ILogger<TradingEngineServer> logger,IOptions<TradingEngineServerConfiguration> config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = config.Value ?? throw new ArgumentNullException(nameof(config));
        }

        public Task Run(CancellationToken token) => ExecuteAsync(token);
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Starting {nameof(TradingEngineServer)}");
            while (!stoppingToken.IsCancellationRequested)
            {
            }
            _logger.LogInformation($"Ending {nameof(TradingEngineServer)}");
            return Task.CompletedTask;
        }
    }
}
