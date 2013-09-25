using Growthstories.Domain.Messaging;
using Growthstories.UI.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ReactiveUI;
using System.Reactive.Linq;
using Microsoft.Phone.Tasks;

namespace Growthstories.UI.WindowsPhone.ViewModels
{
    public abstract class ClientPlantActionViewModel : PlantActionViewModel
    {
        protected BitmapImage _IconImg;
        public BitmapImage IconImg
        {
            get
            {
                if (_IconImg == null)
                {
                    _IconImg = new BitmapImage(this.IconUri);
                }
                return _IconImg;
            }
        }


        public ClientPlantActionViewModel(ActionBase state, IGSApp app)
            : base(state, app)
        {

        }


        public static ClientPlantActionViewModel Create(ActionBase state, IGSApp app)
        {

            if (state == null)
                throw new ArgumentNullException("State [ActionBase] cannot be null");
            var t = state.GetType();
            if (t == typeof(Commented))
                return new PlantCommentViewModel((Commented)state, app);
            if (t == typeof(Watered))
                return new PlantWaterViewModel((Watered)state, app);
            if (t == typeof(Fertilized))
                return new PlantFertilizeViewModel((Fertilized)state, app);
            if (t == typeof(Photographed))
                return new PlantPhotographViewModel((Photographed)state, app);
            if (t == typeof(Measured))
                return new PlantMeasureViewModel((Measured)state, app);

            return null;
        }

        public static IPlantActionViewModel Create<T>(ActionBase state, IGSApp app) where T : IPlantActionViewModel
        {
            if (state != null)
                return Create(state, app);
            var t = typeof(T);
            if (t == typeof(IPlantCommentViewModel))
                return new PlantCommentViewModel(null, app);
            if (t == typeof(IPlantWaterViewModel))
                return new PlantWaterViewModel(null, app);
            if (t == typeof(IPlantFertilizeViewModel))
                return new PlantFertilizeViewModel(null, app);
            if (t == typeof(IPlantPhotographViewModel))
                return new PlantPhotographViewModel(null, app);
            if (t == typeof(IPlantMeasureViewModel))
                return new PlantMeasureViewModel(null, app);

            return null;
        }




        public override void AddCommandSubscription(object p)
        {
            throw new NotImplementedException();
        }
    }



    public class PlantCommentViewModel : ClientPlantActionViewModel, IPlantCommentViewModel
    {


        public new string Title { get { return "COMMENTED"; } }

        public PlantCommentViewModel(Commented state, IGSApp app)
            : base(state, app)
        {
            this._IconType = IconType.NOTE;

            this.CanExecute = this.WhenAnyValue(x => x.Note, x => !string.IsNullOrWhiteSpace(x));
        }


        public override void AddCommandSubscription(object p)
        {
            //this.SendCommand(new Comment(this.State.EntityId, this.State.PlantId, this.Note), true);
        }

    }

    public class PlantMeasureViewModel : ClientPlantActionViewModel, IPlantMeasureViewModel
    {


        public new string Title { get { return "measure"; } }

        public IList<MeasurementTypeViewModel> MeasurementTypes { get; protected set; }


        protected ObservableAsPropertyHelper<MeasurementTypeViewModel> _Series;
        public MeasurementTypeViewModel Series
        {
            get { return _Series.Value; }
        }

        public ReactiveCommand SeriesSelected { get; protected set; }

        protected string _SValue;
        public string SValue
        {
            get { return _SValue; }
            set { this.RaiseAndSetIfChanged(ref _SValue, value); }
        }

        protected double? _Value;
        public double? Value
        {
            get { return _Value; }
            set { this.RaiseAndSetIfChanged(ref _Value, value); }
        }

        //public string Series { get { return ((Measured)this.State).Series.ToString("G"); } }
        //public string Value { get { return ((Measured)this.State).Value.ToString("F1"); } }

        public override void AddCommandSubscription(object p)
        {


        }


