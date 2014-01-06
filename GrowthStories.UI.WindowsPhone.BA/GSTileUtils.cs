using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Phone.Shell;
using Growthstories.UI.ViewModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using System.ComponentModel;
using Microsoft.Phone.Tasks;
using System.Windows.Media.Animation;
using System.Windows.Media;
using Growthstories.Domain;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Newtonsoft.Json;
using System.IO.IsolatedStorage;
using System.Diagnostics;


namespace GrowthStories.UI.WindowsPhone.BA
{

    public class TileUpdateInfo
    {

        [JsonProperty]
        public DateTimeOffset Last { get; set; }
        
        [JsonProperty]
        public TimeSpan Interval { get; set; }
        
        [JsonProperty]
        public String UrlPathSegment { get; set; }

        [JsonProperty]
        public String UrlPath { get; set; }

        [JsonProperty]
        public List<Uri> PhotoUris { get; set; }

        [JsonProperty]
        public string Name { get; set; }

    }


    public class PeriodicTaskInfo
    {
        [JsonProperty]
        public HashSet<TileUpdateInfo> infos { get; set;}
    }
    

   
    public class GSTileUtils
    {


        public const string SETTINGS_KEY = "periodicTaskInfo";


        public static PeriodicTaskInfo CreateEmptyPeriodikTaskInfo()
        {
            var info = new PeriodicTaskInfo();
            info.infos = new HashSet<TileUpdateInfo>();

            return info;
        }


        public static PeriodicTaskInfo ReadPeriodicTaskInfo()
        {
            var settings = IsolatedStorageSettings.ApplicationSettings;

            if (!settings.Contains(SETTINGS_KEY))
            {
                return CreateEmptyPeriodikTaskInfo();
            }

            string infoString = (string)settings[SETTINGS_KEY];
            
            try {
                var ret = JsonConvert.DeserializeObject<PeriodicTaskInfo>(infoString);
                return ret;

            } catch (Exception ex){
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                return CreateEmptyPeriodikTaskInfo();
            }
        }


        public static void WritePeriodicTaskInfo(PeriodicTaskInfo info)
        {
            var settings = IsolatedStorageSettings.ApplicationSettings;

            var s = JsonConvert.SerializeObject(info, Formatting.None);

            settings.Remove(SETTINGS_KEY);
            settings.Add(SETTINGS_KEY, s);
            settings.Save();
        }


        public static void UpdateTilesAndInfos(IGSAppViewModel app, IUIPersistence uip)
        {
            var pti = CreateEmptyPeriodikTaskInfo();

            pti.infos = CreateTileUpdateInfos(app, uip);
            WritePeriodicTaskInfo(pti);

            UpdateTiles(pti.infos);
        }


        public static void UpdateTiles()
        {
            var pti = ReadPeriodicTaskInfo();
            UpdateTiles(pti.infos);
        }


        private static HashSet<PlantViewModel> GetPlantViewModels(IGSAppViewModel app, IUIPersistence pers)
        {
            var ret = new HashSet<PlantViewModel>();

            if (app.User == null)
            {
                return ret;
            }

            var uid = app.User.Id;

            foreach (var plant in pers.GetPlants(null, null, uid))
            {
                var wsvm = new ScheduleViewModel(plant.Item2, ScheduleType.WATERING, app);
                var fsvm = new ScheduleViewModel(plant.Item3, ScheduleType.FERTILIZING, app);
                var pvm = new PlantViewModel(plant.Item1, wsvm, fsvm, app);
                ret.Add(pvm);
            }
            return ret;
        }


        private static HashSet<TileUpdateInfo> CreateTileUpdateInfos(IGSAppViewModel app, IUIPersistence uip)
        {
            var pvms = GSTileUtils.GetPlantViewModels(app, uip);
            return CreateTileUpdateInfos(pvms, app, uip);
        }


