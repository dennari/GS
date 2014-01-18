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
using Microsoft.Phone.Tasks;
using Growthstories.Domain.Entities;
using Growthstories.Sync;


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


        public static async Task<Photo> HandlePhotoChooserCompleted(PhotoResult e, IReactiveCommand ShowPopup)
        {
            var image = e.ChosenPhoto;

            if (e.TaskResult == TaskResult.OK)
            {
                if (image.CanRead && image.Length > 0)
                {
                    return await image.SavePhotoToLocalStorageAsync();
                }
                else
                {
                    // For some reason images renamed by connecting a computer to Windows
                    // causes the e.ChosenPhoto to contain an empty stream.
                    // 
                    // The official foursquare app and Ilta-Sanomat app crashes when such
                    // a photo is selected via the photo chooser.
                    //
                    // Even the OneNote app fails to add the photo (but does not crash).
                    //
                    // We are handling it better and displaying an error message
                    //
                    //  -- JOJ 18.1.2014
                    //
                    var pvm = new PopupViewModel()
                    {
                        Caption = "Cannot add image",
                        Message = "Growth Stories could not read the image you selected. Please try selecting another one.",
                        IsLeftButtonEnabled = true,
                        LeftButtonContent = "OK"
                    };

                    ShowPopup.Execute(pvm);
                }
            }

            return null;
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


}
