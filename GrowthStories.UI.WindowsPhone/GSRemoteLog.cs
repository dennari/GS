﻿using EventStore.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using Windows.Networking.Sockets;
using Windows.Networking;
using Windows.Storage.Streams;
using System.Diagnostics;
using ReactiveUI;

namespace Growthstories.UI.WindowsPhone
{



    public class GSRemoteLog : ILog, ILogger
    {
        private readonly Type Type;
        public static int Port;
        public static string Host;
        private readonly string StreamName = "GSStream";
        private readonly string NodeName = "GSNode";
        private readonly int TimeOut = 500;

        private readonly static StreamSocket Socket = new StreamSocket();
        private static DataWriter Writer;
        private static bool Try = true;
        private static bool IsConnected = false;

        public GSRemoteLog(Type type = null)
        {
            this.Type = type;

        }

        private void Send(string level, string message, params object[] values)
        {

            var msg = string.Format("+log|{0}|{1}|{2}|{4:HH:mm:ss.fff} <{5}>\n{3}\r\n", StreamName, NodeName, level, string.Format(message, values), DateTime.Now, Type == null ? "#" : Type.Name);
            //Byte[] data = System.Text.Encoding.UTF8.GetBytes(msg);

            if (Debugger.IsAttached)
            {
                System.Diagnostics.Debug.WriteLine(msg);
            }
            lock (Socket)
            {
                if (Try)
                {
                    try
                    {
                        if (!IsConnected)
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
                        Try = false;
                        if (Debugger.IsAttached)
                        {
                            System.Diagnostics.Debug.WriteLine("Couldn't connect remote-logger: {0}", e);
                        }
                    }
                }
            }




        }

        public void Write(string message, LogLevel logLevel)
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
    }
}