using Growthstories.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
//using Growthstories.UI.WindowsPhone.ViewModels;

namespace Growthstories.UI.ViewModel
{


    public sealed class MeasurementTypeHelper
    {
        public MeasurementType Type;
        public string TimelineTitle;
        public string SeriesTitle;
        public string SeriesColor;
        public string Unit;
        public int Decimals;
        public IconType Icon;

        public string FormatValue(double value, bool withUnit = false)
        {
            var v = value.ToString("F" + this.Decimals);
            return withUnit ? v + " " + Unit : v;
        }

        public string TitleWithUnit
        {
            get
            {
                return string.IsNullOrWhiteSpace(Unit) ? SeriesTitle : string.Format("{0} [{1}]", SeriesTitle, Unit);
            }
        }

        public static Dictionary<MeasurementType, MeasurementTypeHelper> Options = new Dictionary<MeasurementType, MeasurementTypeHelper>()
        {
        
            {MeasurementType.LENGTH,new MeasurementTypeHelper() {
                Type = MeasurementType.LENGTH,
                TimelineTitle= "length",
                Icon = IconType.MEASURE,
                SeriesTitle= "Length",
                SeriesColor= "#92b163",
                Unit= "cm",
                Decimals= 1
            }},
            //{MeasurementType.SOIL_HUMIDITY,new MeasurementTypeHelper() {
            //    Type = MeasurementType.SOIL_HUMIDITY,
            //    TimelineTitle= "soil humidity",
            //    Icon = IconType.MEASURE,
            //    SeriesTitle= "Soil humidity",
            //    SeriesColor= "#26a8ba",
            //    Unit= "cm",
            //    Decimals= 1
            //}},
            {MeasurementType.AIR_HUMIDITY,new MeasurementTypeHelper() {
                Type = MeasurementType.AIR_HUMIDITY,
                TimelineTitle= "air humidity",
                Icon = IconType.AIRHUMIDITY,
                SeriesTitle= "Air humidity",
                SeriesColor= "#26a8ba",
                Unit= "%",
                Decimals= 1
            }},
            {MeasurementType.PH,new MeasurementTypeHelper() {
                Type = MeasurementType.PH,
                TimelineTitle= "pH",
                Icon = IconType.PH2,
                SeriesTitle= "pH (acidity)",
                SeriesColor= "#ec9150",
                Unit= "",
                Decimals= 1
            }},
            {MeasurementType.CO2,new MeasurementTypeHelper() {
                Type = MeasurementType.CO2,
                TimelineTitle= "CO2",
                Icon = IconType.CO2,
                SeriesTitle= "CO2",
                SeriesColor= "#ec9150",
                Unit= "ppm",
                Decimals= 1
            }}
      };
    }
}
