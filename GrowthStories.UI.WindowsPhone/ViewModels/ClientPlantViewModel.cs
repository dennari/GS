
using System;
using System.Reactive.Linq;
using Growthstories.Domain.Entities;
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
            PinCommand
                .Subscribe(_ =>
                {
                    if (HasTile)
                    {
                        TileHelper.DeleteTile();
                    }
                    else
                    {
                        TileHelper.CreateOrUpdateTile();
                    }

                });

            DeleteObservable
               .Subscribe(_ =>
               {
                   if (HasTile)
                   {
                       TileHelper.DeleteTile();
                   }

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