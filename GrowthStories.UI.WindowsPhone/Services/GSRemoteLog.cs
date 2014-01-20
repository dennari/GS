using Growthstories.Core;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace Growthstories.UI.WindowsPhone
{



    public class GSRemoteLog : IGSLog, ILogger
    {
        private readonly Type Type;
        public static int Port;
        public static string Host;
        private readonly string StreamName = "GSStream";
        private readonly string NodeName = "GSNode";
        private readonly int TimeOut = 2500;

        private readonly static StreamSocket Socket = new StreamSocket();
        private static DataWriter Writer;
        private static int Tried = 0;
        private static int MaxTries = 10;
        private static bool IsConnected = false;


        public GSRemoteLog(Type type = null)
        {
            this.Type = type;

        }

        private void Send(string level, string message, params object[] values)
        {

            // useful to do this check before doing heavy string operations
            if (Tried >= MaxTries)
            {
                return;
            }

            if (this.Type != null &&  this.Type.Name != null)
            {
                switch (Type.Name)
                {
                    case "JsonSerializer":
                    case "OptimisticEventStream":
                    case "OptimisticPipelineHook":
                    case "SQLiteUIPersistence":
                        return;
                }
            }

            if (message.StartsWith("ReactiveObject") 
                || message.StartsWith("MemoizingMRUCache") 
                || message.StartsWith("MessageBus")
                || message.StartsWith("ObservableAsPropertyHelper")
                || message.StartsWith("LogHost"))
            {
                return;
            }

            string content = string.Format(message, values).Replace("\r\n", "\n");
            var msg = string.Format("+log|{0}|{1}|{2}|{4:HH:mm:ss.fff} <{5}>\n{3}\r\n", StreamName, NodeName, level, content, DateTime.Now, Type == null ? "#" : Type.Name);
            //Byte[] data = System.Text.Encoding.UTF8.GetBytes(msg);

            //if (Debugger.IsAttached)
            //{
            //    System.Diagnostics.Debug.WriteLine(msg);
            //}

            if (Tried < MaxTries)
            {
                lock (Socket)
                {

                    try
                    {
                        if (!IsConnected || Socket.Information.RemoteHostName == null)
                        {
                            Socket.Control.KeepAlive = true;
                            Socket.ConnectAsync(new HostName(Host), Port.ToString()).AsTask().Wait(TimeOut);
                            Writer = new DataWriter(Socket.OutputStream);
                            IsConnected = true;
                        }

                        Writer.WriteString(msg);
                        Writer.StoreAsync().AsTask().Wait(TimeOut);
                    }
                    catch (Exception e)
                    {
                        Tried++;
                        if (Debugger.IsAttached)
                        {
                            System.Diagnostics.Debug.WriteLine("Couldn't connect remote-logger: {0}", e);
                        }
                    }
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
