using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TradingEngineServer.Logging
{
    public class TextLogger : AbstractLogger, ITextLogger, IDisposable
    {
        //FIELDS//
        private readonly LoggerConfiguration _loggingConfiguration;
        private readonly BufferBlock<LogInfo> _logQueue = new BufferBlock<LogInfo>();
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private bool _disposed;
        private Task _logTask;
        private string _currentLogFilePath;

        public TextLogger(IOptions<LoggerConfiguration> loggingConfiguration) : base()
        {

            _loggingConfiguration = loggingConfiguration.Value ?? throw new ArgumentNullException(nameof(loggingConfiguration));
            if (_loggingConfiguration.LoggerType != LoggerType.Text)
            {
                throw new ArgumentException($"{nameof(TextLogger)} doesn't match LoggerType of {_loggingConfiguration.LoggerType}");
            }

            string logDirectory = CreateLogDirectory();
            _currentLogFilePath = CreateLogFilePath(logDirectory);

            _logTask = Task.Run(() => LogAsync(_currentLogFilePath, _logQueue, _tokenSource.Token));
            
        }

        private string CreateLogDirectory()
        {
            try
            {
                string baseDir = Environment.ExpandEnvironmentVariables(_loggingConfiguration.TextLoggerConfiguration.Directory);
                var now = DateTime.Now;
                string fullPath = Path.Combine(baseDir, "Logs", $"{now:yyyy-MM-dd}");

                
                Directory.CreateDirectory(fullPath);
                
                // Simple write test
                //string testFile = Path.Combine(fullPath, "test.txt");
                //File.WriteAllText(testFile, "Test");
                //File.Delete(testFile);

                Console.WriteLine($"Successfully created and verified log directory: {fullPath}");
                return fullPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating log directory: {ex.Message}");
                throw; // Rethrow the exception to be handled by the caller
            }
        }

        private string CreateLogFilePath(string logDirectory)
        {
            try
            {
                var now = DateTime.Now;
                string uniqueLogName = $"{_loggingConfiguration.TextLoggerConfiguration.Filename} {now:HH_mm_ss}";
                string baseLogName = Path.ChangeExtension(uniqueLogName,_loggingConfiguration.TextLoggerConfiguration.FileExtension);
                string filePath = Path.Combine(logDirectory, baseLogName);
                //Console.WriteLine($"Attempting to create log file: {filePath}");

                // Test file creation
                //using (File.Create(filePath)) { }
                //Console.WriteLine($"Successfully created log file: {filePath}");

                return filePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating log file in {logDirectory}: {ex}");
                throw;
            }
        }

        private static async Task LogAsync(string filepath, BufferBlock<LogInfo> logQueue, CancellationToken token)
        {
            try
            {
                using var fs = new FileStream(filepath, FileMode.Append, FileAccess.Write, FileShare.Read);
                using var sw = new StreamWriter(fs) { AutoFlush = true };
                while (!token.IsCancellationRequested)
                {
                    var logItem = await logQueue.ReceiveAsync(token).ConfigureAwait(false);
                    string formattedMessage = FormatLogItem(logItem);
                    await sw.WriteLineAsync(formattedMessage).ConfigureAwait(false);
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

        ~TextLogger()
        {
            Dispose(false);
        }
    }
}