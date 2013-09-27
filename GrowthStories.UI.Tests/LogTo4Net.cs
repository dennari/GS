using EventStore.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;

namespace Growthstories.DomainTests
{
    class LogToNLog : EventStore.Logging.ILog
    {
        private Type type;
        private readonly Logger Logger;

        public LogToNLog(Type type)
        {
            // TODO: Complete member initialization
            this.type = type;
            this.Logger = LogManager.GetLogger(type.Name);
        }

        public void Verbose(string message, params object[] values)
        {
            Logger.Debug(message, values);
        }

        public void Debug(string message, params object[] values)
        {

            Logger.Debug(message, values);
        }

        public void Info(string message, params object[] values)
        {

            Logger.Info(message, values);

        }

        public void Warn(string message, params object[] values)
        {
            Logger.Warn(message, values);

        }

        public void Error(string message, params object[] values)
        {
            Logger.Error(message, values);

        }

        public void Fatal(string message, params object[] values)
        {
            Logger.Fatal(message, values);

        }
    }
}
