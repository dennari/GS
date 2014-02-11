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
using System.Windows.Media.Imaging;
using Microsoft.Phone.Controls;
using System.Windows.Controls;

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

        
        public static void ResetImage(Image i)
        {
            i.CacheMode = null;
            var bitmapImage = i.Source as BitmapImage;
            if (bitmapImage != null)
            {
                bitmapImage.UriSource = null;
                i.Source = null;
            }
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
            using (var image = e.ChosenPhoto)
            {
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
            }

            return null;
        }








        // from http://cbailiss.wordpress.com/2014/01/24/windows-phone-8-longlistselector-memory-leak/

        public static void ClearLongListSelectorDependencyValues(LongListSelector oList)
        {
            ClearValue(oList, LongListSelector.ActualHeightProperty);
            ClearValue(oList, LongListSelector.ActualWidthProperty);
            ClearValue(oList, LongListSelector.AllowDropProperty);
            ClearValue(oList, LongListSelector.BackgroundProperty);
            ClearValue(oList, LongListSelector.BorderBrushProperty);
            ClearValue(oList, LongListSelector.BorderThicknessProperty);
            ClearValue(oList, LongListSelector.CacheModeProperty);
            ClearValue(oList, LongListSelector.CharacterSpacingProperty);
            ClearValue(oList, LongListSelector.ClipProperty);
            ClearValue(oList, LongListSelector.CursorProperty);
            ClearValue(oList, LongListSelector.DataContextProperty);
            //ClearValue(oList, LongListSelector.DefaultStyleKeyProperty);
            ClearValue(oList, LongListSelector.FlowDirectionProperty);
            ClearValue(oList, LongListSelector.FontFamilyProperty);
            ClearValue(oList, LongListSelector.FontSizeProperty);
            ClearValue(oList, LongListSelector.FontStretchProperty);
            ClearValue(oList, LongListSelector.FontStyleProperty);
            ClearValue(oList, LongListSelector.FontWeightProperty);
            ClearValue(oList, LongListSelector.ForegroundProperty);
            ClearValue(oList, LongListSelector.GridCellSizeProperty);
            ClearValue(oList, LongListSelector.GroupFooterTemplateProperty);
            ClearValue(oList, LongListSelector.GroupHeaderTemplateProperty);
            ClearValue(oList, LongListSelector.HeightProperty);
            ClearValue(oList, LongListSelector.HideEmptyGroupsProperty);
            ClearValue(oList, LongListSelector.HorizontalAlignmentProperty);
            ClearValue(oList, LongListSelector.HorizontalContentAlignmentProperty);
            ClearValue(oList, LongListSelector.IsEnabledProperty);
            ClearValue(oList, LongListSelector.IsGroupingEnabledProperty);
            ClearValue(oList, LongListSelector.IsHitTestVisibleProperty);
            ClearValue(oList, LongListSelector.IsTabStopProperty);
            ClearValue(oList, LongListSelector.ItemsSourceProperty);
            ClearValue(oList, LongListSelector.ItemTemplateProperty);
            ClearValue(oList, LongListSelector.JumpListStyleProperty);
            ClearValue(oList, LongListSelector.LanguageProperty);
            ClearValue(oList, LongListSelector.ListFooterProperty);
            ClearValue(oList, LongListSelector.ListFooterTemplateProperty);
            ClearValue(oList, LongListSelector.ListHeaderProperty);
            ClearValue(oList, LongListSelector.ListHeaderTemplateProperty);
            ClearValue(oList, LongListSelector.MarginProperty);
            ClearValue(oList, LongListSelector.MaxHeightProperty);
            ClearValue(oList, LongListSelector.MaxWidthProperty);
            ClearValue(oList, LongListSelector.MinHeightProperty);
            ClearValue(oList, LongListSelector.MinWidthProperty);
            //ClearValue(oList, LongListSelector.NameProperty);
            ClearValue(oList, LongListSelector.OpacityMaskProperty);
            ClearValue(oList, LongListSelector.OpacityProperty);
            ClearValue(oList, LongListSelector.PaddingProperty);
            ClearValue(oList, LongListSelector.ProjectionProperty);
            ClearValue(oList, LongListSelector.RenderTransformOriginProperty);
            ClearValue(oList, LongListSelector.RenderTransformProperty);
            ClearValue(oList, LongListSelector.StyleProperty);
            ClearValue(oList, LongListSelector.TabIndexProperty);
            ClearValue(oList, LongListSelector.TabNavigationProperty);
            ClearValue(oList, LongListSelector.TagProperty);
            ClearValue(oList, LongListSelector.TemplateProperty);
            ClearValue(oList, LongListSelector.UseLayoutRoundingProperty);
            ClearValue(oList, LongListSelector.UseOptimizedManipulationRoutingProperty);
            ClearValue(oList, LongListSelector.VerticalAlignmentProperty);
            ClearValue(oList, LongListSelector.VerticalContentAlignmentProperty);
            ClearValue(oList, LongListSelector.VisibilityProperty);
            ClearValue(oList, LongListSelector.WidthProperty);

            //oList.ClearValue(LongListSelector.DoubleTapEvent);
            //oList.ClearValue(LongListSelector.HoldEvent);
            //oList.ClearValue(LongListSelector.KeyDownEvent);
            //oList.ClearValue(LongListSelector.KeyUpEvent);
            //oList.ClearValue(LongListSelector.LoadedEvent);
            //oList.ClearValue(LongListSelector.ManipulationCompletedEvent);
            //oList.ClearValue(LongListSelector.ManipulationDeltaEvent);
            //oList.ClearValue(LongListSelector.ManipulationStartedEvent);
            //oList.ClearValue(LongListSelector.MouseLeftButtonDownEvent);
            //oList.ClearValue(LongListSelector.MouseLeftButtonUpEvent);
            //oList.ClearValue(LongListSelector.TapEvent);
            //oList.ClearValue(LongListSelector.TextInputEvent);
            //oList.ClearValue(LongListSelector.TextInputStartEvent);
            //oList.ClearValue(LongListSelector.TextInputUpdateEvent);
        }



        public static void ClearLongListMultiSelectorDependencyValues(LongListMultiSelector oList)
        {
            ClearValue(oList, LongListMultiSelector.ActualHeightProperty);
            ClearValue(oList, LongListMultiSelector.ActualWidthProperty);
            ClearValue(oList, LongListMultiSelector.AllowDropProperty);
            ClearValue(oList, LongListMultiSelector.BackgroundProperty);
            ClearValue(oList, LongListMultiSelector.BorderBrushProperty);
            ClearValue(oList, LongListMultiSelector.BorderThicknessProperty);
            ClearValue(oList, LongListMultiSelector.CacheModeProperty);
            ClearValue(oList, LongListMultiSelector.CharacterSpacingProperty);
            ClearValue(oList, LongListMultiSelector.ClipProperty);
            ClearValue(oList, LongListMultiSelector.CursorProperty);
            ClearValue(oList, LongListMultiSelector.DataContextProperty);
            //ClearValue(oList, LongListMultiSelector.DefaultStyleKeyProperty);
            ClearValue(oList, LongListMultiSelector.FlowDirectionProperty);
            ClearValue(oList, LongListMultiSelector.FontFamilyProperty);
            ClearValue(oList, LongListMultiSelector.FontSizeProperty);
            ClearValue(oList, LongListMultiSelector.FontStretchProperty);
            ClearValue(oList, LongListMultiSelector.FontStyleProperty);
            ClearValue(oList, LongListMultiSelector.FontWeightProperty);
            ClearValue(oList, LongListMultiSelector.ForegroundProperty);
            ClearValue(oList, LongListMultiSelector.GridCellSizeProperty);
            ClearValue(oList, LongListMultiSelector.GroupFooterTemplateProperty);
            ClearValue(oList, LongListMultiSelector.GroupHeaderTemplateProperty);
            ClearValue(oList, LongListMultiSelector.HeightProperty);
            ClearValue(oList, LongListMultiSelector.HideEmptyGroupsProperty);
            ClearValue(oList, LongListMultiSelector.HorizontalAlignmentProperty);
            ClearValue(oList, LongListMultiSelector.HorizontalContentAlignmentProperty);
            ClearValue(oList, LongListMultiSelector.IsEnabledProperty);
            ClearValue(oList, LongListMultiSelector.IsGroupingEnabledProperty);
            ClearValue(oList, LongListMultiSelector.IsHitTestVisibleProperty);
            ClearValue(oList, LongListMultiSelector.IsTabStopProperty);
            ClearValue(oList, LongListMultiSelector.ItemsSourceProperty);
            ClearValue(oList, LongListMultiSelector.ItemTemplateProperty);
            ClearValue(oList, LongListMultiSelector.JumpListStyleProperty);
            ClearValue(oList, LongListMultiSelector.LanguageProperty);
            ClearValue(oList, LongListMultiSelector.ListFooterProperty);
            ClearValue(oList, LongListMultiSelector.ListFooterTemplateProperty);
            ClearValue(oList, LongListMultiSelector.ListHeaderProperty);
            ClearValue(oList, LongListMultiSelector.ListHeaderTemplateProperty);
            ClearValue(oList, LongListMultiSelector.MarginProperty);
            ClearValue(oList, LongListMultiSelector.MaxHeightProperty);
            ClearValue(oList, LongListMultiSelector.MaxWidthProperty);
            ClearValue(oList, LongListMultiSelector.MinHeightProperty);
            ClearValue(oList, LongListMultiSelector.MinWidthProperty);
            //ClearValue(oList, LongListMultiSelector.NameProperty);
            ClearValue(oList, LongListMultiSelector.OpacityMaskProperty);
            ClearValue(oList, LongListMultiSelector.OpacityProperty);
            ClearValue(oList, LongListMultiSelector.PaddingProperty);
            ClearValue(oList, LongListMultiSelector.ProjectionProperty);
            ClearValue(oList, LongListMultiSelector.RenderTransformOriginProperty);
            ClearValue(oList, LongListMultiSelector.RenderTransformProperty);
            ClearValue(oList, LongListMultiSelector.StyleProperty);
            ClearValue(oList, LongListMultiSelector.TabIndexProperty);
            ClearValue(oList, LongListMultiSelector.TabNavigationProperty);
            ClearValue(oList, LongListMultiSelector.TagProperty);
            ClearValue(oList, LongListMultiSelector.TemplateProperty);
            ClearValue(oList, LongListMultiSelector.UseLayoutRoundingProperty);
            ClearValue(oList, LongListMultiSelector.UseOptimizedManipulationRoutingProperty);
            ClearValue(oList, LongListMultiSelector.VerticalAlignmentProperty);
            ClearValue(oList, LongListMultiSelector.VerticalContentAlignmentProperty);
            ClearValue(oList, LongListMultiSelector.VisibilityProperty);
            ClearValue(oList, LongListMultiSelector.WidthProperty);

            ClearValue(oList, LongListMultiSelector.EnforceIsSelectionEnabledProperty);
            ClearValue(oList, LongListMultiSelector.SelectedItemsProperty);
            ClearValue(oList, LongListMultiSelector.IsSelectionEnabledProperty);
        }

        public static void ClearPivotItemDependencyValues(PivotItem oPivotItem)
        {
            ClearValue(oPivotItem, PivotItem.ActualHeightProperty);
            ClearValue(oPivotItem, PivotItem.ActualWidthProperty);
            ClearValue(oPivotItem, PivotItem.AllowDropProperty);
            ClearValue(oPivotItem, PivotItem.BackgroundProperty);
            ClearValue(oPivotItem, PivotItem.BorderBrushProperty);
            ClearValue(oPivotItem, PivotItem.BorderThicknessProperty);
            ClearValue(oPivotItem, PivotItem.CacheModeProperty);
            ClearValue(oPivotItem, PivotItem.CharacterSpacingProperty);
            ClearValue(oPivotItem, PivotItem.ClipProperty);
            ClearValue(oPivotItem, PivotItem.ContentProperty);
            ClearValue(oPivotItem, PivotItem.ContentTemplateProperty);
            ClearValue(oPivotItem, PivotItem.CursorProperty);
            ClearValue(oPivotItem, PivotItem.DataContextProperty);
            //ClearValue(oPivotItem, PivotItem.DefaultStyleKeyProperty);
            ClearValue(oPivotItem, PivotItem.FlowDirectionProperty);
            ClearValue(oPivotItem, PivotItem.FontFamilyProperty);
            ClearValue(oPivotItem, PivotItem.FontSizeProperty);
            ClearValue(oPivotItem, PivotItem.FontStretchProperty);
            ClearValue(oPivotItem, PivotItem.FontStyleProperty);
            ClearValue(oPivotItem, PivotItem.FontWeightProperty);
            ClearValue(oPivotItem, PivotItem.ForegroundProperty);
            ClearValue(oPivotItem, PivotItem.HeaderProperty);
            ClearValue(oPivotItem, PivotItem.HeightProperty);
            ClearValue(oPivotItem, PivotItem.HorizontalAlignmentProperty);
            ClearValue(oPivotItem, PivotItem.HorizontalContentAlignmentProperty);
            ClearValue(oPivotItem, PivotItem.IsEnabledProperty);
            ClearValue(oPivotItem, PivotItem.IsHitTestVisibleProperty);
            ClearValue(oPivotItem, PivotItem.IsTabStopProperty);
            ClearValue(oPivotItem, PivotItem.LanguageProperty);
            ClearValue(oPivotItem, PivotItem.MarginProperty);
            ClearValue(oPivotItem, PivotItem.MaxHeightProperty);
            ClearValue(oPivotItem, PivotItem.MaxWidthProperty);
            ClearValue(oPivotItem, PivotItem.MinHeightProperty);
            ClearValue(oPivotItem, PivotItem.MinWidthProperty);
            //ClearValue(oPivotItem, PivotItem.NameProperty);
            ClearValue(oPivotItem, PivotItem.OpacityMaskProperty);
            ClearValue(oPivotItem, PivotItem.OpacityProperty);
            ClearValue(oPivotItem, PivotItem.PaddingProperty);
            ClearValue(oPivotItem, PivotItem.ProjectionProperty);
            ClearValue(oPivotItem, PivotItem.RenderTransformOriginProperty);
            ClearValue(oPivotItem, PivotItem.RenderTransformProperty);
            ClearValue(oPivotItem, PivotItem.StyleProperty);
            ClearValue(oPivotItem, PivotItem.TabIndexProperty);
            ClearValue(oPivotItem, PivotItem.TabNavigationProperty);
            ClearValue(oPivotItem, PivotItem.TagProperty);
            ClearValue(oPivotItem, PivotItem.TemplateProperty);
            ClearValue(oPivotItem, PivotItem.UseLayoutRoundingProperty);
            ClearValue(oPivotItem, PivotItem.UseOptimizedManipulationRoutingProperty);
            ClearValue(oPivotItem, PivotItem.VerticalAlignmentProperty);
            ClearValue(oPivotItem, PivotItem.VerticalContentAlignmentProperty);
            ClearValue(oPivotItem, PivotItem.VisibilityProperty);
            ClearValue(oPivotItem, PivotItem.WidthProperty);
        }

        public static void ClearPivotDependencyValues(Pivot oPivot)
        {
            foreach (PivotItem oPivotItem in oPivot.Items)
            {
                ClearPivotItemDependencyValues(oPivotItem);
            }
            ClearValue(oPivot, Pivot.ActualHeightProperty);
            ClearValue(oPivot, Pivot.ActualWidthProperty);
            ClearValue(oPivot, Pivot.AllowDropProperty);
            ClearValue(oPivot, Pivot.BackgroundProperty);
            ClearValue(oPivot, Pivot.BorderBrushProperty);
            ClearValue(oPivot, Pivot.BorderThicknessProperty);
            ClearValue(oPivot, Pivot.CacheModeProperty);
            ClearValue(oPivot, Pivot.CharacterSpacingProperty);
            ClearValue(oPivot, Pivot.ClipProperty);
            ClearValue(oPivot, Pivot.CursorProperty);
            ClearValue(oPivot, Pivot.DataContextProperty);
            //ClearValue(oPivot, Pivot.DefaultStyleKeyProperty);
            ClearValue(oPivot, Pivot.DisplayMemberPathProperty);
            ClearValue(oPivot, Pivot.FlowDirectionProperty);
            ClearValue(oPivot, Pivot.FontFamilyProperty);
            ClearValue(oPivot, Pivot.FontSizeProperty);
            ClearValue(oPivot, Pivot.FontStretchProperty);
            ClearValue(oPivot, Pivot.FontStyleProperty);
            ClearValue(oPivot, Pivot.FontWeightProperty);
            ClearValue(oPivot, Pivot.ForegroundProperty);
            ClearValue(oPivot, Pivot.HeaderTemplateProperty);
            ClearValue(oPivot, Pivot.HeightProperty);
            ClearValue(oPivot, Pivot.HorizontalAlignmentProperty);
            ClearValue(oPivot, Pivot.HorizontalContentAlignmentProperty);
            ClearValue(oPivot, Pivot.IsEnabledProperty);
            ClearValue(oPivot, Pivot.IsHitTestVisibleProperty);
            ClearValue(oPivot, Pivot.IsTabStopProperty);
            ClearValue(oPivot, Pivot.ItemContainerStyleProperty);
            ClearValue(oPivot, Pivot.ItemsPanelProperty);
            ClearValue(oPivot, Pivot.ItemsSourceProperty);
            ClearValue(oPivot, Pivot.ItemTemplateProperty);
            ClearValue(oPivot, Pivot.LanguageProperty);
            ClearValue(oPivot, Pivot.MarginProperty);
            ClearValue(oPivot, Pivot.MaxHeightProperty);
            ClearValue(oPivot, Pivot.MaxWidthProperty);
            ClearValue(oPivot, Pivot.MinHeightProperty);
            ClearValue(oPivot, Pivot.MinWidthProperty);
            //ClearValue(oPivot, Pivot.NameProperty);
            ClearValue(oPivot, Pivot.OpacityMaskProperty);
            ClearValue(oPivot, Pivot.OpacityProperty);
            ClearValue(oPivot, Pivot.PaddingProperty);
            ClearValue(oPivot, Pivot.ProjectionProperty);
            ClearValue(oPivot, Pivot.RenderTransformOriginProperty);
            ClearValue(oPivot, Pivot.RenderTransformProperty);
            ClearValue(oPivot, Pivot.SelectedIndexProperty);
            //ClearValue(oPivot, Pivot.SelectedItemProperty);
            ClearValue(oPivot, Pivot.StyleProperty);
            ClearValue(oPivot, Pivot.TabIndexProperty);
            ClearValue(oPivot, Pivot.TabNavigationProperty);
            ClearValue(oPivot, Pivot.TagProperty);
            ClearValue(oPivot, Pivot.TemplateProperty);
            ClearValue(oPivot, Pivot.TitleProperty);
        }


        private static void ClearValue(DependencyObject oObject, DependencyProperty oProperty)
        {
            object localValue = oObject.ReadLocalValue(oProperty);
            if (localValue != DependencyProperty.UnsetValue) oObject.ClearValue(oProperty);
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
