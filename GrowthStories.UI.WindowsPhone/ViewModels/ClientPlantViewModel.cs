
using System;
using System.Reactive.Linq;
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
        private ITileHelper TileHelper
        {
            get
            {
                return _TileHelper ?? (_TileHelper = TileHelperFactory(this));
            }

        }




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




            ShareCommand.ObserveOn(RxApp.MainThreadScheduler).Subscribe(_ => Share());
        }






        private void Share()
        {
            ShareLinkTask shareLinkTask = new ShareLinkTask();

            shareLinkTask.Title = "Story of " + Name;
            shareLinkTask.LinkUri = new Uri(
                "http://www.growthstories.com/plant/" + UserId + "/" + Id, UriKind.Absolute);
            shareLinkTask.Message = "Check out how my plant " + Name + " is doing!";

            shareLinkTask.Show();
        }

    }


}