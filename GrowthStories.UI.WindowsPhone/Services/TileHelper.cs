using System;
using System.Reactive.Linq;
using Growthstories.Core;
using Growthstories.UI.ViewModel;
using GrowthStories.UI.WindowsPhone.BA;
using Microsoft.Phone.Shell;
using ReactiveUI;
using Growthstories.Domain.Entities;
using System.Collections.Generic;


namespace Growthstories.UI.WindowsPhone
{
    class TileHelper : ReactiveObject, ITileHelper
    {


        private readonly IPlantViewModel Vm;
        private readonly IAuthUser AppUser;

        // IDisposable UpdateSubscription;


        public TileHelper(IPlantViewModel vm, IAuthUser appUser = null)
        {
            this.Vm = vm;
            // this is optional and used only for optimization
            this.AppUser = appUser;

            // should not listen to followed users plants 
            vm.WhenAnyValue(x => x.HasWriteAccess)
                .Where(x => x).Subscribe(_ => SubscribeToUpdates());

            vm.WhenAnyValue(x => x.Id).Where(x => x != default(Guid))
                .Take(1).Subscribe(x => this.HasTile = Current != null);
        }


        public bool CreateOrUpdateTile()
        {
            var tile = Current;
            if (tile != null)
            {
                GSTileUtils.UpdateTileAndInfoAfterDelay(Vm);

            }
            else
            {
                var info = GSTileUtils.CreateTileUpdateInfo(Vm);
                ShellTile.Create(new Uri(info.UrlPath, UriKind.Relative), GSTileUtils.GetTileData(info), true);
                GSTileUtils.WriteTileUpdateInfo(info);
            }
            _Current = null;
            HasTile = true;
            return true;
        }


        public void UpdateHasTile()
        {
            _Current = null;
            HasTile = Current != null;
        }


        public bool DeleteTile()
        {
            if (HasTile)
                GSTileUtils.DeleteTile(Vm);
            GSTileUtils.ClearTileUpdateInfo(Vm);
            ClearSubscriptions();
            _Current = null;
            HasTile = false;
            return true;
        }


        private ShellTile _Current;
        private ShellTile Current { get { return _Current ?? (_Current = GSTileUtils.GetShellTile(Vm)); } }


        private bool _HasTile;
        public bool HasTile
        {
            get
            {
                return _HasTile;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref _HasTile, value);
            }
        }

        List<IDisposable> subs = new List<IDisposable>();
        IDisposable missedSubscription;


        private void ClearSubscriptions()
        {
            foreach (var d in subs)
            {
                d.Dispose();
            }
            subs.Clear();
        }


        private void SubscribeToUpdates()
        {
            // there should not be any at this point, but does not hurt
            ClearSubscriptions(); 
         
            //  watch for watering scheduler updates
            subs.Add(Vm.WhenAnyValue(z => z.WateringScheduler).Subscribe(u =>
            {
                if (missedSubscription != null)
                {
                    missedSubscription.Dispose();
                }
                // missed is updated whenever items are added 
                // or removed, or schedule is recalculated

                if (u != null)
                {
                    missedSubscription = u.WhenAnyValue(y => y.Missed)
                    .Subscribe(_ =>
                    {
                        TriggerTileUpdate();
                    });
                    subs.Add(missedSubscription);
                }
            }));

            // watch for watering schedule enabled updates
            subs.Add(Vm.WhenAnyValue(w => w.IsWateringScheduleEnabled).Subscribe(u =>
            {
                TriggerTileUpdate();
            }));

            // watch for new photos
            subs.Add(Vm.Actions.ItemsAdded.Subscribe(a =>
            {
                if (a.ActionType == PlantActionType.PHOTOGRAPHED)
                {
                    TriggerTileUpdate();
                }
            }));


            // this implementation could not deal with the wateringscheduler
            // being switched
            // 
            // -- JOJ 26.1.2014
            //

            //return Vm.WhenAny(
            //    z => z.WateringScheduler.Missed,
            //    z => z.IsWateringScheduleEnabled,
            //    z => z.Actions.ItemsAdded,
            //    (a, b, c) => true
            //    )
            //    .Subscribe(_ =>
            //        {
            //            this.Log().Info("updating tileinfo for {0}", Vm.Name);
            //            GSTileUtils.UpdateTileAndInfoAfterDelay(Vm);
            //        });
        }

        private void TriggerTileUpdate()
        {
            this.Log().Info("triggered update of tileinfo for {0}", Vm.Name);
            GSTileUtils.UpdateTileAndInfoAfterDelay(Vm);
        }


    }

}
