﻿using Growthstories.Domain.Messaging;
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

namespace Growthstories.UI.WindowsPhone.ViewModels
{


    public class ClientPlantPhotographViewModel : PlantPhotographViewModel, IPlantPhotographViewModel
    {



        protected BitmapImage _Photo;
        public BitmapImage Photo
        {
            get
            {
                return _Photo ?? (_Photo = new BitmapImage()
                {
                    CreateOptions = BitmapCreateOptions.DelayCreation,
                    DecodePixelType = DecodePixelType.Physical
                });
            }
        }

        public ClientPlantPhotographViewModel(PlantActionState state, IGSAppViewModel app)
            : base(state, app)
        {

            var photoStream = this.WhenAnyValue(x => x.PhotoData, x => x)
                .Where(x => x != default(Photo));


            if (state != null)
                photoStream.StartWith(state.Photo.Value);

            photoStream.Subscribe(x => Photo.SetSource(x));
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
                PhotoData = await image.SavePhotoToLocalStorageAsync();
            }
        }




    }


















}
