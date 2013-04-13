using Growthstories.WP8.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Growthstories.WP8.Helpers
{
    class ActionTemplateSelector : DataTemplateSelector
    {

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item != null)
            {
                if (item is PhotoAction)
                {
                    return this.PhotoActionTemplate;
                }
                else if (item is WateringAction)
                {
                    return this.WateringActionTemplate;
                }
                else if (item is FertilizerAction)
                {
                    return this.FertilizerActionTemplate;
                }
            }

            return base.SelectTemplate(item, container);
        }

        public DataTemplate WateringActionTemplate { get; set; }

        public DataTemplate FertilizerActionTemplate { get; set; }

        public DataTemplate PhotoActionTemplate { get; set; }
    }
}
