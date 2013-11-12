
using Growthstories.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using Growthstories.Domain.Entities;
using ReactiveUI;

namespace Growthstories.UI.ViewModel
{



    public class PlantActionViewModel : DesignViewModelBase, IPlantActionViewModel
    {

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

            _OpenZoomView = new MockReactiveCommand(o =>
            {
                this.IsZoomViewOpen = true;
            });
        }

        public string WeekDay { get; set; }

        public string Date { get; set; }

        public string Time { get; set; }

        public string Note { get; set; }

        public PlantActionType ActionType { get; protected set; }


        public IconType IconType { get; protected set; }

        protected bool _IsZoomViewOpen = false;
        public bool IsZoomViewOpen
        {
            get { return _IsZoomViewOpen; }
            set
            {
                if (_IsZoomViewOpen != value)
                {
                    _IsZoomViewOpen = value;
                    RaisePropertyChanged();
                }
            }
        }

        public Guid PlantActionId { get; set; }

        public DateTimeOffset Created { get; set; }


        public string Title { get; set; }

        public string PlantTitle
        {
            get { return ""; }
        }


        MockReactiveCommand _OpenZoomView;
        public ReactiveUI.IReactiveCommand OpenZoomView
        {
            get
            {
                return _OpenZoomView;
            }
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
            this.IconType = IconType.NOTE;
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
            this.IconType = IconType.MEASURE;
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
            this.IconType = IconType.WATER;
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
            this.IconType = IconType.NOURISH;
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
            this.IconType = IconType.PHOTO;
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
        public IconType IconType { get; set; }
        public MeasurementType Type { get; set; }

        public MeasurementTypeViewModel(MeasurementType type, string title, IconType icon)
        {
            this.Type = type;
            this.Title = title;
            this.IconType = icon;


        }

        public override string ToString()
        {
            return Title;
        }


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

        public static IDictionary<MeasurementType, MeasurementTypeViewModel> GetAllDict()
        {
            return GetAll().ToDictionary(x => x.Type);
        }

    }



}
