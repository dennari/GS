using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Growthstories.UI.ViewModel;
using Microsoft.Phone.Tasks;
using Growthstories.Sync;
using Growthstories.Domain;
using Growthstories.UI;
using System.Windows.Media.Imaging;
using System.IO;
using Windows.Storage.Streams;
using Growthstories.UI.WindowsPhone;
using Microsoft.Phone.Controls;
using System.Windows.Controls;
using System.Windows.Media;
using ReactiveUI;
using Growthstories.Domain.Entities;

namespace Growthstories.UI.WindowsPhone.ViewModels
{
    public class ClientAddPhotographViewModel : AddPhotographViewModel
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public ClientAddPhotographViewModel(PlantState state, IGSApp app)
            : base(state, app)
        {


            //this.PhotoChooserCommand.Execute(null);
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

        protected ReactiveCommand _PhotoChooserCommand;
        public ReactiveCommand PhotoChooserCommand
        {
            get
            {
                if (_PhotoChooserCommand == null)
                {
                    _PhotoChooserCommand = new ReactiveCommand();
                    _PhotoChooserCommand.Subscribe(_ =>
                    {
                        Chooser.Show();
                    });
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
                var buffer = image.ToBuffer();
                this.Path = new Uri(await buffer.SaveAsync(), UriKind.Relative);
                var o = buffer.Orientation();
                Photo.DecodePixelWidth = o == ImagingExtensions.OrientationType.LANDSCAPE ? 1280 : 0;
                Photo.DecodePixelHeight = o == ImagingExtensions.OrientationType.LANDSCAPE ? 0 : 1280;
                image.Position = 0;
                Photo.SetSource(image);
            }
        }


        protected BitmapImage _Photo;
        public BitmapImage Photo
        {
            get
            {
                if (_Photo == null)
                    _Photo = new BitmapImage();
                return _Photo;
            }
        }

    }
}
