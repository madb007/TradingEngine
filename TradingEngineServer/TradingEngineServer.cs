using TradingEngineServer.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TradingEngineServer.Core.Configuration;
namespace TradingEngineServer.Core
{
    internal sealed class TradingEngineServer : BackgroundService, ITradingEngineServer
    {
        private readonly ITextLogger _logger;
        private readonly TradingEngineServerConfiguration _configuration;
        public TradingEngineServer(ITextLogger textLogger,IOptions<TradingEngineServerConfiguration> config)
        {
            _logger = textLogger ?? throw new ArgumentNullException(nameof(textLogger));
            _configuration = config.Value ?? throw new ArgumentNullException(nameof(config));
        }

        public Task Run(CancellationToken token) => ExecuteAsync(token);
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Info(nameof(TradingEngineServer), "Starting Trading Engine");
            while (!stoppingToken.IsCancellationRequested)
            {
            }
            _logger.Info(nameof(TradingEngineServer), "Stopping Trading Engine");
            return Task.CompletedTask;
        }
    }
}
