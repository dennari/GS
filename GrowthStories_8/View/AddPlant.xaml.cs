using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Growthstories.WP8.Models;
using Growthstories.PCL.Services;
using Growthstories.PCL.ViewModel;
using Microsoft.Phone.Tasks;

namespace Growthstories.WP8.View
{
    public partial class AddPlant : PhoneApplicationPage
    {
        CameraCaptureTask cameraCaptureTask;
        PhotoChooserTask photoChooserTask;
        AddPlantViewModel vm;

        public AddPlant()
        {
            InitializeComponent();
            cameraCaptureTask = new CameraCaptureTask();
            cameraCaptureTask.Completed += new EventHandler<PhotoResult>(cameraCaptureTask_Completed);
            photoChooserTask = new PhotoChooserTask();
            photoChooserTask.Completed += new EventHandler<PhotoResult>(photoChooserTask_Completed);
            vm = DataContext as AddPlantViewModel;
        }

        void cameraCaptureTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                vm.ProfilePhoto = e.ChosenPhoto;
            }
        }

        void photoChooserTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                vm.ProfilePhoto = e.ChosenPhoto;
            }
        }

        private void appBarOkButton_Click(object sender, EventArgs e)
        {

            vm.save(nameTextBox.Text, genusTextBox.Text);
        }

        private void appBarCancelButton_Click(object sender, EventArgs e)
        {
            vm.Nav.GoBack();
        }


        private void snapProfilePhotoButton_Click(object sender, RoutedEventArgs e)
        {
            cameraCaptureTask.Show();
        }

        private void chooseProfilePhotoButton_Click(object sender, RoutedEventArgs e)
        {
            photoChooserTask.Show();

        }
    }
}