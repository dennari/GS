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

        public TileHelper(IPlantViewModel vm, IAuthUser appUser = null)
        {
            this.Vm = vm;
            // this is optional and used only for optimization
            this.AppUser = appUser;

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
            GSTileUtils.DeleteTile(Vm);
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

    }
}
