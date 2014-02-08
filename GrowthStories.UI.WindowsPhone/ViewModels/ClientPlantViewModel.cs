
using System;
using System.Reactive.Linq;
using System.Windows.Media.Imaging;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.UI.ViewModel;
using Microsoft.Phone.Tasks;
using ReactiveUI;


namespace Growthstories.UI.WindowsPhone.ViewModels
{



    public class ClientPlantViewModel : PlantViewModel
    {

        private readonly Func<IPlantViewModel, ITileHelper> TileHelperFactory;

        private ITileHelper _TileHelper;
        public ITileHelper TileHelper
        {
            get
            {
                return _TileHelper ?? (_TileHelper = TileHelperFactory(this));
            }
        }


        public BitmapImage TilePhotoSource
        {
            get
            {
                var vm = ProfilePictureAction as ClientPlantPhotographViewModel;

                if (vm == null || vm.Photo.Uri == null)
                {
                    return null;
                }

                return new BitmapImage(new Uri(vm.Photo.Uri, UriKind.RelativeOrAbsolute))
                {
                    CreateOptions = BitmapCreateOptions.DelayCreation,
                    DecodePixelType = DecodePixelType.Logical,
                    DecodePixelHeight = 396
                };
            }

            //set
            //{
            //    this.RaiseAndSetIfChanged(ref _PhotoSource, value);
            //}
        }

        //private BitmapImage _TilePhotoSource;
        //public BitmapImage TilePhotoSource
        //{
        //    get
        //    {
        //        return _TilePhotoSource;
        //    }
        //    set
        //    {
        //        this.RaiseAndSetIfChanged(ref _TilePhotoSource, value);
        //    }
        //}



        public ClientPlantViewModel(
            IObservable<Tuple<PlantState, ScheduleState, ScheduleState>> stateObservable,
            Func<IPlantViewModel, ITileHelper> tileHelperFactory,
            IGSAppViewModel app)
            : base(stateObservable, app)
        {

            if (tileHelperFactory == null)
                throw new ArgumentNullException("tileHelperFactory needs to be given.");
            this.TileHelperFactory = tileHelperFactory;

            TileHelper.WhenAnyValue(x => x.HasTile).Subscribe(x => this.HasTile = x);

            //this.WhenAnyValue(x => x.HasTile).Where(x => x).Subscribe(_ => TileHelper.SubscribeToUpdates());

            PinCommand
                .Subscribe(_ =>
                {
                    if (TileHelper.HasTile)
                    {
                        TileHelper.DeleteTile();
                    }
                    else
                    {
                        TileHelper.CreateOrUpdateTile();
                    }

                });

            this.WhenAnyValue(x => x.Id)
                .Where(x => x != default(Guid))
                .SelectMany(x => this.ListenTo<AggregateDeleted>(x).Take(1))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ =>
                {
                    TileHelper.DeleteTile();
                    //TileHelper.
                });

            this.WhenAnyValue(x => x.ProfilePictureAction)
                //.Where(x => x != null)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x =>
                {
                    if (x != null)
                    {
                        this.Log().Info("raising property changed for tilephotosource, actionId is {0} url is {1}", x.PlantActionId, x.PhotoUri);
                    }
                    else
                    {
                        this.Log().Info("raising property changed for tilephotosource, action is null");
                    }
                    raisePropertyChanged("TilePhotoSource");
                });

            ShareCommand.ObserveOn(RxApp.MainThreadScheduler).Subscribe(_ => Share());
        }




        private void Share()
        {
            ShareLinkTask shareLinkTask = new ShareLinkTask();

            shareLinkTask.Title = "Story of " + Name.ToUpper();
            shareLinkTask.LinkUri = new Uri(
                "http://www.growthstories.com/plant/" + UserId + "/" + Id, UriKind.Absolute);
            shareLinkTask.Message = "Check out how my plant " + Name.ToUpper() + " is doing!";

            shareLinkTask.Show();
        }

    }


}