
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows.Media;
using Growthstories.Domain.Messaging;
using Growthstories.UI.Services;
using Growthstories.UI.ViewModel;
using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using Growthstories.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUI;
using System.Reactive.Linq;
using System.Diagnostics;


namespace GrowthStories.UI.WindowsPhone.BA
{


    public class TileUpdateInfo
    {

        [JsonProperty]
        public DateTimeOffset? Last { get; set; }

        [JsonProperty]
        public TimeSpan? Interval { get; set; }

        [JsonProperty]
        public String UrlPathSegment { get; set; }

        [JsonProperty]
        public String UrlPath { get; set; }

        [JsonProperty]
        public List<Uri> PhotoUris { get; set; }

        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public Uri ProfilePhotoUri { get; set; }

        public TileUpdateInfo()
        {
            Interval = null;
            Last = null;
        }

    }


    // Tile operations needed by the background agent
    // other tile-related stuff is in the TileHelper and TilesHelper classes
    // in GrowthStories.UI.WindowsPhone
    //
    public class GSTileUtils
    {


        public const string SETTINGS_KEY = "periodicTaskInfo";
        public const string SETTINGS_MUTEX = "GSSettingsMutex";
        public const string DELETE_MUTEX = "GSTileUtilsDeleteMutex";


        /**
        * Update tiles based on TileUpdateInfos
        * stored in IsolatedStorageSettings
        */
        public static void UpdateTiles()
        {
            var pti = ReadTileUpdateInfos();
            UpdateTiles(pti);

            UpdateApplicationTile();
        }


        public static HashSet<TileUpdateInfo> ReadTileUpdateInfos()
        {
            var ret = new HashSet<TileUpdateInfo>();

            using (Mutex mutex = new Mutex(false, SETTINGS_MUTEX))
            {

                try { mutex.WaitOne(); }
                catch { } // catch exceptions associated with abandoned mutexes

                try
                {
                    foreach (var pair in IsolatedStorageSettings.ApplicationSettings)
                    {
                        string key = pair.Key as string;
                        string val = pair.Value as string;

                        if (key != null && key.StartsWith(SETTINGS_KEY) && val != null)
                        {
                            var info = JsonConvert.DeserializeObject<TileUpdateInfo>(val);
                            if (info != null && info.Name != null)
                            {
                                ret.Add(info);
                            }
                        }
                    }
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }

            return ret;
        }


        public static ShellTile GetShellTile(string urlPathSegment)
        {
            return ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains(urlPathSegment));
        }


        public static string GetSettingsKey(TileUpdateInfo info)
        {
            return SETTINGS_KEY + info.UrlPath;
        }


