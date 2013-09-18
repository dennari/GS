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
using GrowthStories.UI.WindowsPhone;
using ReactiveUI;
using System.Windows.Input;
using System.Reactive.Disposables;
using Growthstories.Domain.Entities;

namespace Growthstories.UI.WindowsPhone
{
    public partial class PlantView : UserControl, IViewFor<PlantViewModel>
    {

        bool isPinch = false;
        bool isDrag = false;
        double initialAngle;
        bool isGestureOnTarget = true;

        public PlantView()
        {
            InitializeComponent();
            this.WhenNavigatedTo(ViewModel, () =>
            {
                DataContext = ViewModel;
                ViewModel.PinCommand.Subscribe(x => this.AddTile((PlantState)x));
                return Disposable.Empty;
            });
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



        private void btnFlipTile_Click(object sender, RoutedEventArgs e)
        {
            // find the tile object for the application tile that using "flip" contains string in it.
            ShellTile oTile = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains("flip".ToString()));


            if (oTile != null && oTile.NavigationUri.ToString().Contains("flip"))
            {
                FlipTileData oFliptile = new FlipTileData();
                oFliptile.Title = "Hello WP8!!";
                oFliptile.Count = 11;
                oFliptile.BackTitle = "Updated Flip Tile";

                oFliptile.BackContent = "back of tile";
                oFliptile.WideBackContent = "back of the wide tile";

                oFliptile.SmallBackgroundImage = new Uri("Assets/Tiles/Flip/159x159.png", UriKind.Relative);
                oFliptile.BackgroundImage = new Uri("Assets/Tiles/Flip/336x336.png", UriKind.Relative);
                oFliptile.WideBackgroundImage = new Uri("Assets/Tiles/Flip/691x336.png", UriKind.Relative);

                oFliptile.BackBackgroundImage = new Uri("/Assets/Tiles/Flip/A336.png", UriKind.Relative);
                oFliptile.WideBackBackgroundImage = new Uri("/Assets/Tiles/Flip/A691.png", UriKind.Relative);
                oTile.Update(oFliptile);
                MessageBox.Show("Flip Tile Data successfully update.");
            }
            else
            {
                // once it is created flip tile
                Uri tileUri = new Uri("/MainPage.xaml?tile=flip", UriKind.Relative);
                ShellTileData tileData = this.CreateFlipTileData();
                ShellTile.Create(tileUri, tileData, true);
            }
        }

        private ShellTileData CreateFlipTileData()
        {
            return new FlipTileData()
            {
                Title = "Hi Flip Tile",
                BackTitle = "This is WP8 flip tile",
                BackContent = "Live Tile Demo",
                WideBackContent = "Hello Nokia Lumia 920",
                Count = 8,
                SmallBackgroundImage = new Uri("/Assets/Tiles/Flip/A159.png", UriKind.Relative),
                BackgroundImage = new Uri("/Assets/Tiles/Flip/A336.png", UriKind.Relative),
                WideBackgroundImage = new Uri("/Assets/Tiles/Flip/A691.png", UriKind.Relative),
            };
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

        // UIElement.ManipulationStarted indicates the beginning of a touch interaction. It tells us
        // that we went from having no fingers on the screen to having at least one finger on the screen.
        // It doesn't tell us what gesture this is going to become, but it can be useful for 
        // initializing your gesture handling code.
        private void OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
        }
        // UIElement.ManipulationDelta represents either a drag or a pinch.
        // If PinchManipulation == null, then we have a drag, corresponding to GestureListener.DragStarted, 
        // GestureListener.DragDelta, or GestureListener.DragCompleted.
        // If PinchManipulation != null, then we have a pinch, corresponding to GestureListener.PinchStarted, 
        // GestureListener.PinchDelta, or GestureListener.PinchCompleted.
        // 
        // In this sample we track drag and pinch state to illustrate how to manage transitions between 
        // pinching and dragging, but commonly only the pinch or drag deltas will be of interest, in which 
        // case determining when pinches and drags begin and end is not necessary.
        //
        // Note that the exact APIs for the event args are not quite the same as the ones in GestureListener.
        // Comments inside methods called from here will note where they diverge.
        private void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            bool oldIsPinch = isPinch;
            bool oldIsDrag = isDrag;
            isPinch = e.PinchManipulation != null;

