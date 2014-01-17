using Growthstories.UI.ViewModel;
using GrowthStories.UI.WindowsPhone.BA;
using Microsoft.Phone.Shell;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.UI.WindowsPhone
{
    class TileHelper : ITileHelper
    {
        private readonly IPlantViewModel Vm;
        public TileHelper(IPlantViewModel vm)
        {
            this.Vm = vm;
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
            return true;
        }

        private ShellTile _Current;
        private ShellTile Current { get { return _Current ?? (_Current = GSTileUtils.GetShellTile(Vm)); } }


        public bool HasTile
        {
            get { return Current != null; }
        }
    }
}
