using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Growthstories.Core;
using Growthstories.Domain.Entities;
using Growthstories.Sync;
using Growthstories.UI.ViewModel;
using Microsoft.Phone.Tasks;
using ReactiveUI;

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


            var uri = GetUri(p);
            this.PhotoUri = uri;
            TimelinePhotoSource = new BitmapImage(uri)
            {
                CreateOptions = BitmapCreateOptions.DelayCreation,
                DecodePixelType = DecodePixelType.Logical,
                DecodePixelHeight = 220,
                //DecodePixelWidth = 450
            };

            PhotoSource = new BitmapImage(uri)
            {
                CreateOptions = BitmapCreateOptions.DelayCreation,
                DecodePixelType = DecodePixelType.Physical,
                DecodePixelHeight = (int)p.Height,
                //DecodePixelWidth = (int)p.Width
            };


        }

        private Size _MaxPhotoSize = Size.Empty;
        private Size MaxPhotoSize
        {
            get { return _MaxPhotoSize == Size.Empty ? (_MaxPhotoSize = ResolutionHelper.MaxImageSize) : _MaxPhotoSize; }
        }
        private Uri GetUri(Photo p)
        {
            if (p.LocalFullPath != null)
                return new Uri(p.LocalFullPath, UriKind.RelativeOrAbsolute);
            var remoteUri = p.RemoteUri;

            if (remoteUri.Contains("ggpht"))
            {
                remoteUri += string.Format("=s{0}", Math.Max((int)MaxPhotoSize.Width, (int)MaxPhotoSize.Height));

            }

            return new Uri(remoteUri, UriKind.RelativeOrAbsolute);

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
                if (CanChooseNewPhoto)
                {
                    Chooser.Show();
                }
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
            var res = await ViewHelpers.HandlePhotoChooserCompleted(e, App.ShowPopup);
            if (res != null)
            {
                Photo = res;
            }
        }





    }


















}