        public static void UpdateApplicationTile()
        {
            var pti = ReadTileUpdateInfos();

            uint maxCount = 0;

            // if wideContent is set to null, the tile will retain the previous 
            // setting, therefore this must be an empty string instead of null
            string wideContent = "";

            double maxMissed = Double.MinValue;
            string missedWc = null;
            string nextWc = null;
            long minTicksToAction = long.MaxValue;

            foreach (var info in pti)
            {
                var data = GetTileData(info);

                if (info.Last != null && info.Interval != null)
                {
                    var missed = PlantScheduler.CalculateMissed((DateTimeOffset)info.Last, (TimeSpan)info.Interval);

                    if (missed > PlantScheduler.WINDOW)
                    {
                        if (missed > maxMissed)
                        {
                            maxMissed = missed;
                            missedWc = PlantScheduler.NotificationText(
                                    (TimeSpan)info.Interval, missed, ScheduleType.WATERING, info.Name);
                            maxCount = (uint)data.Count;
                        }

                    }
                    else
                    {

                        var next = PlantScheduler.ComputeNext((DateTimeOffset)info.Last, (TimeSpan)info.Interval);

                        var ticksToAction = next.Ticks;
                        if (ticksToAction < minTicksToAction)
                        {
                            minTicksToAction = ticksToAction;
                            var s = next.ToString("d");
                            nextWc = info.Name.ToUpper() + " should be watered on " + s;
                        }
                    }

                    // if watering is missed, we show a notification for the plant
                    // whose watering has been missed relatively most (where Missed is highest)
                    //
                    // if no watering is missed, we show notification on which plant
                    // should be watered next in absolute terms
                    //
                    //  -- JOJ 12.1.2014

                    if (missedWc != null)
                    {
                        wideContent = missedWc;
                    }
                    else if (nextWc != null)
                    {
                        wideContent = nextWc;
                    }

                }
            }

            Color clr;
            if (maxCount > 0)
            {
                clr = new Color();
                clr.A = 0xff;
                clr.R = 0xfa;
                clr.G = 0x68;
                clr.B = 0x00;

            }
            else
            {
                clr = new Color();
                clr.A = 0xff;
                clr.R = 0x93;
                clr.G = 0x1B;
                clr.B = 0x80;
            }

            var td = new IconicTileData()
            {
                Title = "Growth Stories",
                IconImage = new System.Uri("appdata:/Assets/Tiles/IconImage_03.png"),
                SmallIconImage = new System.Uri("appdata:/Assets/Tiles/SmallIconImage_03.png"),
                Count = (int)maxCount,

                // according to documentation the widecontent does not do 
                // automatic line wrapping but at least in my phone it does 
                // -- JOJ 11.1.2014
                WideContent1 = wideContent,
                BackgroundColor = clr
            };

            var appTile = ShellTile.ActiveTiles.First();
            appTile.Update(td);
        }



        private static void UpdateTiles(HashSet<TileUpdateInfo> infos)
        {
            foreach (var info in infos)
            {
                UpdateTile(info);
            }
        }

        public static CycleTileData GetTileData(TileUpdateInfo info)
        {
            var cnt = 0;

            if (info.Interval != null && info.Last != null)
            {
                var missed = PlantScheduler.CalculateMissed((DateTimeOffset)info.Last, (TimeSpan)info.Interval);
                cnt = PlantScheduler.GetMissedCount(missed);
            }

            var tileData = new CycleTileData()
            {
                Title = info.Name.ToUpper(),
                Count = cnt,
                CycleImages = info.PhotoUris,
                SmallBackgroundImage = info.ProfilePhotoUri
            };

            return tileData;
        }


        public static void UpdateTile(TileUpdateInfo info)
        {
            var tileData = GetTileData(info);
            var tile = GetShellTile(info.UrlPathSegment);

            if (tile != null)
            {
                ClearTile(tile);
                tile.Update(tileData);
            }
        }


        private static void ClearTile(ShellTile tile)
        {
            // needed to clear the previous images from cycle tile
            // from http://social.msdn.microsoft.com/Forums/wpapps/en-US/700b13e0-fc4d-401e-92c7-936379c23a1f/cycle-tile-clearing-previous-images?forum=wpdevelop
            //
            var clearTileData = new CycleTileData("<?xml version=\"1.0\" encoding=\"utf-8\"?><wp:Notification xmlns:wp=\"WPNotification\" Version=\"2.0\"> <wp:Tile Id=\"TileID\" Template=\"CycleTile\"> <wp:SmallBackgroundImage Action=\"Clear\" /> <wp:CycleImage1 Action=\"Clear\" /> <wp:CycleImage2 Action=\"Clear\" /> <wp:CycleImage3 Action=\"Clear\" /> <wp:CycleImage4 Action=\"Clear\" /> <wp:CycleImage5 Action=\"Clear\" /> <wp:CycleImage6 Action=\"Clear\" /> <wp:CycleImage7 Action=\"Clear\" /> <wp:CycleImage8 Action=\"Clear\" /> <wp:CycleImage9 Action=\"Clear\" /> <wp:Count Action=\"Clear\" /> <wp:Title Action=\"Clear\" /> </wp:Tile></wp:Notification>");
            tile.Update(clearTileData);
        }


    }


}
