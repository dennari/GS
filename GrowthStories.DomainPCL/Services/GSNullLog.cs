using Growthstories.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Domain.Services
{
    public class GSNullLog : IGSLog, ILogger
    {

        public void Exception(Exception e, string message = null, params object[] values)
        {
        }

        public void Write(string message, GSLogLevel logLevel)
        {
        }

        public GSLogLevel Level { get; set; }


        public void Debug(string message, params object[] values)
        {
        }

        public void Error(string message, params object[] values)
        {
        }

        public void Fatal(string message, params object[] values)
        {
        }

        public void Info(string message, params object[] values)
        {
        }

        public void Verbose(string message, params object[] values)
        {
        }

        public void Warn(string message, params object[] values)
        {
        }

        public void Write(string message, LogLevel logLevel)
        {
        }

        LogLevel ILogger.Level { get; set; }

        private static GSNullLog _Instance;

        public static GSNullLog Instance
        {
            get
            {
                return _Instance ?? (_Instance = new GSNullLog());
            }
        }
    }
}
