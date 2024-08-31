using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingEngineServer.Logging
{
    public record LogInfo(LoggingLevels logLevel, string module, string message, DateTime now, int threadID, string threadName);
}