        private static HashSet<TileUpdateInfo> CreateTileUpdateInfos(HashSet<PlantViewModel> pvms, IGSAppViewModel app, IUIPersistence uip)
        {
            // little bit lame to have side effects in this method, could refactor
            // -- JOJ 6.1.2014

            var ret = new HashSet<TileUpdateInfo>();

            foreach (var pvm in pvms)
            {
                var tile = GetShellTile(pvm);

                if (tile != null)
                {
                    if (pvm.State.IsDeleted)
                    {
                        tile.Delete();
                    
                    } else {
                        var ti = CreateTileUpdateInfo(pvm, FetchActions(pvm.State.Id, uip, app));
                        if (ti != null)
                        {
                            ret.Add(ti);
                        }
                        pvm.HasTile = true;
                    }
                } else {
                    pvm.HasTile = false;
                }
            }
            return ret;
        }


        public static void CreateOrUpdateTile(PlantViewModel pvm, IGSAppViewModel app, IUIPersistence uip)
        {
            var info = CreateTileUpdateInfo(pvm, FetchActions(pvm.State.Id, uip, app));
            UpdateTile(info);
            
            UpdateTilesAndInfos(app, uip);
        }

        public static ShellTile GetShellTile(IPlantViewModel pvm)
        {
            return GetShellTile(pvm.UrlPathSegment);
        }


        public static ShellTile GetShellTile(string urlPathSegment)
        {
            return ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains(urlPathSegment)); 
        }


        public static List<PlantActionViewModel> FetchActions(Guid plantId, IUIPersistence uip, IGSAppViewModel app)
        {
            var ret = new List<PlantActionViewModel>();
            
            foreach (var state in uip.GetPhotoActions(plantId))
            {
                if (state.Type == PlantActionType.PHOTOGRAPHED)
                {
                    var avm = new PlantPhotographViewModel(app, state);
                    ret.Add(avm);
                }
            }

            return ret;
        }


        private static void UpdateTiles(HashSet<TileUpdateInfo> infos)
        {
            foreach (var info in infos)
            {
                UpdateTile(info);
            }
        }


        private static TileUpdateInfo CreateTileUpdateInfo(IPlantViewModel pvm, IEnumerable<IPlantActionViewModel> list)
        {            
            TileUpdateInfo info = new TileUpdateInfo();

            if (pvm.WateringSchedule != null && pvm.WateringScheduler != null)
            {
                info.Interval = (TimeSpan)pvm.WateringSchedule.Interval;
                info.Last = (DateTimeOffset)pvm.WateringScheduler.LastActionTime;
            }
            info.UrlPathSegment = pvm.UrlPathSegment;
            info.UrlPath = pvm.UrlPath;
            info.Name = pvm.Name;

            var photoUris = new List<Uri>();

            foreach (var action in list)
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
                            break;
                        }
                    }
                }
            }

            if (photoUris.Count == 0)
            {
                photoUris.Add(new System.Uri("appdata:/Assets/Icons/NoImageNoText.png"));
            }

            info.PhotoUris = photoUris;

            return info;
        }


        public static CycleTileData GetTileData(TileUpdateInfo info)
        {
            var cnt = 0;

            if (info.Interval != null && info.Last != null)
            {
                var missed = PlantScheduler.CalculateMissed(info.Last, info.Interval);
                cnt = PlantScheduler.GetMissedCount(missed);
            }

            var tileData = new CycleTileData()
            {
                Title = info.Name.ToUpper(),
                Count = cnt,
                CycleImages = info.PhotoUris
            };

            return tileData;
        }


        public static void UpdateTile(TileUpdateInfo info)
        {
            var tileData = GetTileData(info);

            var tile = GetShellTile(info.UrlPathSegment);
            if (tile == null) {
                ShellTile.Create(new Uri(info.UrlPath, UriKind.Relative), tileData, true);
            
            } else {
                tile.Update(tileData);
            }
        }


        public static void DeleteTile(IPlantViewModel pvm)
        {
            var t = GetShellTile(pvm);
            if (t != null)
            {
                t.Delete();
                pvm.HasTile = false;
            }

            // no need to update infos
        }


        public static void DeleteAllTiles()
        {
            foreach (var tile in ShellTile.ActiveTiles.AsEnumerable())
            {
                tile.Delete();
            }
        }

    }


}
