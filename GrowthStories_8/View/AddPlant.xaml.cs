using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Growthstories.PCL.Models;
using Growthstories.PCL.Services;
using Growthstories.PCL.ViewModel;
using Microsoft.Phone.Tasks;

namespace Growthstories.WP8.View
{
    public partial class AddPlant : PhoneApplicationPage
    {
        CameraCaptureTask cameraCaptureTask;
        PhotoChooserTask photoChooserTask;
        PhotoResult e;

        public AddPlant()
        {
            InitializeComponent();
            cameraCaptureTask = new CameraCaptureTask();
            cameraCaptureTask.Completed += new EventHandler<PhotoResult>(cameraCaptureTask_Completed);
            photoChooserTask = new PhotoChooserTask();
            photoChooserTask.Completed += new EventHandler<PhotoResult>(photoChooserTask_Completed);
        }

        void cameraCaptureTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                //MessageBox.Show(e.ChosenPhoto.Length.ToString());

                //Code to display the photo on the page in an image control named myImage.
                System.Windows.Media.Imaging.BitmapImage bmp = new System.Windows.Media.Imaging.BitmapImage();
                bmp.SetSource(e.ChosenPhoto);
                ProfilePhoto.Source = bmp;
                this.e = e;
            }
        }

        void photoChooserTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                MessageBox.Show(e.OriginalFileName);

                //Code to display the photo on the page in an image control named myImage.
                System.Windows.Media.Imaging.BitmapImage bmp = new System.Windows.Media.Imaging.BitmapImage();
                bmp.SetSource(e.ChosenPhoto);
                ProfilePhoto.Source = bmp;
                this.e = e;
            }
        }


        private void appBarOkButton_Click(object sender, EventArgs e)
        {
            AddPlantViewModel vm = DataContext as AddPlantViewModel;
            // Confirm there is some text in the text box.
            if (nameTextBox.Text.Length > 0 && genusTextBox.Text.Length > 0 && this.e != null)
            {
                // Create a new to-do item.
                vm.NewPlant.Name = nameTextBox.Text;
                vm.NewPlant.Genus = genusTextBox.Text;
                vm.NewPlant.ProfilePicture = this.e.ChosenPhoto;
                // Add the item to the ViewModel.
                vm.save();

                // Return to the main page.
                if (NavigationService.CanGoBack)
                {
                    NavigationService.GoBack();
                }
            }
        }

        private void appBarCancelButton_Click(object sender, EventArgs e)
        {
            // Return to the main page.
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
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