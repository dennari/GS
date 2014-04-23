using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using EventStore.Logging;
using Growthstories.Core;
using Growthstories.Domain.Entities;
using Growthstories.UI.ViewModel;
using GrowthStories.UI.WindowsPhone.BA;
using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using ReactiveUI;


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

            UpdateHasTile();
        }

        // only call from RxApp.MainThreadScheduler thread
        public bool CreateOrUpdateTile()
        {
            var tile = Current;
            if (tile != null)
            {
                TilesHelper.UpdateTileCommand.Execute(Vm);

            }
            else
            {
                var info = TilesHelper.CreateTileUpdateInfo(Vm);
                ShellTile.Create(new Uri(info.UrlPath, UriKind.Relative), GSTileUtils.GetTileData(info), true);
                TilesHelper.WriteTileUpdateInfo(info);
            }
            _Current = null;
            HasTile = true;
            return true;
        }


        public void UpdateHasTile()
        {
            var k = new ReactiveCommand();
            k.ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ =>
            {
                _Current = TilesHelper.GetShellTile(Vm);
                HasTile = Current != null;
                this.Log().Info("updating has tile to {0}", HasTile);
            });

            k.Execute(null);
        }

        // only call from RxApp.MainThreadScheduler thread
        public bool DeleteTile()
        {
            if (HasTile)
                TilesHelper.DeleteTile(Vm);
            TilesHelper.ClearTileUpdateInfo(Vm);
            ClearSubscriptions();
            _Current = null;
            HasTile = false;
            return true;
        }


        private ShellTile _Current;
        private ShellTile Current { get { return _Current; } } 


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
                else
                {
                    // watering schedule is set to null when there is no (anymore) actions etc.
                    TriggerTileUpdate();
                }
            }));

            // watch for watering schedule enabled updates
            subs.Add(Vm.WhenAnyValue(w => w.IsWateringScheduleEnabled).Subscribe(u =>
            {
                TriggerTileUpdate();
            }));

            subs.Add(Vm.Actions.ItemsAdded.Subscribe(a =>
            {
                if (a.ActionType == PlantActionType.PHOTOGRAPHED)
                {
                    TriggerTileUpdate();
                }
            }));

            // for some reason did not work with .Concat(...)
            subs.Add(Vm.Actions.ItemsRemoved.Subscribe(a =>
            {
                if (a.ActionType == PlantActionType.PHOTOGRAPHED)
                {
                    TriggerTileUpdate();
                }
            }));

            subs.Add(Vm.WhenAnyValue(a => a.Photo).Subscribe(_ => TriggerTileUpdate()));
            subs.Add(Vm.WhenAnyValue(a => a.Name).Subscribe(_ => TriggerTileUpdate()));

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
            //this.Log().Info("triggered update of tileinfo for {0}", Vm.Name);
            TilesHelper.UpdateTileCommand.Execute(Vm);
        }


    }


    class TilesHelper
    {

        private static ILog Logger = LogFactory.BuildLogger(typeof(TilesHelper));


        public static string GetSettingsKey(IPlantViewModel pvm)
        {
            return GSTileUtils.SETTINGS_KEY + pvm.UrlPath;
        }


        public static void WriteTileUpdateInfo(TileUpdateInfo info)
        {
            using (Mutex mutex = new Mutex(false, GSTileUtils.SETTINGS_MUTEX))
            {
                try { mutex.WaitOne(); }
                catch (AbandonedMutexException ex)
                {
                    Logger.Info("WriteTileUpdateInfo / Abandoned mutex: {0}", ex.ToStringExtended());
                    return;
                }
                catch (Exception ex)
                {
                    Logger.Info("WriteTileUpdateInfo / Unexpected exception in mutex.WaitOne {0}", ex.ToStringExtended());
                    return;
                }

                try
                {
                    var settings = IsolatedStorageSettings.ApplicationSettings;

                    var s = JsonConvert.SerializeObject(info, Formatting.None);

                    settings.Remove(GSTileUtils.GetSettingsKey(info));
                    settings.Add(GSTileUtils.GetSettingsKey(info), s);
                    settings.Save();
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }


        //
        // To be called when database is cleared
        //
        public static void ClearAllTileUpdateInfos()
        {
            var kludge = new ReactiveCommand();
            kludge.ObserveOn(RxApp.MainThreadScheduler).Subscribe(_ =>
            {
                using (Mutex mutex = new Mutex(false, GSTileUtils.SETTINGS_MUTEX))
                {
                    try { mutex.WaitOne(); }
                    catch (AbandonedMutexException ex)
                    {
                        Logger.Info("ClearAllTileUpdateInfos / Abandoned mutex: {0}", ex.ToStringExtended());
                        return;
                    }
                    catch (Exception ex)
                    {
                        Logger.Info("ClearAllTileUpdateInfos / Unexpected exception in mutex.WaitOne {0}", ex.ToStringExtended());
                        return;
                    }

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
                GSTileUtils.UpdateTiles();
            });
            kludge.Execute(null);
        }


        // 
        // To be called whenever plants are removed
        //
        public static void ClearTileUpdateInfo(IPlantViewModel pvm)
        {
            using (Mutex mutex = new Mutex(false, GSTileUtils.SETTINGS_MUTEX))
            {
                try { mutex.WaitOne(); }
                catch (AbandonedMutexException ex)
                {
                    Logger.Info("ClearTileUpdateInfo / Abandoned mutex: {0}", ex.ToStringExtended());
                    return;
                }
                catch (Exception ex)
                {
                    Logger.Info("ClearTileUpdateInfo / Unexpected exception in mutex.WaitOne {0}", ex.ToStringExtended());
                    return;
                }

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
            GSTileUtils.UpdateApplicationTile();
        }


        public static ShellTile GetShellTile(IPlantViewModel pvm)
        {
            return GSTileUtils.GetShellTile(pvm.UrlPathSegment);
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
                .OrderByDescending(x => x.Created)
                .Select(x => new Uri(x.Photo.LocalUri))
                .Take(9)
                .ToList();

            if (pvm.Photo != null)
            {
                //Logger.Info("localFullPath is {0}, Uri is {1}, LocalUri is {2}, RemoteUri is {3}",
                //    pvm.Photo.LocalFullPath, pvm.Photo.Uri, pvm.Photo.LocalUri, pvm.Photo.RemoteUri);
            }

            if (photoUris.Count == 0)
            {
                photoUris.Add(new System.Uri("appdata:/Assets/Icons/NoImageNoText.png"));
            }

            info.PhotoUris = photoUris;

            if (pvm.Photo != null && pvm.Photo.LocalUri != null)
            {
                try
                {
                    info.ProfilePhotoUri = new Uri(pvm.Photo.LocalUri);
                }
                catch { Logger.Warn("could not parse profilephotouri"); }
            }
            if (info.ProfilePhotoUri == null)
            {
                info.ProfilePhotoUri = new System.Uri("appdata:/Assets/Icons/NoImageNoText.png");
            }

            return info;
        }


        private static void UpdateTileAndInfo(IPlantViewModel pvm)
        {
            pvm.Log().Info("updating tile and info for {0} in thread {1}", pvm.Name, Thread.CurrentThread.Name);
            //throw new Exception("moroo");

            var vm = (PlantViewModel)pvm;
            if (vm.State.IsDeleted)
            {
                ClearTileUpdateInfo(pvm);
                GSTileUtils.UpdateApplicationTile();
                return;
            }

            var tile = GetShellTile(pvm);
            TileUpdateInfo info = CreateTileUpdateInfo(pvm);
            WriteTileUpdateInfo(info);

            if (tile != null)
            {
                GSTileUtils.UpdateTile(info);
            }

            GSTileUtils.UpdateApplicationTile();
        }


        static IReactiveCommand _UpdateTileCommand;
        public static IReactiveCommand UpdateTileCommand
        {
            get
            {
                if (_UpdateTileCommand == null)
                {
                    _UpdateTileCommand = new ReactiveCommand();
                    _UpdateTileCommand
                        .OfType<IPlantViewModel>()
                        .Throttle(TimeSpan.FromMilliseconds(750), RxApp.TaskpoolScheduler)
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(x =>
                        {
                            try
                            {
                                UpdateTileAndInfo(x);
                            }
                            catch (Exception ex)
                            {
                                RxApp.MainThreadScheduler.Schedule(() =>
                                {
                                    throw ex;
                                });
                            }
                        });
                }

                return _UpdateTileCommand;
            }
        }


        public static void DeleteTile(IPlantViewModel pvm)
        {
            var t = GetShellTile(pvm);
            if (t != null)
            {
                SafelyDeleteTile(t);
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
                    SafelyDeleteTile(tile);
                }
                cnt++;
            }
            ClearAllTileUpdateInfos();
        }


        public static void SafelyDeleteTile(ShellTile tile)
        {
            using (Mutex mutex = new Mutex(false, GSTileUtils.SHELL_MUTEX))
            {
                try { mutex.WaitOne(); }
                catch (AbandonedMutexException ex)
                {
                    Logger.Info("SafelyDeleteTile / Abandoned mutex: {0}", ex.ToStringExtended());
                    return;
                }
                catch (Exception ex)
                {
                    Logger.Info("SafelyDeleteTile / Unexpected exception in mutex.WaitOne {0}", ex.ToStringExtended());
                    return;
                }

                try
                {
                    tile.Delete();
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }

    }

}
