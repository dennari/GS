﻿using System;
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
using System.Reactive.Linq;
using System.Reactive;

using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using System.Linq.Expressions;

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




    public class ClientAddPlantViewModel : AddPlantViewModel
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public ClientAddPlantViewModel()
        {

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

        private MockReactiveCommand _ViewFSCommand;
        public IReactiveCommand ViewFSCommand
        {
            get
            {

                if (_ViewFSCommand == null)
                {
                    _ViewFSCommand = new MockReactiveCommand();
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

        private MockReactiveCommand _CMOpen;
        public IReactiveCommand CMOpen
        {
            get
            {

                if (_CMOpen == null)
                {
                    _CMOpen = new MockReactiveCommand();
                    _CMOpen.Subscribe(_ =>
                        {
                            //ChoosePhoto();
                        });
                }
                return _CMOpen;

            }
        }

        private MockReactiveCommand _CMClose;
        public IReactiveCommand CMClose
        {
            get
            {

                if (_CMClose == null)
                {
                    _CMClose = new MockReactiveCommand();
                    _CMOpen.Subscribe(_ =>
                    {
                        //ChoosePhoto();
                    });
                }
                return _CMClose;

            }
        }



        public Task<int> t_Completed(object sender, PhotoResult e)
        {
            //throw new NotImplementedException();
            var image = e.ChosenPhoto;
            if (e.TaskResult == TaskResult.OK && image.CanRead && image.Length > 0)
            {
                this.ProfilePictureButtonText = "";
                return Task.FromResult(0);
            }
            return Task.FromResult(0);

        }

    }

    public class ClientEditPlantViewModel : ClientAddPlantViewModel
    {
        public ClientEditPlantViewModel()
        {

            string photo = null;

            Tags = new MockReactiveList<string>() { "orchid", "blue", "perennial" };
            Name = "Jore";
            Title = string.Format("edit {0}", Name);
            Species = "Aloe Vera";

            Photo = new Photo()
            {
                LocalFullPath = photo ?? @"/TestData/517e100d782a828894.jpg",
                LocalUri = photo ?? @"/TestData/517e100d782a828894.jpg"
            };

        }
    }
}
