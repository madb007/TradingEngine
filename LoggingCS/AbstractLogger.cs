using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingEngineServer.Logging;

namespace LoggingCS
{
    abstract class AbstractLogger : ILogger
    {
        protected AbstractLogger()
        {

        }

        public void Debug(string module, string message) => Log(LoggingLevels.Debug, module, message);

        public void Debug(string module, Exception exception) => Log(LoggingLevels.Debug, module, $"{exception}");

        public void Error(string module, string message) => Log(LoggingLevels.Error, module, message);

        public void Error(string module, Exception exception) => Log(LoggingLevels.Error, module, $"{exception}");

        public void Info(string module, string message) => Log(LoggingLevels.Info, module, message);

        public void Info(string module, Exception exception) => Log(LoggingLevels.Info, module, $"{exception}");

        public void Warn(string module, string message) => Log(LoggingLevels.Warning, module, message);

        public void Warn(string module, Exception exception) => Log(LoggingLevels.Warning, module, $"{exception}");

        protected abstract void Log(LoggingLevels logLevel, string module, string message);
    }
}
