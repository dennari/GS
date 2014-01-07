using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.Scheduler;
using System;


namespace GrowthStories.UI.WindowsPhone.BA
{


    public class ScheduledAgent : ScheduledTaskAgent
    {

        public const string TASK_NAME = "tileupdate";


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
            
            try {
                GSTileUtils.UpdateTiles();
                NotifyComplete();

            } catch {
                if (Debugger.IsAttached) { Debugger.Break(); }
                Abort();
            }
            
        }


        public static PeriodicTask CreateTask()
        {
            var task = new PeriodicTask(TASK_NAME);
            task.Description = "Updates watering notifications for plant tiles";

            return task;
        }


        public static void RegisterScheduledTask()
        {

            // If the task already exists and background agents are enabled for the
            // application, we must remove the task and then add it again to update 
            // the schedule.
            ScheduledAction task = ScheduledActionService.Find(ScheduledAgent.TASK_NAME);
            if (task != null)
            {
                ScheduledActionService.Remove(ScheduledAgent.TASK_NAME);
            }

            task = ScheduledAgent.CreateTask();

            try
            {
                ScheduledActionService.Add(task);
            }

            catch (InvalidOperationException exception)
            {
                if (exception.Message.Contains("BNS Error: The action is disabled"))
                {
                    // means that user has disabled background agents for this app
                }
                if (exception.Message.Contains("BNS Error: The maximum number of ScheduledActions of this type have already been added."))
                {
                    // global count for scheduledactions is too large, user should disable
                    // background agents for less cool applications
                }

            }
            catch (SchedulerServiceException)
            {
                // unclear when this happens
            }

        }


    }

}