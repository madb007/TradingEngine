using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TradingEngineServer.Core;
using TradingEngineServer.Logging;
using TradingEngineServer.Orderbook;
using TradingEngineServer.Orders;
using TradingEngineServer.Instrument;
using TradingEngineServer.Matching;
using Xunit;
using System.Threading;

namespace TradingEngineServer.Tests
{
    public class TradingEngineServerIntegrationTests : IAsyncLifetime
    {
        private IHost _host;
        private IServiceProvider _serviceProvider;
        private ITradingEngineServer _tradingEngine;
        private IOrderbookManager _orderbookManager;
        private ILogger _logger;
        private CancellationTokenSource _cts;
        private IMatchingEngine _matchingEngine;

        public async Task InitializeAsync()
        {
            try
            {
                _host = TradingEngineServerHostBuilder.BuildTradingEngineServer(registerAsHostedService: false);
                await _host.StartAsync();
                _serviceProvider = _host.Services;
                _orderbookManager = _serviceProvider.GetRequiredService<IOrderbookManager>();
                _tradingEngine = _serviceProvider.GetRequiredService<ITradingEngineServer>();
                _logger = _serviceProvider.GetRequiredService<ILogger>();
                _matchingEngine = _serviceProvider.GetRequiredService<IMatchingEngine>();

                _logger.Info("InitializeAsync", "Initialization started");

                _cts = new CancellationTokenSource();
                var tradingEngineTask = Task.Run(async () =>
                {
                    await (_tradingEngine as IHostedService).StartAsync(_cts.Token);
                });

                var security = new Security(1, "AAPL");
                _tradingEngine.AddSecurity(security, _matchingEngine);

                _logger.Info("InitializeAsync", "Security added, waiting for processing");
                await Task.Delay(1000);
                _logger.Info("InitializeAsync", "Initialization completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during initialization: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task DisposeAsync()
        {
            _cts?.Cancel();
            if (_tradingEngine is IHostedService hostedService)
            {
                await hostedService.StopAsync(CancellationToken.None);
            }

            _cts?.Dispose();

            if (_host != null)
            {
                await _host.StopAsync();
                await _host.WaitForShutdownAsync();
                _host.Dispose();
            }
        }

        [Fact]
        public async Task TestOrderbookIntegrationWithMatching()
        {
            // Arrange
            var security = new Security(1, "AAPL");

            // Verify that the orderbook was created
            Assert.True(_orderbookManager.TryGetOrderbook(security, out var orderbook), "Orderbook should have been created");

            // Act
            var buyOrderCore = new OrderCore(1, "user1", security.Id);
            var sellOrderCore = new OrderCore(2, "user2", security.Id);
            var buyOrder = new Order(buyOrderCore, 15000, 100, true);
            var sellOrder = new Order(sellOrderCore, 15100, 100, false);

            orderbook.AddOrder(buyOrder);
            orderbook.AddOrder(sellOrder);

            // Perform matching
            var matchResult = orderbook.Match();

            // Wait for the TradingEngineServer to process the orders and matching
            await Task.Delay(5000);

            // Assert
            var spread = orderbook.GetSpread();
            Assert.NotNull(spread);
            Assert.True(spread.Bid <= spread.Ask, "Bid should be less than or equal to Ask");
            Assert.Equal(15000, spread.Bid);
            Assert.Equal(15100, spread.Ask);

            var bidOrders = orderbook.GetBidOrders();
            var askOrders = orderbook.GetAskOrders();

            Assert.Single(bidOrders);
            Assert.Single(askOrders);
            Assert.Equal(buyOrder.OrderID, bidOrders[0].CurrentOrder.OrderID);
            Assert.Equal(sellOrder.OrderID, askOrders[0].CurrentOrder.OrderID);

            // Assert matching results
            Assert.Empty(matchResult.Trades); // No trades should occur as the prices don't match
            Assert.Empty(matchResult.Rejections);

            // Add a matching order
            var matchingBuyOrderCore = new OrderCore(3, "user3", security.Id);
            var matchingBuyOrder = new Order(matchingBuyOrderCore, 15100, 50, true);
            orderbook.AddOrder(matchingBuyOrder);

            // Perform matching again
            matchResult = orderbook.Match();
            await Task.Delay(5000);

            // Assert matching results
            Assert.Single(matchResult.Trades);
            Assert.Equal(50, (decimal)matchResult.Trades[0].Quantity);
            Assert.Equal(15100, matchResult.Trades[0].Price);
            Assert.Empty(matchResult.Rejections);

            // Check updated orderbook state
            spread = orderbook.GetSpread();
            Assert.Equal(15000, spread.Bid);
            Assert.Equal(15100, spread.Ask);

            bidOrders = orderbook.GetBidOrders();
            askOrders = orderbook.GetAskOrders();

            Assert.Single(bidOrders);
            Assert.Single(askOrders);
            Assert.Equal(50, (decimal)askOrders[0].CurrentOrder.CurrentQuantity);
        }
    }
}