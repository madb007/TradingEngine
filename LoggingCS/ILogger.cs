using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoggingCS
{
    public interface ILogger
    {
        void Debug(string module, string message);
        void Debug(string module, Exception exception);
        void Info(string module, string message);
        void Info(string module, Exception exception);
        void Warn(string module, string message);
        void Warn(string module, Exception exception);
        void Error(string module, string message);
        void Error(string module, Exception exception);

    }
}