        public PlantMeasureViewModel(Measured state, IGSApp app)
            : base(state, app)
        {
            this._IconType = IconType.MEASURE;
            this.MeasurementTypes = MeasurementTypeViewModel.GetAll(app);
            this.SeriesSelected = new ReactiveCommand();
            this.SeriesSelected
                .OfType<MeasurementTypeViewModel>()
                .ToProperty(this, x => x.Series, out _Series, state != null ? MeasurementTypes.FirstOrDefault(x => x.Type == state.Series) : MeasurementTypes[0]);





            double dValue = 0;
            this.WhenAnyValue(x => x.SValue, x => x)
                .Where(x => double.TryParse(x, out dValue))
                .Subscribe(x => this.Value = dValue);

            this.CanExecute = this.WhenAnyValue(x => x.Value, x => x.HasValue);

            if (state != null)
            {
                this.SValue = state.Value.ToString("F1");
                this.Value = state.Value;
            }
        }


    }

    public class PlantWaterViewModel : ClientPlantActionViewModel, IPlantWaterViewModel
    {

        public new string Title { get { return "WATERED"; } }


        public PlantWaterViewModel(Watered state, IGSApp app)
            : base(state, app)
        {
            this._IconType = IconType.WATER;
        }

        public override void AddCommandSubscription(object p)
        {
            //this.SendCommand(new Water(this.State.EntityId, this.State.PlantId, this.Note), true);
        }
    }

    public class PlantFertilizeViewModel : ClientPlantActionViewModel, IPlantFertilizeViewModel
    {

        public new string Title { get { return "NOURISHED"; } }

        public PlantFertilizeViewModel(Fertilized state, IGSApp app)
            : base(state, app)
        {
            this._IconType = IconType.FERTILIZE;
        }

        public override void AddCommandSubscription(object p)
        {
            //this.SendCommand(new Fertilize(this.State.EntityId, this.State.PlantId, this.Note), true);
        }

    }


    public class PlantPhotographViewModel : ClientPlantActionViewModel, IPlantPhotographViewModel
    {
        public new string Title { get { return "PHOTOGRAPHED"; } }

        protected Photo _PhotoData;
        public Photo PhotoData
        {
            get
            {
                return _PhotoData;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _PhotoData, value);
            }
        }


        protected BitmapImage _Photo;
        public BitmapImage Photo
        {
            get
            {
                return _Photo ?? (_Photo = new BitmapImage()
                {
                    CreateOptions = BitmapCreateOptions.DelayCreation,
                    DecodePixelType = DecodePixelType.Physical
                });
            }
        }

        public PlantPhotographViewModel(Photographed state, IGSApp app)
            : base(state, app)
        {
            this._IconType = IconType.PHOTO;

            this.WhenAnyValue(x => x.PhotoData, x => x)
                .Where(x => x != default(Photo))
                .Subscribe(x => Photo.SetSource(x));

            if (state != null)
            {
                this.PhotoData = state.Photo;
            }
        }


        public override void AddCommandSubscription(object p)
        {
            //this.SendCommand(new Photograph(this.State.EntityId, this.State.PlantId, this.Note, this.Path), true);
        }

        private PhotoChooserTask _Chooser;
        public PhotoChooserTask Chooser
        {
            get
            {
                if (_Chooser == null)
                {
                    var t = new PhotoChooserTask();
                    t.Completed += async (s, e) => await t_Completed(s, e);
                    //t.Completed += t_Completed;
                    t.ShowCamera = true;
                    _Chooser = t;
                }
                return _Chooser;
            }
        }

        protected ReactiveCommand _PhotoChooserCommand;
        public ReactiveCommand PhotoChooserCommand
        {
            get
            {
                if (_PhotoChooserCommand == null)
                {
                    _PhotoChooserCommand = new ReactiveCommand();
                    _PhotoChooserCommand.Subscribe(_ =>
                    {
                        Chooser.Show();
                    });
                }
                return _PhotoChooserCommand;
            }
        }


        async Task t_Completed(object sender, PhotoResult e)
        {
            //throw new NotImplementedException();
            var image = e.ChosenPhoto;
            if (e.TaskResult == TaskResult.OK && image.CanRead && image.Length > 0)
            {
                PhotoData = await image.SavePhotoToLocalStorageAsync();
            }
        }




    }


















}
