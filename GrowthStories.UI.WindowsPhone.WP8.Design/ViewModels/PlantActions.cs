
using Growthstories.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using Growthstories.Domain.Entities;

namespace Growthstories.UI.ViewModel
{



    public class PlantActionViewModel : DesignViewModelBase, IPlantActionViewModel
    {
        protected IconType _IconType;

        public PlantActionViewModel()
            : this(DateTimeOffset.Now)
        {

        }

        public PlantActionViewModel(DateTimeOffset Created)
        {


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


        public string Title { get; set; }

        public string PlantTitle
        {
            get { return ""; }
        }
    }

    public sealed class PlantCommentViewModel : PlantActionViewModel, IPlantCommentViewModel
    {
        public PlantCommentViewModel()
            : this(DateTimeOffset.Now)
        { }
        public PlantCommentViewModel(DateTimeOffset created)
            : base(created)
        {
            this.Title = "COMMENTED";
            this._IconType = IconType.NOTE;
            this.ActionType = PlantActionType.COMMENTED;
        }
    }

    public sealed class PlantMeasureViewModel : PlantActionViewModel, IPlantMeasureViewModel
    {
        public PlantMeasureViewModel()
            : this(DateTimeOffset.Now)
        { }
        public PlantMeasureViewModel(DateTimeOffset created)
            : base(created)
        {
            this.Title = "MEASURED";
            this._IconType = IconType.MEASURE;
            this.Series = new MeasurementTypeViewModel(MeasurementType.LENGTH, "LENGHT", IconType.MEASURE);
            this.Value = 23.45;
            this.ActionType = PlantActionType.MEASURED;

        }

        public MeasurementTypeViewModel Series { get; set; }
        public double? Value { get; set; }

    }

    public sealed class PlantWaterViewModel : PlantActionViewModel, IPlantWaterViewModel
    {
        public PlantWaterViewModel()
            : this(DateTimeOffset.Now)
        { }
        public PlantWaterViewModel(DateTimeOffset created)
            : base(created)
        {
            this.Title = "WATERED";
            this._IconType = IconType.WATER;
            this.ActionType = PlantActionType.WATERED;

        }
    }

    public sealed class PlantFertilizeViewModel : PlantActionViewModel, IPlantFertilizeViewModel
    {
        public PlantFertilizeViewModel()
            : this(DateTimeOffset.Now)
        { }
        public PlantFertilizeViewModel(DateTimeOffset created)
            : base(created)
        {
            this.Title = "NOURISHED";
            this._IconType = IconType.NOURISH;
            this.ActionType = PlantActionType.FERTILIZED;
        }
    }

    public sealed class PlantPhotoViewModel : PlantActionViewModel, IPlantPhotographViewModel
    {

        public PlantPhotoViewModel()
            : this(null, DateTimeOffset.Now)
        {

        }

        public PlantPhotoViewModel(string photo, DateTimeOffset created)
            : base(created)
        {
            this.Title = "PHOTOGRAPHED";
            this._IconType = IconType.PHOTO;
            this.PhotoData = new Photo()
            {
                LocalFullPath = photo ?? @"/TestData/517e100d782a828894.jpg",
                LocalUri = photo ?? @"/TestData/517e100d782a828894.jpg"
            };
            this.Photo = new BitmapImage(new Uri(PhotoData.LocalUri, UriKind.RelativeOrAbsolute));
            this.ActionType = PlantActionType.PHOTOGRAPHED;

        }

        public Photo PhotoData { get; set; }

        private BitmapImage _Photo;
        public BitmapImage Photo
        {
            get
            {
                return _Photo;
            }
            set
            {
                _Photo = value;
            }
        }

    }



    public sealed class MeasurementTypeViewModel
    {
        public string Title { get; set; }
        private IconType _Icon;
        public MeasurementType Type { get; set; }

        public MeasurementTypeViewModel(MeasurementType type, string title, IconType icon)
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

        public IList<MeasurementTypeViewModel> Types { get { return GetAll(); } }

        public static IList<MeasurementTypeViewModel> GetAll()
        {
            return new List<MeasurementTypeViewModel>()
            {
                new MeasurementTypeViewModel(MeasurementType.ILLUMINANCE,"Illuminance",IconType.ILLUMINANCE),
                new MeasurementTypeViewModel(MeasurementType.LENGTH,"Length",IconType.MEASURE),
                new MeasurementTypeViewModel(MeasurementType.PH,"PH",IconType.PH),
                new MeasurementTypeViewModel(MeasurementType.SOIL_HUMIDITY,"Soil Humidity",IconType.MISTING),
                new MeasurementTypeViewModel(MeasurementType.WEIGHT,"Weight",IconType.MEASURE)
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


}
