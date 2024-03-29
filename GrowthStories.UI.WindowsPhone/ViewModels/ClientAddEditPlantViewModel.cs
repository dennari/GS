﻿using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Growthstories.Domain.Entities;
using Growthstories.UI.ViewModel;
using Microsoft.Phone.Tasks;
using ReactiveUI;

namespace Growthstories.UI.WindowsPhone.ViewModels
{


    public class ClientAddEditPlantViewModel : AddEditPlantViewModel
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>

        public ClientAddEditPlantViewModel(
            IGSAppViewModel app,
            IObservable<IGardenViewModel> gardenObservable,
            Func<Tuple<PlantState, ScheduleState, ScheduleState>, IPlantViewModel> plantF,
            IReactiveCommand NotifyOfPlantCommand,
            IPlantViewModel current = null)
            : base(app, gardenObservable, plantF, NotifyOfPlantCommand, current)
        {

            this.ChooseProfilePictureCommand.Subscribe(_ => this.PhotoChooser.Show());

            this.WhenAnyValue(x => x.Photo, x => x)
                .Where(x => x != null)
                .Subscribe(_ =>
            {
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
