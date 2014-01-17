
using EventStore.Logging;
using System;


namespace Growthstories.Core
{

    public enum GSLogLevel
    {
        Debug = 1, Info, Warn, Error, Fatal,
    }

    public interface IGSLog : ILog
    {

        void Exception(Exception e, string message = null, params object[] values);

        void Write(string message, GSLogLevel logLevel);

        GSLogLevel Level { get; set; }

    }

    /// <summary>
    /// "Implement" this interface in your class to get access to the Log() 
    /// Mixin, which will give you a Logger that includes the class name in the
    /// log.
    /// </summary>
    public interface IHasLogger
    {
        IGSLog Logger { get; set; }
    }

    public static class LogMixins
    {
        public static IGSLog GSLog<T>(this T This) where T : IHasLogger
        {
            if (This.Logger == null)
                This.Logger = GSLogFactory.BuildLogger(typeof(T));

            return This.Logger;
        }
    }

    public static class GSLogFactory
    {
        public static Func<Type, IGSLog> BuildLogger { get; set; }
    }




}
