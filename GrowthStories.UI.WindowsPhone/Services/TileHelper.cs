using System;
using Growthstories.Core;
using Growthstories.UI.ViewModel;
using GrowthStories.UI.WindowsPhone.BA;
using Microsoft.Phone.Shell;

namespace Growthstories.UI.WindowsPhone
{
    class TileHelper : ITileHelper
    {
        private readonly IPlantViewModel Vm;
        private readonly IAuthUser AppUser;

        public TileHelper(IPlantViewModel vm, IAuthUser appUser = null)
        {
            this.Vm = vm;
            // this is optional and used only for optimization
            this.AppUser = appUser;
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
            return true;

        }

        public bool DeleteTile()
        {
            GSTileUtils.DeleteTile(Vm);
            _Current = null;
            return true;
        }

        private ShellTile _Current;
        private ShellTile Current { get { return _Current ?? (_Current = GSTileUtils.GetShellTile(Vm)); } }


        public bool HasTile
        {
            get
            {
                if (AppUser != null && AppUser.Id != Vm.UserId)
                    return false;
                return Current != null;
            }
        }
    }
}
