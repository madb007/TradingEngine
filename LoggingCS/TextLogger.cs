using Microsoft.Extensions.Options;
using System.Runtime.InteropServices.Marshalling;
using TradingEngineServer.Logging;
using LoggingCS.LoggingConfiguration;

namespace LoggingCS
{
    public class TextLogger : AbstractLogger, ITextLogger
    {
        public TextLogger(IOptions<LoggingConfiguration.LoggerConfiguration> loggingConfiguration) : base() {
        
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        protected override void Log(LoggingLevels logLevel, string module, string message)
        {
            throw new NotImplementedException();
        }
    }
}
