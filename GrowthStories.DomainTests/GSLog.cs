using EventStore.Logging;
using ReflectSoftware.Insight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Growthstories.DomainTests
{
    class GSLog : ILog
    {
        private Type type;
        public static readonly ReflectInsight Logger = new ReflectInsight();

        public GSLog(Type type)
        {
            // TODO: Complete member initialization
            this.type = type;
            //this.Logger = new ReflectInsight(type.Name);
        }

        private Tuple<string, object[]> BeforeLog(string message, params object[] values)
        {
            Logger.Category = type.Name;
            return Tuple.Create(message, values);
        }

        public void Verbose(string message, params object[] values)
        {
            var p = BeforeLog(message, values);
            Logger.SendVerbose(p.Item1, p.Item2);
        }

        public void Debug(string message, params object[] values)
        {
            var p = BeforeLog(message, values);
            Logger.SendDebug(p.Item1, p.Item2);

        }

        public void Info(string message, params object[] values)
        {
            var p = BeforeLog(message, values);
            Logger.SendInformation(p.Item1, p.Item2);


        }

        public void Warn(string message, params object[] values)
        {
            var p = BeforeLog(message, values);
            Logger.SendWarning(p.Item1, p.Item2);


        }

        public void Error(string message, params object[] values)
        {
            var p = BeforeLog(message, values);
            Logger.SendError(p.Item1, p.Item2);


        }

        public void Fatal(string message, params object[] values)
        {
            var p = BeforeLog(message, values);
            Logger.SendFatal(p.Item1, p.Item2);


        }
    }
}
