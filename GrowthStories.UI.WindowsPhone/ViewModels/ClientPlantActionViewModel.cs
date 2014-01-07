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


    public class ClientPlantPhotographViewModel : PlantPhotographViewModel, IPlantPhotographViewModel
    {


        private BitmapImage _PhotoSource;
        public BitmapImage PhotoSource
        {
            get
            {
                return _PhotoSource;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _PhotoSource, value);
            }
        }

        private BitmapImage _TimelinePhotoSource;
        public BitmapImage TimelinePhotoSource
        {
            get
            {
                return _TimelinePhotoSource;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _TimelinePhotoSource, value);
            }
        }




        private void SetPhotos(Photo p)
        {
            if (p == null || p.Uri == null)
                return;
            TimelinePhotoSource = new BitmapImage(new Uri(p.Uri, UriKind.RelativeOrAbsolute))
            {
                CreateOptions = BitmapCreateOptions.DelayCreation,
                DecodePixelType = DecodePixelType.Physical,
                DecodePixelHeight = 220,
                //DecodePixelWidth = 450
            };
            //var fakeUri = new Uri("http://upload.wikimedia.org/wikipedia/commons/e/e3/CentaureaCyanus-bloem-kl.jpg", UriKind.RelativeOrAbsolute);
            //var fakeUri = new Uri("http://dennari-macbook.lan:8080/_ah/img/uAtkbmrhY17K87WPNEyaZA?paska=moi.jpg", UriKind.RelativeOrAbsolute);

            //TimelinePhotoSource = new BitmapImage(fakeUri)
            //{
            //    CreateOptions = BitmapCreateOptions.DelayCreation,
            //    DecodePixelType = DecodePixelType.Physical,
            //    DecodePixelHeight = 220,

            //    //DecodePixelWidth = 450
            //};
            //TimelinePhotoSource.ImageOpened += TimelinePhotoSource_ImageOpened;
            //TimelinePhotoSource.ImageFailed += TimelinePhotoSource_ImageFailed;

            //PhotoSource = new BitmapImage(fakeUri)
            //{
            //    CreateOptions = BitmapCreateOptions.DelayCreation,
            //    DecodePixelType = DecodePixelType.Physical,
            //    DecodePixelHeight = (int)p.Height,
            //    //DecodePixelWidth = (int)p.Width
            //};
            PhotoSource = new BitmapImage(new Uri(p.Uri, UriKind.RelativeOrAbsolute))
            {
                CreateOptions = BitmapCreateOptions.DelayCreation,
                DecodePixelType = DecodePixelType.Physical,
                DecodePixelHeight = (int)p.Height,
                //DecodePixelWidth = (int)p.Width
            };
        }

        private void TimelinePhotoSource_ImageFailed(object sender, System.Windows.ExceptionRoutedEventArgs e)
        {
            var ee = e.ErrorException;
        }

        private void TimelinePhotoSource_ImageOpened(object sender, System.Windows.RoutedEventArgs e)
        {
            var ee = e.OriginalSource;

        }

        public ClientPlantPhotographViewModel(IGSAppViewModel app, PlantActionState state = null)
            : base(app, state)
        {

            var photoStream = this.WhenAnyValue(x => x.Photo, x => x)
                .Where(x => x != default(Photo));


            if (state != null)
                photoStream.StartWith(state.Photo);

            photoStream.ObserveOn(RxApp.MainThreadScheduler).Subscribe(x =>
            {
                if (x != null)
                    SetPhotos(x);
            });

            PhotoChooserCommand.ObserveOn(RxApp.MainThreadScheduler).Subscribe(_ =>
            {
                Chooser.Show();
            });

        }


        public override void AddCommandSubscription(object p)
        {
            //this.SendCommand(new Photograph(this.State.EntityId, this.State.PlantId, this.Note, this.Path), true);
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


        async Task t_Completed(object sender, PhotoResult e)
        {
            //throw new NotImplementedException();
            var image = e.ChosenPhoto;
            if (e.TaskResult == TaskResult.OK && image.CanRead && image.Length > 0)
            {
                Photo = await image.SavePhotoToLocalStorageAsync();
            }
        }




    }


















}
