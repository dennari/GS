using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Growthstories.Sync;
using Growthstories.UI.ViewModel;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using ReactiveUI;


namespace Growthstories.UI.WindowsPhone.ViewModels
{

    public static class Mixins
    {
        public static void SetSource(this BitmapImage i, Photo x)
        {
            i.DecodePixelHeight = (int)x.Height;
            i.DecodePixelWidth = (int)x.Width;
            i.UriSource = new Uri(x.Uri, UriKind.RelativeOrAbsolute);
        }
    }


    public class ClientAddEditPlantViewModel : AddEditPlantViewModel
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>

        public ClientAddEditPlantViewModel(IGSAppViewModel app, IObservable<IGardenViewModel> gardenObservable, IPlantViewModel current = null)
            : base(app, gardenObservable, current)
        {

            this.ChooseProfilePictureCommand.Subscribe(_ => this.PhotoChooser.Show());

            this.WhenAnyValue(x => x.Photo, x => x)
                .Where(x => x != null)
                .Subscribe(x => Profilepicture.SetSource(x));
        }



        void Profilepicture_ImageFailed(object sender, System.Windows.ExceptionRoutedEventArgs e)
        {
            throw e.ErrorException;
        }

        protected BitmapImage _Profilepicture;
        public BitmapImage Profilepicture
        {
            get
            {
                if (_Profilepicture == null)
                {
                    _Profilepicture = new BitmapImage()
                    {
                        CreateOptions = BitmapCreateOptions.DelayCreation,
                        DecodePixelType = DecodePixelType.Physical
                    };
                    Profilepicture.ImageFailed += Profilepicture_ImageFailed;
                }
                return _Profilepicture;
            }
        }


        private PhotoChooserTask _PhotoChooser;
        protected PhotoChooserTask PhotoChooser
        {
            get
            {
                if (_PhotoChooser == null)
                {
                    var t = new PhotoChooserTask();
                    t.Completed += async (s, e) => await t_Completed(s, e);
                    //t.Completed += t_Completed;
                    t.ShowCamera = true;
                    _PhotoChooser = t;
                }
                return _PhotoChooser;
            }
        }

        private ReactiveCommand _ViewFSCommand;
        public ReactiveCommand ViewFSCommand
        {
            get
            {

                if (_ViewFSCommand == null)
                {
                    _ViewFSCommand = new ReactiveCommand();
                    _ViewFSCommand.Subscribe(_ =>
                    {
                        if (this.Photo == null)
                            return;
                        FSView.Show();
                    });
                }
                return _ViewFSCommand;

            }
        }

        CustomMessageBox FSView
        {
            get
            {
                return new CustomMessageBox()
                 {
                     IsLeftButtonEnabled = false,
                     IsRightButtonEnabled = false,
                     Content = new Image()
                     {
                         Stretch = Stretch.UniformToFill,
                         Source = this.Profilepicture
                     },
                     IsFullScreen = true // Pivots should always be full-screen.
                 };
            }
        }

        private ReactiveCommand _CMOpen;
        public ReactiveCommand CMOpen
        {
            get
            {

                if (_CMOpen == null)
                {
                    _CMOpen = new ReactiveCommand();
                    _CMOpen.Subscribe(_ =>
                        {
                            //ChoosePhoto();
                        });
                }
                return _CMOpen;

            }
        }

        private ReactiveCommand _CMClose;
        public ReactiveCommand CMClose
        {
            get
            {

                if (_CMClose == null)
                {
                    _CMClose = new ReactiveCommand();
                    _CMOpen.Subscribe(_ =>
                    {
                        //ChoosePhoto();
                    });
                }
                return _CMClose;

            }
        }


        async Task t_Completed(object sender, PhotoResult e)
        {
            var res = await ViewHelpers.HandlePhotoChooserCompleted(e, App.ShowPopup);
            if (res != null)
            {
                Photo = res;
            }
        }

    }
}
