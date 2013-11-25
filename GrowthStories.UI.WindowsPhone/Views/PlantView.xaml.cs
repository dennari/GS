using System;
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
                Title = ViewModel.Name,
                BackTitle = ViewModel.Name,
                BackContent = "GROWTH STORIES",
                WideBackContent = "GROWTH STORIES",
                Count = ViewModel.MissedCount.HasValue && ViewModel.MissedCount.Value > 0 ? ViewModel.MissedCount : null,
                //SmallBackgroundImage = [small Tile size URI],
                //BackgroundImage = [front of medium Tile size URI],
                //BackBackgroundImage = [back of medium Tile size URI],
                //WideBackgroundImage = [front of wide Tile size URI],
                //WideBackBackgroundImage = [back of wide Tile size URI],
            };

            if (Tile == null)
                ShellTile.Create(new Uri(ViewModel.UrlPath, UriKind.Relative), TileData, true);
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