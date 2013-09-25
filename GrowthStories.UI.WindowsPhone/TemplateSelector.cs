

using Growthstories.Domain.Messaging;
using Growthstories.UI.ViewModel;
using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Growthstories.UI.WindowsPhone
{
    public abstract class TemplateSelector : ContentControl
    {
        public abstract DataTemplate SelectTemplate(object item, int index, int totalCount, DependencyObject container);

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            var parent = GetParentByType<LongListSelector>(this);
            var index = parent.ItemsSource.IndexOf(newContent);
            var totalCount = parent.ItemsSource.Count;

            ContentTemplate = SelectTemplate(newContent, index, totalCount, this);
        }

        private static T GetParentByType<T>(DependencyObject element) where T : FrameworkElement
        {
            T result = null;
            DependencyObject parent = VisualTreeHelper.GetParent(element);

            while (parent != null)
            {
                result = parent as T;

                if (result != null)
                {
                    return result;
                }

                parent = VisualTreeHelper.GetParent(parent);
            }

            return null;
        }
    }


    public class ActionTemplateSelector : TemplateSelector
    {

        public override DataTemplate SelectTemplate(object item, int index, int totalCount, DependencyObject container)
        {
            //return Application.Current.Resources["CommentTemplate"] as DataTemplate;
            return GetTemplate((dynamic)item);
        }

        private DataTemplate GetTemplate(IPlantCommentViewModel item)
        {
            return Application.Current.Resources["CommentTemplate"] as DataTemplate;
        }

        private DataTemplate GetTemplate(IPlantWaterViewModel item)
        {
            //return Application.Current.Resources["WaterTemplate"] as DataTemplate;
            return Application.Current.Resources["CommentTemplate"] as DataTemplate;
        }

        private DataTemplate GetTemplate(IPlantFertilizeViewModel item)
        {
            //return Application.Current.Resources["FertilizeTemplate"] as DataTemplate;
            return Application.Current.Resources["CommentTemplate"] as DataTemplate;
        }

        private DataTemplate GetTemplate(IPlantMeasureViewModel item)
        {
            //return Application.Current.Resources["FertilizeTemplate"] as DataTemplate;
            return Application.Current.Resources["MeasurementTemplate"] as DataTemplate;
        }

        private DataTemplate GetTemplate(IPlantPhotographViewModel item)
        {
            return Application.Current.Resources["PhotographTemplate"] as DataTemplate;
        }
    }


}


