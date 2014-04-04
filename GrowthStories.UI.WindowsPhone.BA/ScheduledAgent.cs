using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.Scheduler;
using System;
using System.IO.IsolatedStorage;
using System.IO;
using Growthstories.Core;

namespace GrowthStories.UI.WindowsPhone.BA
{

    public class BALog
    {

        public static void Log(string msg)
        {
            var LogFile = "baLog.txt";

            DateTime now = DateTime.Now;
            string time = now.ToString("G");

            try
            {
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream fs = storage.OpenFile(LogFile, FileMode.Append))
                    {
                        using (StreamWriter w = new StreamWriter(fs))
                        {
                            w.Write(time + " " + msg + "\n");
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

    }


    public class ScheduledAgent : ScheduledTaskAgent
    {



        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        static ScheduledAgent()
        {
            // Subscribe to the managed exception handler
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                Application.Current.UnhandledException += UnhandledException;
            });
        }

        /// Code to execute on Unhandled Exceptions
        private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive task is invoked
        /// </remarks>
        protected override void OnInvoke(ScheduledTask task)
        {
            BALog.Log("running background agent");
            
            try {
                GSTileUtils.UpdateTiles();

                BALog.Log("clean finish for background agent");
                NotifyComplete();

            } catch (Exception e) {
                if (Debugger.IsAttached) { Debugger.Break(); }
                BALog.Log(String.Format("dirty finish for background agent: {0}", e.ToStringExtended()));
                Abort();
            }

        }



    }

}