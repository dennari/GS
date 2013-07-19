using EventStore.Logging;
using EventStore.Persistence.SqlPersistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Growthstories.DomainTests
{
    class GSLog : ILog
    {
        private Type type;
        private readonly ILog Logger;

        public GSLog(Type type)
        {
            // TODO: Complete member initialization
            this.type = type;
            this.Logger = new LogTo4Net(type);
        }

        protected string Tag(string m)
        {
            if (type.FullName.Contains("Growthstories") || type == typeof(SQLitePersistenceEngine))
                return "[GS] " + m;
            else
                return m;
        }

        public void Verbose(string message, params object[] values)
        {
            Logger.Verbose(Tag(message), values);
        }

        public void Debug(string message, params object[] values)
        {
            Logger.Debug(Tag(message), values);

        }

        public void Info(string message, params object[] values)
        {
            Logger.Info(Tag(message), values);


        }

        public void Warn(string message, params object[] values)
        {
            Logger.Warn(Tag(message), values);


        }

        public void Error(string message, params object[] values)
        {
            Logger.Error(Tag(message), values);


        }

        public void Fatal(string message, params object[] values)
        {
            Logger.Fatal(Tag(message), values);


        }
    }
}
