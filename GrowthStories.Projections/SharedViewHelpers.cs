using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.UI
{

    public class SharedViewHelpers
    {

        public static string FormatWeekDay(DateTimeOffset offset)
        {
            if (offset == null)
            {
                return "";
            }
            return offset.DayOfWeek.ToString().ToLower();
        }
    }

    
}
