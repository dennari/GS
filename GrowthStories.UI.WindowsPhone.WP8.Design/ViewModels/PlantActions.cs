
using Growthstories.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace Growthstories.UI.ViewModel
{

    public enum PlantActionType
    {
        NOTYPE,
        WATERED,
        FERTILIZED,
        PHOTOGRAPHED,
        MEASURED,
        COMMENTED
    }

    public interface IPlantActionViewModel
    {
        string WeekDay { get; }
        string Date { get; }
        string Time { get; }
        string Note { get; }
        PlantActionType ActionType { get; }
        Uri IconUri { get; }
        Guid PlantActionId { get; }
        DateTimeOffset Created { get; }

        string TopTitle { get; }
        string Title { get; }

        //PlantActionState State { get; }

    }

    public interface IPlantCommentViewModel : IPlantActionViewModel
    {


    }

    public interface IPlantMeasureViewModel : IPlantActionViewModel
    {

        MeasurementTypeViewModelDesign Series { get; }
        double? Value { get; }
    }

    public interface IPlantWaterViewModel : IPlantActionViewModel
    {


    }

    public interface IPlantFertilizeViewModel : IPlantActionViewModel
    {


    }

    public interface IPlantPhotographViewModel : IPlantActionViewModel
    {

        Photo PhotoData { get; }
    }

    public class PlantActionViewModelDesign : IPlantActionViewModel
    {
        protected IconType _IconType;

        public PlantActionViewModelDesign()
        {

            var Created = DateTimeOffset.Now;

            this.Note = "Just a note";
            this.WeekDay = Created.ToString("dddd");
            this.Date = Created.ToString("d");
            this.Time = Created.ToString("t");
            this.PlantActionId = Guid.NewGuid();
            this.Created = Created;
        }

        public string WeekDay { get; set; }

        public string Date { get; set; }

        public string Time { get; set; }

        public string Note { get; set; }

        public PlantActionType ActionType { get; protected set; }


        public Uri IconUri { get { return Utils.BigIcons[this._IconType]; } }

        public BitmapImage Icon
        {
            get
            {
                return new BitmapImage(IconUri);
            }
        }

        public Guid PlantActionId { get; set; }

        public DateTimeOffset Created { get; set; }

        public string TopTitle { get; set; }

        public string Title { get; set; }
    }

    public sealed class PlantCommentViewModel : PlantActionViewModelDesign, IPlantCommentViewModel
    {
        public PlantCommentViewModel()
        {
            this.Title = "COMMENTED";
            this._IconType = IconType.NOTE;
            this.ActionType = PlantActionType.COMMENTED;
        }
    }

    public sealed class PlantMeasureViewModel : PlantActionViewModelDesign, IPlantMeasureViewModel
    {
        public PlantMeasureViewModel()
        {
            this.Title = "MEASURED";
            this._IconType = IconType.MEASURE;
            this.Series = new MeasurementTypeViewModelDesign(MeasurementType.LENGTH, "LENGHT", IconType.MEASURE);
            this.Value = 23.45;
            this.ActionType = PlantActionType.MEASURED;

        }

        public MeasurementTypeViewModelDesign Series { get; set; }
        public double? Value { get; set; }

    }

    public sealed class PlantWaterViewModel : PlantActionViewModelDesign, IPlantWaterViewModel
    {
        public PlantWaterViewModel()
        {
            this.Title = "WATERED";
            this._IconType = IconType.WATER;
            this.ActionType = PlantActionType.WATERED;

        }
    }

    public sealed class PlantFertilizeViewModel : PlantActionViewModelDesign, IPlantFertilizeViewModel
    {
        public PlantFertilizeViewModel()
        {
            this.Title = "NOURISHED";
            this._IconType = IconType.NOURISH;
            this.ActionType = PlantActionType.FERTILIZED;
        }
    }

    public sealed class PlantPhotoViewModel : PlantActionViewModelDesign, IPlantPhotographViewModel
    {

        public PlantPhotoViewModel()
            : this(null)
        {

        }

        public PlantPhotoViewModel(string photo = null)
        {
            this.Title = "PHOTOGRAPHED";
            this._IconType = IconType.PHOTO;
            this.PhotoData = new Photo()
            {
                LocalFullPath = photo ?? @"/TestData/517e100d782a828894.jpg",
                LocalUri = photo ?? @"/TestData/517e100d782a828894.jpg"
            };
            //this.Photo = new BitmapImage(new Uri(PhotoData.LocalUri, UriKind.Relative));
            this.ActionType = PlantActionType.PHOTOGRAPHED;

        }

        public Photo PhotoData { get; set; }

        //private BitmapImage _Photo;
        //public BitmapImage Photo
        //{
        //    get
        //    {
        //        return _Photo;
        //    }
        //    set
        //    {
        //        _Photo = value;
        //    }
        //}

    }



    public sealed class MeasurementTypeViewModelDesign
    {
        public string Title { get; set; }
        private IconType _Icon;
        public MeasurementType Type { get; set; }

        public MeasurementTypeViewModelDesign(MeasurementType type, string title, IconType icon)
        {
            this.Type = type;
            this.Title = title;
            this._Icon = icon;


        }

        public override string ToString()
        {
            return Title;
        }

        public Uri Icon { get { return Utils.BigIcons[this._Icon]; } }

        public static IList<MeasurementTypeViewModelDesign> GetAll()
        {
            return new List<MeasurementTypeViewModelDesign>()
            {
                new MeasurementTypeViewModelDesign(MeasurementType.ILLUMINANCE,"Illuminance",IconType.ILLUMINANCE),
                new MeasurementTypeViewModelDesign(MeasurementType.LENGTH,"Length",IconType.MEASURE),
                new MeasurementTypeViewModelDesign(MeasurementType.PH,"PH",IconType.PH),
                new MeasurementTypeViewModelDesign(MeasurementType.SOIL_HUMIDITY,"Soil Humidity",IconType.MISTING),
                new MeasurementTypeViewModelDesign(MeasurementType.WEIGHT,"Weight",IconType.MEASURE)
            };
        }

    }


    public static class Utils
    {
        public static readonly IDictionary<IconType, Uri> BigIcons = new Dictionary<IconType, Uri>()
        {
            {IconType.WATER,new Uri("/Assets/Icons/icon_watering.png", UriKind.RelativeOrAbsolute)},
            {IconType.PHOTO,new Uri("/Assets/Icons/icon_photo.png", UriKind.RelativeOrAbsolute)},
            {IconType.FERTILIZE,new Uri("/Assets/Icons/icon_nutrient.png", UriKind.RelativeOrAbsolute)},
            {IconType.NOURISH,new Uri("/Assets/Icons/icon_nutrient.png", UriKind.RelativeOrAbsolute)},
            {IconType.NOTE,new Uri("/Assets/Icons/icon_comment.png", UriKind.RelativeOrAbsolute)},
            {IconType.MEASURE,new Uri("/Assets/Icons/icon_length.png", UriKind.RelativeOrAbsolute)},
            {IconType.CHANGESOIL,new Uri("/Assets/Icons/icon_soilchange.png", UriKind.RelativeOrAbsolute)},
            {IconType.BLOOMING,new Uri("/Assets/Icons/icon_blooming.png", UriKind.RelativeOrAbsolute)},
            {IconType.DECEASED,new Uri("/Assets/Icons/icon_deceased.png", UriKind.RelativeOrAbsolute)},
            {IconType.ILLUMINANCE,new Uri("/Assets/Icons/icon_illuminance.png", UriKind.RelativeOrAbsolute)},
            {IconType.MISTING,new Uri("/Assets/Icons/icon_misting.png", UriKind.RelativeOrAbsolute)},
            {IconType.PH,new Uri("/Assets/Icons/icon_ph.png", UriKind.RelativeOrAbsolute)},
            {IconType.POLLINATION,new Uri("/Assets/Icons/icon_pollination.png", UriKind.RelativeOrAbsolute)},
            {IconType.SPROUTING,new Uri("/Assets/Icons/icon_sprouting.png", UriKind.RelativeOrAbsolute)},
        };
    }

    public enum IconType
    {
        ADD,
        CHECK,
        CANCEL,
        DELETE,
        CHECK_LIST,
        WATER,
        FERTILIZE,
        PHOTO,
        NOTE,
        MEASURE,
        NOURISH,
        CHANGESOIL,
        SHARE,
        BLOOMING,
        DECEASED,
        ILLUMINANCE,
        LENGTH,
        MISTING,
        PH,
        POLLINATION,
        SPROUTING
    }

}
