﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Reactive.Linq;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.ComponentModel;
using Growthstories.UI.ViewModel;
using ReactiveUI;
using System.Reactive.Disposables;
using Microsoft.Phone.Tasks;

namespace Growthstories.UI.WindowsPhone
{
    public class PlantViewBase : GSView<IPlantViewModel>
    {

    }

    public partial class PlantView : PlantViewBase
    {


        public PlantView()
        {
            InitializeComponent();
            Height = Double.NaN;

            if (ViewModel != null)
            {
                OnViewModelChanged(ViewModel);
            }

            //this.WhenAnyValue(x => (int?)x.ViewModel.MissedCount)
            //    .Subscribe(x =>
            //    {
            //        if (Tile != null)
            //        {
            //            this.CreateOrUpdateTile();
            //        }
            //    });
        }


        private ShellTile _Tile;
        public ShellTile Tile
        {
            get
            {
                if (_Tile == null)
                {
                    _Tile = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains(ViewModel.UrlPathSegment));
                }
                return _Tile;
            }
        }



        IDisposable PinCommandSubscription = Disposable.Empty;
        IDisposable ShareCommandSubscription = Disposable.Empty;
        IDisposable DeleteCommandSubscription = Disposable.Empty;


        protected override void OnViewModelChanged(IPlantViewModel vm)
        {

            PinCommandSubscription.Dispose();
            _Tile = null;
            if (Tile != null)
                ViewModel.HasTile = true;

            PinCommandSubscription = vm.PinCommand.ObserveOn(RxApp.MainThreadScheduler).Subscribe(_ =>
            {
                if (Tile == null)
                    CreateOrUpdateTile();
                else
                    DeleteTile();
            });

            ShareCommandSubscription.Dispose();
            ShareCommandSubscription = vm.ShareCommand.ObserveOn(RxApp.MainThreadScheduler).Subscribe(_ =>
            {
                Share(vm);
            });

            if (vm.UserId == vm.App.User.Id) {
                Margin = new Thickness(0, 0, 0, 72);
            } else {
                Margin = new Thickness(0, 0, 0, 0);
            }

            DeleteCommandSubscription.Dispose();
            DeleteCommandSubscription = vm.DeleteCommand.ObserveOn(RxApp.MainThreadScheduler).Subscribe(_ =>
            {
                DeleteTile(vm);
            });
        }


        public static void DeleteTile(IPlantViewModel pvm)
        {
            var t = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains(pvm.UrlPathSegment));
            if (t != null)
            {
                t.Delete();
                pvm.HasTile = false; 
            }
        }

        private void Share(IPlantViewModel vm)
        {
            ShareLinkTask shareLinkTask = new ShareLinkTask();

            shareLinkTask.Title = "Story of " + vm.Name;
            shareLinkTask.LinkUri = new Uri(
                "http://www.growthstories.com/plant/" + vm.UserId + "/" + vm.Id , UriKind.Absolute);
            shareLinkTask.Message = "Check out how my plant " + vm.Name + " is doing!";

            shareLinkTask.Show();
        }


        private void DeleteTile()
        {
            if (Tile != null)
                Tile.Delete();
            ViewModel.HasTile = false;
        }


        private void CreateOrUpdateTile()
        {
            FlipTileData TileData = new FlipTileData()
            {
                Title = ViewModel.Name.ToUpper(),
                BackTitle = ViewModel.Name.ToUpper(),
                //BackContent = "GROWTH STORIES",
                //WideBackContent = "GROWTH STORIES",
                //Count = ViewModel.MissedCount.HasValue && ViewModel.MissedCount.Value > 0 ? ViewModel.MissedCount : null,
                BackgroundImage = new System.Uri("appdata:/Assets/Icons/NoImageNoText.png"),
                BackBackgroundImage = new System.Uri("appdata:/Assets/Icons/NoImageNoText.png"),
                
                //SmallBackgroundImage = [small Tile size URI],
                //BackgroundImage = [front of medium Tile size URI],
                //BackBackgroundImage = [back of medium Tile size URI],
                //WideBackgroundImage = [front of wide Tile size URI],
                //WideBackBackgroundImage = [back of wide Tile size URI],
            };

            if (Tile == null)
                ShellTile.Create(new Uri(ViewModel.UrlPath, UriKind.Relative), TileData, false);
            else
                Tile.Update(TileData);
            ViewModel.HasTile = true;
        }

        //private void PlantActionView_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        //{
        //    var plantActionView = (PlantActionView)sender;
        //    //var plant = ViewModel.SelectedItem;
        //    ViewModel.ActionTapped.Execute(plantActionView.ViewModel);
        //}

    }
}