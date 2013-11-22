
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


        public IconType Icon { get; protected set; }

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


        public MeasurementType MeasurementType { get; set; }


        public double? Value { get; set; }

        public Photo Photo { get; set; }


        public string Label { get; set; }

    }



    public sealed class PlantMeasureViewModel : PlantActionViewModel, IPlantMeasureViewModel
    {
        public PlantMeasureViewModel()
            : this(DateTimeOffset.Now)
        { }
        public PlantMeasureViewModel(DateTimeOffset created)
            : base(created)
        {
            this.Series = new MeasurementTypeViewModel(MeasurementType.LENGTH, "LENGHT", IconType.MEASURE);
            this.MeasurementType = MeasurementType.LENGTH;
            this.Value = 23.45;

        }

        public MeasurementTypeViewModel Series { get; set; }
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
            this.Photo = new Photo()
            {
                LocalFullPath = photo ?? @"/TestData/517e100d782a828894.jpg",
                LocalUri = photo ?? @"/TestData/517e100d782a828894.jpg"
            };
            this.PhotoSource = new BitmapImage(new Uri(Photo.LocalUri, UriKind.RelativeOrAbsolute));

        }


        private BitmapImage _PhotoSource;
        public BitmapImage PhotoSource
        {
            get
            {
                return _PhotoSource;
            }
            set
            {
                _PhotoSource = value;
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
