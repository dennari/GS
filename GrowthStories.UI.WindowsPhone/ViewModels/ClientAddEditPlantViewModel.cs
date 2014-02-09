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
                .Subscribe(_ =>
            {
                this.Log().Info("raisepropchange for profilepicture");
                this.raisePropertyChanged("Profilepicture");
            });
        }


        void Profilepicture_ImageFailed(object sender, System.Windows.ExceptionRoutedEventArgs e)
        {
            throw e.ErrorException;
        }


        public BitmapImage Profilepicture
        {
            get
            {

                if (Photo == null || Photo.Uri == null)
                {
                    return null;
                }

                return new BitmapImage(new Uri(Photo.Uri, UriKind.RelativeOrAbsolute))
                {
                    CreateOptions = BitmapCreateOptions.DelayCreation,
                    DecodePixelType = DecodePixelType.Logical,
                    DecodePixelHeight = 396
                };
            }
        }

        //protected BitmapImage _Profilepicture;
        //public BitmapImage Profilepicture
        //{
        //    get
        //    {
        //        if (_Profilepicture == null)
        //        {
        //            _Profilepicture = new BitmapImage()
        //            {
        //                CreateOptions = BitmapCreateOptions.DelayCreation,
        //                DecodePixelType = DecodePixelType.Physical
        //            };
        //            Profilepicture.ImageFailed += Profilepicture_ImageFailed;
        //        }
        //        return _Profilepicture;
        //    }
        //}


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
