using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TradingEngineServer.Logging
{
    public class ConsoleLogger : AbstractLogger, IConsoleLogger, IDisposable
    {
        //FIELDS//
        private readonly LoggerConfiguration _loggingConfiguration;
        private readonly BufferBlock<LogInfo> _logQueue = new BufferBlock<LogInfo>();
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private bool _disposed;
        private Task _logTask;

        public ConsoleLogger(IOptions<LoggerConfiguration> loggingConfiguration) : base()
        {

            _loggingConfiguration = loggingConfiguration.Value ?? throw new ArgumentNullException(nameof(loggingConfiguration));
            if (_loggingConfiguration.LoggerType != LoggerType.Console)
            {
                throw new ArgumentException($"{nameof(TextLogger)} doesn't match LoggerType of {_loggingConfiguration.LoggerType}");
            }

            _logTask = Task.Run(() => LogAsync(_logQueue, _tokenSource.Token));

        }

        private static async Task LogAsync(BufferBlock<LogInfo> logQueue, CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var logItem = await logQueue.ReceiveAsync(token).ConfigureAwait(false);
                    string formattedMessage = FormatLogItem(logItem);
                    Console.WriteLine(formattedMessage);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
            }

        }

        private static string FormatLogItem(LogInfo logItem)
        {
            string threadInfo = string.IsNullOrEmpty(logItem.threadName)
                ? $"{"Thread",25}:{logItem.threadID:000}]"
                : $"{logItem.threadName,25}:{logItem.threadID:000}]";

            return $"[{logItem.now:yyyy-MM-dd HH:mm:ss.fffffff}] [{threadInfo} [{logItem.logLevel}] {logItem.module}: {logItem.message}";
        }

        protected override void Log(LoggingLevels logLevel, string module, string message)
        {
            try
            {
                _logQueue.Post(new LogInfo(logLevel, module, message,
                    DateTime.Now, Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.Name));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error posting to log queue: {ex}");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                try
                {
                    _tokenSource.Cancel();
                    _logTask?.Wait(TimeSpan.FromSeconds(5));
                    _tokenSource.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during disposal: {ex}");
                }
            }

            _disposed = true;
        }

        ~ConsoleLogger()
        {
            Dispose(false);
        }
    }
}