            // The origin of the first manipulation after a pinch is completed always corresponds to the
            // primary touch point from the pinch, even if the secondary touch point is the one that 
            // remains active. In this sample we only want a drag to affect the rectangle if the finger
            // on the screen falls inside the rectangle's bounds, so if we've just finished a pinch,
            // we have to defer until the next ManipulationDelta to determine whether or not a new 
            // drag has started.
            isDrag = e.PinchManipulation == null && !oldIsPinch;

            // check for ending gestures
            if (oldIsDrag && !isDrag)
            {
                //this.OnDragCompleted();
            }
            if (oldIsPinch && !isPinch)
            {
                //this.OnPinchCompleted();
            }

            // check for continuing gestures
            if (oldIsDrag && isDrag)
            {
                //this.OnDragDelta(sender, e);
            }
            if (oldIsPinch && isPinch)
            {
                //this.OnPinchDelta(sender, e);
            }

            // check for starting gestures
            if (!oldIsDrag && isDrag)
            {
                // Once a manipulation has started on the UIElement, that element will continue to fire ManipulationDelta
                // events until all fingers have left the screen and we get a ManipulationCompleted. In this sample
                // however, we treat each transition between pinch and drag as a new gesture, and we only want to 
                // apply effects to our border control if the the gesture begins within the bounds of the border.
                //isGestureOnTarget = e.ManipulationContainer == border &&
                //        new Rect(0, 0, border.ActualWidth, border.ActualHeight).Contains(e.ManipulationOrigin);
                //this.OnDragStarted();
            }
            if (!oldIsPinch && isPinch)
            {
                //isGestureOnTarget = e.ManipulationContainer == border &&
                //        new Rect(0, 0, border.ActualWidth, border.ActualHeight).Contains(e.PinchManipulation.Original.PrimaryContact);
                //this.OnPinchStarted(sender, e);
            }
        }

        // UIElement.ManipulationCompleted indicates the end of a touch interaction. It tells us that
        // we went from having at least one finger on the screen to having no fingers on the screen.
        // If e.IsInertial is true, then it's also the same thing as GestureListener.Flick,
        // although the event args API for the flick case are different, as will be noted inside that method.
        private void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (isDrag)
            {
                isDrag = false;
                //this.OnDragCompleted();

                if (e.IsInertial)
                {
                    this.OnFlick(sender, e);
                }
            }

            if (isPinch)
            {
                isPinch = false;
                //this.OnPinchCompleted();
            }
        }

        private void OnFlick(object sender, ManipulationCompletedEventArgs e)
        {
            if (isGestureOnTarget)
            {
                // All of the properties on FlickGestureEventArgs have been replaced by the single property
                // FinalVelocities.LinearVelocity.  This method shows how to retrieve from FinalVelocities.LinearVelocity
                // the properties that used to be in FlickGestureEventArgs. Also, note that while the GestureListener
                // provided fairly precise directional information, small linear velocities here are rounded
                // to 0, resulting in flick vectors that are often snapped to one axis.

                //Point transformedVelocity = GetTransformNoTranslation(transform).Transform(e.FinalVelocities.LinearVelocity);

                double horizontalVelocity = e.FinalVelocities.LinearVelocity.X;
                double verticalVelocity = e.FinalVelocities.LinearVelocity.Y;

                ViewModel.FlickCommand.Execute(Tuple.Create(horizontalVelocity, verticalVelocity));

                //flickData.Text = string.Format("{0} Flick: Angle {1} Velocity {2},{3}",
                //    this.GetDirection(horizontalVelocity, verticalVelocity), Math.Round(this.GetAngle(horizontalVelocity, verticalVelocity)), horizontalVelocity, verticalVelocity);
            }
        }

    }
}