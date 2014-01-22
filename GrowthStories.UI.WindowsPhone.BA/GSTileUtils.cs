
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using Growthstories.Domain.Messaging;
using Growthstories.UI.Services;
using Growthstories.UI.ViewModel;
using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using ReactiveUI;
using Growthstories.Domain.Entities;


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


        public TileUpdateInfo()
        {
            Interval = null;
            Last = null;
        }

    }




    public class GSTileUtils
    {


        public const string SETTINGS_KEY = "periodicTaskInfo";
        public const string SETTINGS_MUTEX = "GSSettingsMutex";
        public const string DELETE_MUTEX = "GSTileUtilsDeleteMutex";


        public static string GetSettingsKey(IPlantViewModel pvm)
        {
            return SETTINGS_KEY + pvm.UrlPath;
        }


        public static string GetSettingsKey(TileUpdateInfo info)
        {
            return SETTINGS_KEY + info.UrlPath;
        }


        public static void WriteTileUpdateInfo(TileUpdateInfo info)
        {
            using (Mutex mutex = new Mutex(false, SETTINGS_MUTEX))
            {

                try { mutex.WaitOne(); }
                catch { } // catch exceptions associated with abandoned mutexes

                try
                {
                    var settings = IsolatedStorageSettings.ApplicationSettings;

                    var s = JsonConvert.SerializeObject(info, Formatting.None);

                    settings.Remove(GetSettingsKey(info));
                    settings.Add(GetSettingsKey(info), s);
                    settings.Save();

                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
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
                            missedWc = wideContent = PlantScheduler.NotificationText(
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



        public static void SubscribeForTileUpdates(IGardenViewModel garden)
        {
            // subscribe to changes of each watering scheduler

            garden.Plants.ItemsAdded
                .Subscribe(x =>
            {
                //  watch for watering scheduler updates
                x.WhenAnyValue(z => z.WateringScheduler).Subscribe(u =>
                {
                    u.WhenAnyValue(y => y.Missed)
                        .Subscribe(_ =>
                        {
                            UpdateTileAndInfoAfterDelay(x);
                        });
                });

                // watch for watering schedule enabled updates
                x.WhenAnyValue(w => w.IsWateringScheduleEnabled).Subscribe(u =>
                {
                    UpdateTileAndInfoAfterDelay(x);
                });

                x.Actions.ItemsAdded.Subscribe(a =>
                {
                    if (a.ActionType == PlantActionType.PHOTOGRAPHED)
                    {
                        UpdateTileAndInfoAfterDelay(x);
                    }
                });

                // also watch for added photos
                //x.Actions.ItemsAdded
                //    .Where(u => u.)
                //    .Subscribe(u =>
                //{
                //    UpdateTileAndInfoAfterDelay(x);
                //});

                x.Actions.ItemsRemoved.Subscribe(a =>
                {
                    if (a.ActionType == PlantActionType.WATERED || a.ActionType == PlantActionType.FERTILIZED)
                    {
                        UpdateTileAndInfoAfterDelay(x);
                    }
                });

            });

            garden.Plants.ItemsRemoved.Subscribe(x => ClearTileUpdateInfo(x));
        }



        //
        // To be called when database is cleared
        //
        public static void ClearAllTileUpdateInfos()
        {
            using (Mutex mutex = new Mutex(false, SETTINGS_MUTEX))
            {
                try { mutex.WaitOne(); }
                catch { } // catch exceptions associated with abandoned mutexes

                try
                {
                    var settings = IsolatedStorageSettings.ApplicationSettings;
                    settings.Clear();

                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
            UpdateTiles();
        }


        // 
        // To be called whenever plants are removed
        //
        public static void ClearTileUpdateInfo(IPlantViewModel pvm)
        {
            using (Mutex mutex = new Mutex(false, SETTINGS_MUTEX))
            {
                try { mutex.WaitOne(); }
                catch { } // catch exceptions associated with abandoned mutexes

                try
                {
                    var settings = IsolatedStorageSettings.ApplicationSettings;
                    if (settings.Contains(GetSettingsKey(pvm)))
                    {
                        settings.Remove(GetSettingsKey(pvm));
                        settings.Save();
                    }
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
            UpdateApplicationTile();
        }


        public static ShellTile GetShellTile(IPlantViewModel pvm)
        {
            return GetShellTile(pvm.UrlPathSegment);
        }


        public static ShellTile GetShellTile(string urlPathSegment)
        {
            return ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains(urlPathSegment));
        }


        private static void UpdateTiles(HashSet<TileUpdateInfo> infos)
        {
            foreach (var info in infos)
            {
                UpdateTile(info);
            }
        }


        public static TileUpdateInfo CreateTileUpdateInfo(IPlantViewModel pvm)
        {
            TileUpdateInfo info = new TileUpdateInfo();

            if (pvm.WateringSchedule != null
                && pvm.WateringScheduler != null
                && pvm.IsWateringScheduleEnabled
                && pvm.WateringScheduler.Interval != null
                && pvm.WateringScheduler.LastActionTime != null)
            {
                info.Interval = (TimeSpan)pvm.WateringSchedule.Interval;
                info.Last = (DateTimeOffset)pvm.WateringScheduler.LastActionTime;
            }

            info.UrlPathSegment = pvm.UrlPathSegment;
            info.UrlPath = pvm.UrlPath;
            info.Name = pvm.Name;

            var photoUris = pvm.Actions
                .Where(x => x.Photo != null && x.Photo.LocalUri != null)
                .Select(x => new Uri(x.Photo.LocalUri))
                .Take(9)
                .ToList();


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
                var missed = PlantScheduler.CalculateMissed((DateTimeOffset)info.Last, (TimeSpan)info.Interval);
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


        private static void UpdateTile(TileUpdateInfo info)
        {
            var tileData = GetTileData(info);
            var tile = GetShellTile(info.UrlPathSegment);

            if (tile != null)
            {
                tile.Update(tileData);
            }
        }


        private static void UpdateTileAndInfo(IPlantViewModel pvm)
        {

            var vm = (PlantViewModel)pvm;

            if (vm.State.IsDeleted)
            {
                ClearTileUpdateInfo(pvm);
                UpdateApplicationTile();
                return;
            }

            var tile = GetShellTile(pvm);
            TileUpdateInfo info = CreateTileUpdateInfo(pvm);
            WriteTileUpdateInfo(info);

            if (tile != null)
            {
                UpdateTile(info);
            }

            UpdateApplicationTile();
        }


        public static async void UpdateTileAndInfoAfterDelay(IPlantViewModel pvm)
        {
            await Task.Delay(2 * 1000);
            UpdateTileAndInfo(pvm);
        }


        public static void DeleteTile(IPlantViewModel pvm)
        {
            var t = GetShellTile(pvm);
            if (t != null)
            {
                t.Delete();
                // the plant takes care of this itself
                //pvm.HasTile = false;
            }

            // no need to update infos
        }


        public static void DeleteAllTiles()
        {
            int cnt = 0;
            foreach (var tile in ShellTile.ActiveTiles.AsEnumerable())
            {
                // application tile is first 
                if (cnt != 0)
                {
                    tile.Delete();
                }
                cnt++;
            }
            ClearAllTileUpdateInfos();
        }

    }



}
