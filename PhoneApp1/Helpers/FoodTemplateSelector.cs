using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using PhoneApp1;

namespace PhoneApp1.Helpers
{
    public class FoodTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Healthy
        {
            get;
            set;
        }

        public DataTemplate UnHealthy
        {
            get;
            set;
        }

        public DataTemplate NotDetermined
        {
            get;
            set;
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            Data foodItem = item as Data;
            if (foodItem != null)
            {
                if (foodItem.Type == "Healthy")
                {
                    return Healthy;
                }
                else if (foodItem.Type == "NotDetermined")
                {
                    return NotDetermined;
                }
                else
                {
                    return UnHealthy;
                }
            }

            return base.SelectTemplate(item, container);
        }
    }

}
