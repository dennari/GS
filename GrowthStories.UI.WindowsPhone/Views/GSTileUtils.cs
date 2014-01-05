using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Phone.Shell;
using Growthstories.UI.ViewModel;
using ReactiveUI;
using System.Net;
using System.Windows;
using System.Reactive.Linq;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.ComponentModel;
using System.Reactive.Disposables;
using Microsoft.Phone.Tasks;
using System.Windows.Media.Animation;
using System.Windows.Media;


namespace Growthstories.UI.WindowsPhone
{

    class GSTileUtils
    {


        public static void UpdateTiles(IGSAppViewModel app)
        {
            
        }


        public static ShellTile getShellTile(IPlantViewModel pvm)
        {
            return ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains(pvm.UrlPathSegment));
        }


        public static void CreateOrUpdateTile(IPlantViewModel pvm)
        {
            var tileData = new CycleTileData()
            {
                Title = pvm.Name.ToUpper(),
                Count = pvm.MissedCount    
            };

            var photoUris = new List<Uri>();

            foreach (var action in pvm.Actions)
            {
                var p = action.Photo;
                if (p != null)
                {
                    if (p.LocalUri != null)
                    {
                        photoUris.Add(new Uri(p.LocalUri));

                        // up to 9 images allowed for cycletile
                        if (photoUris.Count == 9)
                        {
                            return;
                        }
                    }
                }
            }

            if (photoUris.Count == 0)
            {
                photoUris.Add(new System.Uri("appdata:/Assets/Icons/NoImageNoText.png"));
            }

            tileData.CycleImages = photoUris;

            var tile = getShellTile(pvm);

            if (tile == null) {
                ShellTile.Create(new Uri(pvm.UrlPath, UriKind.Relative), tileData, false);

            } else {
                tile.Update(tileData);
            }

            pvm.HasTile = true;
        }


        public static void DeleteTile(IPlantViewModel pvm)
        {
            var t = getShellTile(pvm);
            if (t != null)
            {
                t.Delete();
                pvm.HasTile = false;
            }
        }

    }
}
