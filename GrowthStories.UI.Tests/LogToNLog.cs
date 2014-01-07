using EventStore.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace Growthstories.DomainTests
{
    class LogTo4Net : EventStore.Logging.ILog
    {
        private Type type;
        private readonly log4net.ILog Logger;

        public LogTo4Net(Type type)
        {
            // TODO: Complete member initialization
            this.type = type;
            this.Logger = LogManager.GetLogger(type);
        }

        public void Verbose(string message, params object[] values)
        {
            if (values.Length == 0)
                Logger.Debug(message);
            else
                Logger.DebugFormat(message, values);
        }

        public void Debug(string message, params object[] values)
        {
            if (values.Length == 0)
                Logger.Debug(message);
            else
                Logger.DebugFormat(message, values);
        }

        public void Info(string message, params object[] values)
        {
            if (values.Length == 0)
                Logger.Info(message);
            else
                Logger.InfoFormat(message, values);

        }

        public void Warn(string message, params object[] values)
        {
            Logger.WarnFormat(message, values);

        }

        public void Error(string message, params object[] values)
        {
            Logger.ErrorFormat(message, values);

        }

        public void Fatal(string message, params object[] values)
        {
            Logger.FatalFormat(message, values);

        }
    }
}
