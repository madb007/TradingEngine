using TradingEngineServer.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TradingEngineServer.Core.Configuration;
using TradingEngineServer.Orderbook;
using TradingEngineServer.Instrument;
using System.Collections.Concurrent;
using TradingEngineServer.Matching;
namespace TradingEngineServer.Core
{
    internal sealed class TradingEngineServer : BackgroundService, ITradingEngineServer
    {
        private readonly ILogger _logger;
        private readonly TradingEngineServerConfiguration _configuration;
        private readonly IOrderbookManager _orderbookManager;

        public TradingEngineServer(ILogger logger,
                                   IOptions<TradingEngineServerConfiguration> config,
                                   IOrderbookManager orderbookManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = config.Value ?? throw new ArgumentNullException(nameof(config));
            _orderbookManager = orderbookManager ?? throw new ArgumentNullException(nameof(orderbookManager));
        }

        public Task Run(CancellationToken token) => ExecuteAsync(token);

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Info(nameof(TradingEngineServer), "StartAsync called");
            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Info(nameof(TradingEngineServer), "Starting Trading Engine");

            while (!stoppingToken.IsCancellationRequested)
            {
                LogAllOrderBookStates();
                Thread.Sleep(2500); // Sleep for 5 seconds
            }

            _logger.Info(nameof(TradingEngineServer), "Stopping Trading Engine");
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Info(nameof(TradingEngineServer), "StopAsync called");
            return base.StopAsync(cancellationToken);
        }

        public void AddSecurity(Security security, IMatchingEngine matchingEngine)
        {
            _orderbookManager.CreateOrderbook(security, matchingEngine);
            _logger.Info(nameof(TradingEngineServer), $"Added security: {security.Name} (ID: {security.Id})");
        }

        private void LogAllOrderBookStates()
        {

            Security[] activeSecurities = _orderbookManager.GetSecurities(); // Example with AAPL and GOOGL

            foreach (Security security in activeSecurities)
            {
                try
                {
                    var orderbook = _orderbookManager.GetOrderbook(security);
                    var spread = orderbook.GetSpread();
                    _logger.Info(nameof(TradingEngineServer),
                        $"Security Name {security.Name} Security ID {security.Id} - Current spread - Best Bid: {spread.Bid}, Best Ask: {spread.Ask}");
                }
                catch (KeyNotFoundException)
                {
                    _logger.Error(nameof(TradingEngineServer),
                        $"No orderbook found for Security Name {security.Name} Security ID {security.Id}");
                }
            }
        }
    }
}
