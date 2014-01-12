using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Globalization;
using Growthstories.UI.ViewModel;
using GrowthStories.UI.WindowsPhone.BA;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Scheduler;


namespace Growthstories.UI.WindowsPhone
{



    public static class ViewHelpers
    {


        private static Regex _hexColorMatchRegex = new Regex("^#?(?<a>[a-z0-9][a-z0-9])?(?<r>[a-z0-9][a-z0-9])(?<g>[a-z0-9][a-z0-9])(?<b>[a-z0-9][a-z0-9])$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public static Color GetColorFromHex(string hexColorString)
        {
            if (hexColorString == null)
                throw new NullReferenceException("Hex string can't be null.");

            // Regex match the string
            var match = _hexColorMatchRegex.Match(hexColorString);

            if (!match.Success)
                throw new InvalidCastException(string.Format("Can't convert string \"{0}\" to argb or rgb color. Needs to be 6 (rgb) or 8 (argb) hex characters long. It can optionally start with a #.", hexColorString));

            // a value is optional
            byte a = 255, r = 0, b = 0, g = 0;
            if (match.Groups["a"].Success)
                a = System.Convert.ToByte(match.Groups["a"].Value, 16);
            // r,b,g values are not optional
            r = System.Convert.ToByte(match.Groups["r"].Value, 16);
            b = System.Convert.ToByte(match.Groups["b"].Value, 16);
            g = System.Convert.ToByte(match.Groups["g"].Value, 16);
            return Color.FromArgb(a, r, b, g);
        }

        public static Color ToColor(this string This)
        {
            return GetColorFromHex(This);
        }



        public static void ViewModelValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                var view = (FrameworkElement)sender;
                var vm = e.NewValue;

                if (view.DataContext != vm)
                    view.DataContext = vm;

                var viewF = view as IReportViewModelChange;
                if (viewF != null)
                    viewF.ViewModelChangeReport(vm);


            }
            catch { }
        }
    }


    public class BAUtils
    {

        public const string TASK_NAME = "tileupdate";


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
            ScheduledAction task = ScheduledActionService.Find(TASK_NAME);
            if (task != null)
            {
                ScheduledActionService.Remove(TASK_NAME);
            }

            task = CreateTask();

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


    public class GSMainProgramTileUtils
    {


        /*
         * Referencing ShellTile.Create is not allowed inside code
         * within a background agent, so we have this separately here
         */
        public static void CreateOrUpdateTile(IPlantViewModel pvm)
        {

            var tile = GSTileUtils.GetShellTile(pvm);
            if (tile != null) {
                GSTileUtils.UpdateTileAndInfoAfterDelay(pvm);

            } else {
                var info = GSTileUtils.CreateTileUpdateInfo(pvm);
                ShellTile.Create(new Uri(info.UrlPath, UriKind.Relative), GSTileUtils.GetTileData(info), true);
                GSTileUtils.WriteTileUpdateInfo(info);

            }

        }

    }
}
