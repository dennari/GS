using System;
using System.Reactive.Linq;
using Growthstories.Core;
using Growthstories.UI.ViewModel;
using GrowthStories.UI.WindowsPhone.BA;
using Microsoft.Phone.Shell;
using ReactiveUI;
namespace Growthstories.UI.WindowsPhone
{
    class TileHelper : ReactiveObject, ITileHelper
    {
        private readonly IPlantViewModel Vm;
        private readonly IAuthUser AppUser;
        IDisposable UpdateSubscription;

        public TileHelper(IPlantViewModel vm, IAuthUser appUser = null)
        {
            this.Vm = vm;
            // this is optional and used only for optimization
            this.AppUser = appUser;


            UpdateSubscription = SubscribeToUpdates();

            //this.WhenAnyValue(x => x.HasTile).Skip(1).Subscribe(x =>
            //{
            //    if (x)
            //    else
            //        UpdateSubscription.Dispose();
            //});

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

        public bool DeleteTile()
        {
            if (HasTile)
                GSTileUtils.DeleteTile(Vm);
            GSTileUtils.ClearTileUpdateInfo(Vm);
            UpdateSubscription.Dispose();
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



        private IDisposable SubscribeToUpdates()
        {
            return Vm.WhenAny(
                z => z.WateringScheduler.Missed,
                z => z.IsWateringScheduleEnabled,
                z => z.Actions.ItemsAdded,
                (a, b, c) => true
                )
            .Subscribe(_ => GSTileUtils.UpdateTileAndInfoAfterDelay(Vm));
        }
    }
}
