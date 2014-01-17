
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Growthstories.UI.ViewModel;
using GrowthStories.UI.WindowsPhone.BA;
using Microsoft.Phone.Tasks;


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




        public ClientPlantViewModel(IObservable<Tuple<PlantState, ScheduleState, ScheduleState>> stateObservable, Func<IPlantViewModel, ITileHelper> tileHelperFactory, IGSAppViewModel app)
            : base(stateObservable, app)
        {

            TileHelperFactory = tileHelperFactory;
            Init();

        }



        private void Init()
        {
            PinCommand
                .Select(_ => TileHelper.HasTile ? (Func<bool>)TileHelper.DeleteTile : (Func<bool>)TileHelper.CreateOrUpdateTile)
                .Subscribe(x => x());
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