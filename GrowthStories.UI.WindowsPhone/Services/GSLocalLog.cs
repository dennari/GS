using System;
using System.Diagnostics;
using System.Net.Sockets;
using Growthstories.Core;
using ReactiveUI;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.IO.IsolatedStorage;
using System.IO;


namespace Growthstories.UI.WindowsPhone
{



    public class GSLocalLog : IGSLog, ILogger
    {
        private readonly Type Type;
        public static int Port;
        public static string Host;
        private readonly string StreamName = "GSStream";
        private readonly string NodeName = "GSNode";
        private readonly int TimeOut = 2500;
        private Func<Type, string, bool> Filter;

        private readonly static StreamSocket Socket = new StreamSocket();
        private static DataWriter Writer;
        private static int Tried = 0;
        private static int MaxTries = 10;
        private static bool IsConnected = false;


        public GSLocalLog(Type type = null, Func<Type, string, bool> filter = null)
        {
            this.Type = type;
            this.Filter = filter;
        }


        private void Send(string level, string message, params object[] values)
        {
            if (Filter != null && !Filter(Type, message))
                return;

            string content = message;
            try
            {
                content = string.Format(message, values).Replace("\r\n", "\n");
            }
            catch { }

            var msg = string.Format("+log|{0}|{1}|{2}|{4:HH:mm:ss.fff} <{5}>\n{3}\r\n", StreamName, NodeName, level, content, DateTime.Now, Type == null ? "#" : Type.Name);
            var LogFile = "localLog.txt";
            try
            {
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream fs = storage.OpenFile(LogFile, FileMode.Append))
                    {
                        using (StreamWriter w = new StreamWriter(fs))
                        {
                            w.Write(msg);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }

        }



        public void Exception(Exception e, string message = null, params object[] values)
        {
            if (message == null)
                Send("info", "{0}", e.ToStringExtended());
            else
                Send("info", "{0}: {1}", string.Format(message, values), e.ToStringExtended());

        }

        public void Write(string message, LogLevel logLevel)
        {
            if ((int)logLevel < (int)Level) return;
            Send("info", message);
        }

        public void Write(string message, GSLogLevel logLevel)
        {
            if ((int)logLevel < (int)Level) return;
            Send("info", message);
        }
        public LogLevel Level { get; set; }

        public void Verbose(string message, params object[] values)
        {
            Send("verbose", message, values);
        }

        public void Debug(string message, params object[] values)
        {
            Send("debug", message, values);
        }

        public void Info(string message, params object[] values)
        {
            Send("info", message, values);

        }

        public void Warn(string message, params object[] values)
        {
            Send("warn", message, values);

        }

        public void Error(string message, params object[] values)
        {
            Send("error", message, values);

        }

        public void Fatal(string message, params object[] values)
        {
            Send("fatal", message, values);
        }


        private static GSRemoteLog _Instance;

        public static GSRemoteLog Instance
        {
            get
            {
                return _Instance ?? (_Instance = new GSRemoteLog());
            }
        }



        GSLogLevel IGSLog.Level
        {
            get
            {
                return (GSLogLevel)Level;
            }
            set
            {
                Level = (LogLevel)value;
            }
        }
    }


}
