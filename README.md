# Trading Engine Server

A high-performance, scalable trading engine simulation built in C# .NET 8.0 that models markets.

## Features

- Efficient order book management using advanced data structures (SortedSet, Dictionary)
- Support for multiple matching algorithms (FIFO and Pro-rata)
- Real-time order matching and trade execution
- Scalable architecture capable of handling multiple financial instruments
- Unit and integration testing suite
- Custom logger with text and console logging capabilities

## Architecture

The trading engine is built on a modular architecture:

- `Orderbook`: Manages the order book for each financial instrument
- `MatchingEngine`: Implements order matching algorithms (FIFO and Pro-rata)
- `Order`: Represents individual orders in the system
- `Trade`: Represents executed trades
- `Security`: Represents financial instruments

## Setup

1. Clone the repository:

2. Open the solution in Visual Studio or your preferred C# IDE.

3. Restore NuGet packages.

4. Build the solution.

## Usage

To use the trading engine in your project (see integration tests and TradingEngineServer class for additional examples):

1. Initialize the `OrderbookManager` and `MatchingEngine`:

   ```csharp
   var orderbookManager = new OrderbookManager();
   var matchingEngine = new FIFOMatchingEngine(); // or ProRataMatchingEngine
   ```

2. Create a security and add it to the system:

   ```csharp
   var security = new Security(1, "AAPL");
   orderbookManager.CreateOrderbook(security, matchingEngine);
   ```

3. Add orders to the orderbook:

   ```csharp
   var orderbook = orderbookManager.GetOrderbook(security);
   var buyOrder = new Order(new OrderCore(1, "user1", security.Id), 100, 10, true);
   orderbook.AddOrder(buyOrder);
   ```

4. Perform matching:

   ```csharp
   var matchResult = orderbook.Match();
   ```

5. Process the match results:

   ```csharp
   foreach (var trade in matchResult.Trades)
   {
       Console.WriteLine($"Trade executed: {trade.Quantity} @ {trade.Price}");
   }
   ```

## Testing

Run the included unit and integration tests to verify the system's functionality:

```
dotnet test
```

## Future Enhancements

- Implement additional matching algorithms
- Add support for market orders and other order types
- Integrate with a real-time market data feed
- Implement risk management features
- Add a RESTful API for external integrations
