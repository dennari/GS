using Growthstories.Domain.Messaging;
using Growthstories.UI.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ReactiveUI;
using System.Reactive.Linq;
using System.Reactive;
using Microsoft.Phone.Tasks;
using Growthstories.Domain.Entities;
using Growthstories.Sync;

namespace Growthstories.UI.WindowsPhone.ViewModels
{


    public class ClientPlantPhotographViewModel : PlantPhotographViewModel
    {



        protected BitmapImage _PhotoSource;
        public BitmapImage PhotoSource
        {
            get
            {
                return _PhotoSource;
            }
            set
            {
                _PhotoSource = value;
                this.RaisePropertyChanged();
            }
        }
        public ClientPlantPhotographViewModel(string photo, DateTimeOffset created)
            : base(photo, created)
        {
            if (Photo != null)
                this.PhotoSource = new BitmapImage(new Uri(Photo.LocalUri, UriKind.RelativeOrAbsolute));
        }

        public ClientPlantPhotographViewModel()
            : base()
        {

            if (Photo != null)
                this.PhotoSource = new BitmapImage(new Uri(Photo.LocalUri, UriKind.RelativeOrAbsolute));
        }



        private PhotoChooserTask _Chooser;
        public PhotoChooserTask Chooser
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

        protected MockReactiveCommand _PhotoChooserCommand;
        public IReactiveCommand PhotoChooserCommand
        {
            get
            {
                if (_PhotoChooserCommand == null)
                {
                    _PhotoChooserCommand = new MockReactiveCommand(_ => Chooser.Show());

                }
                return _PhotoChooserCommand;
            }
        }


        async Task t_Completed(object sender, PhotoResult e)
        {
            //throw new NotImplementedException();
            var image = e.ChosenPhoto;
            if (e.TaskResult == TaskResult.OK && image.CanRead && image.Length > 0)
            {
                //Photo = await image.SavePhotoToLocalStorageAsync();
                var img = new BitmapImage();
                img.SetSource(e.ChosenPhoto);
                this.PhotoSource = img;

            }
        }




    }


















}
