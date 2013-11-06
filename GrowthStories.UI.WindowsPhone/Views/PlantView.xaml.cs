using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Growthstories.UI.ViewModel;
using ReactiveUI;
using System.Windows.Input;
using System.Reactive.Disposables;
using Growthstories.Domain.Entities;
//using Growthstories.UI.WindowsPhone.ViewModels;

namespace Growthstories.UI.WindowsPhone
{
    public partial class PlantView : UserControl, IViewFor<PlantViewModel>
    {

        bool isPinch = false;
        bool isDrag = false;
        //double initialAngle;
        bool isGestureOnTarget = true;

        public PlantView()
        {
            InitializeComponent();
            this.WhenNavigatedTo(ViewModel, () =>
            {
                DataContext = ViewModel;
                ViewModel.PinCommand.Subscribe(x => this.AddTile((PlantState)x));
                ViewModel.ScrollCommand.Subscribe(x => this.ScrollTo((PlantActionViewModel)x));
                return Disposable.Empty;
            });
        }

        private void ScrollTo(PlantActionViewModel item)
        {
            this.TimeLine.ScrollTo(item);
        }

        private void AddTile(PlantState p)
        {
            // find the tile object for the application tile that using "flip" contains string in it.


            //var data = new FlipTileData()
            //{
            //    Title = p.Name,
            //    BackgroundImage = new Uri(p.ProfilepicturePath, UriKind.Relative),
            //    SmallBackgroundImage = new Uri(p.ProfilepicturePath, UriKind.Relative),
            //    WideBackgroundImage = new Uri(p.ProfilepicturePath, UriKind.Relative)
            //};
            var imgUri = new Uri("/Assets/Icons/application_icon_179.png", UriKind.Relative);
            var BackImgUri = new Uri("/Assets/Tiles/GSFlipTileLarge.png", UriKind.Relative);
            var data = new FlipTileData()
            {
                Title = p.Name,
                BackgroundImage = imgUri,
                SmallBackgroundImage = imgUri,
                WideBackgroundImage = imgUri,
                BackBackgroundImage = BackImgUri,
                WideBackBackgroundImage = BackImgUri,
                WideBackContent = p.Name,
                BackContent = p.Name
            };
            ShellTile oTile = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains(p.Id.ToString()));
            if (oTile != null)
                oTile.Update(data);
            else
                ShellTile.Create(new Uri(String.Format("/MainWindow.xaml?plant={0}", p.Id.ToString()), UriKind.Relative), data, true);
        }


        public PlantViewModel ViewModel
        {
            get { return (PlantViewModel)GetValue(ViewModelProperty); }
            set
            {
                if (value != null)
                {
                    SetValue(ViewModelProperty, value);
                }
            }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(IRoutableViewModel), typeof(PlantView), new PropertyMetadata(null));


        object IViewFor.ViewModel { get { return this.ViewModel; } set { this.ViewModel = (PlantViewModel)value; } }

    }
}