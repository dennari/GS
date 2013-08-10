using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Growthstories.UI.ViewModel;
using Microsoft.Phone.Tasks;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Messaging;
using Growthstories.Sync;
using Growthstories.Domain;
using Growthstories.UI;
using System.Windows.Media.Imaging;
using System.IO;
using Windows.Storage.Streams;
using GrowthStories.UI.WindowsPhone;
using GalaSoft.MvvmLight.Command;
using Microsoft.Phone.Controls;
using System.Windows.Controls;
using System.Windows.Media;
using ReactiveUI;

namespace GrowthStories.UI.WindowsPhone.ViewModels
{
    class ClientAddPlantViewModel : AddPlantViewModel
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public ClientAddPlantViewModel(IMessenger messenger, IUserService ctx, IMessageBus handler, INavigationService nav)
            : base(messenger, ctx, handler, nav)
        {

        }




        private PhotoChooserTask _Chooser;
        protected PhotoChooserTask Chooser
        {
            get
            {
                if (_Chooser == null)
                {
                    var t = new PhotoChooserTask();
                    t.Completed += async (s, e) => await t_Completed(s, e);
                    //t.Completed += t_Completed;
                    t.ShowCamera = true;
                    _Chooser = t;
                }
                return _Chooser;
            }
        }

        private RelayCommand _ViewFSCommand;
        public RelayCommand ViewFSCommand
        {
            get
            {

                if (_ViewFSCommand == null)
                    _ViewFSCommand = new RelayCommand(() =>
                    {
                        if (this.ProfilepicturePath == null)
                            return;
                        FSView.Show();
                    });
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
                         Source = this.ProfilePicture
                     },
                     IsFullScreen = true // Pivots should always be full-screen.
                 };
            }
        }

        private RelayCommand _CMOpen;
        public RelayCommand CMOpen
        {
            get
            {

                if (_CMOpen == null)
                    _CMOpen = new RelayCommand(() =>
                    {
                        //ChoosePhoto();
                    });
                return _CMOpen;

            }
        }

        private RelayCommand _CMClose;
        public RelayCommand CMClose
        {
            get
            {

                if (_CMClose == null)
                    _CMClose = new RelayCommand(() =>
                    {
                        //ChoosePhoto();
                    });
                return _CMClose;

            }
        }

        protected override void ChoosePhoto()
        {
            base.ChoosePhoto();
            Chooser.Show();

        }

        async Task t_Completed(object sender, PhotoResult e)
        {
            //throw new NotImplementedException();
            if (e.TaskResult == TaskResult.OK && e.ChosenPhoto.CanRead && e.ChosenPhoto.Length > 0)
            {
                this.ProfilePictureButtonText = "";
                await this.Process(e.ChosenPhoto);
            }
        }

        async Task Process(Stream image)
        {
            var buffer = image.ToBuffer();
            this.ProfilepicturePath = await buffer.SaveAsync();
            var o = buffer.Orientation();
            ProfilePicture.DecodePixelWidth = o == ImagingExtensions.OrientationType.LANDSCAPE ? 1280 : 0;
            ProfilePicture.DecodePixelHeight = o == ImagingExtensions.OrientationType.LANDSCAPE ? 0 : 1280;
            image.Position = 0;
            ProfilePicture.SetSource(image);

        }

        protected BitmapImage _ProfilePicture;
        public BitmapImage ProfilePicture
        {
            get
            {
                if (_ProfilePicture == null)
                    _ProfilePicture = new BitmapImage();
                return _ProfilePicture;
            }
        }

    }
}
