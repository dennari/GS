using Growthstories.UI.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Growthstories.UI.WindowsPhone.Design
{
    public class PlantActionView : ContentControl
    {

        public static readonly DependencyProperty NoteVisibilityProperty =
          DependencyProperty.Register("NoteVisibility", typeof(System.Windows.Visibility), typeof(PlantActionView), new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty HeaderVisibilityProperty =
            DependencyProperty.Register("HeaderVisibility", typeof(System.Windows.Visibility), typeof(PlantActionView), new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty ViewModelProperty =
          DependencyProperty.Register("ViewModel", typeof(IPlantActionViewModel), typeof(PlantActionView), new PropertyMetadata(null, ViewModelValueChanged));


        static void ViewModelValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var view = (PlantActionView)sender;
            if (e.NewValue != null)
            {
                var v = e.NewValue as IPlantActionViewModel;
                if (v != null)
                    view.SetDataContext(v);
            }


        }

        public Visibility NoteVisibility
        {
            get { return (Visibility)GetValue(NoteVisibilityProperty); }
            set
            {
                SetValue(NoteVisibilityProperty, value);
            }
        }

        public Visibility HeaderVisibility
        {
            get { return (Visibility)GetValue(HeaderVisibilityProperty); }
            set
            {
                SetValue(HeaderVisibilityProperty, value);
            }
        }

        public IPlantActionViewModel ViewModel
        {
            get { return (IPlantActionViewModel)GetValue(ViewModelProperty); }
            set
            {
                if (value != null)
                {
                    SetValue(ViewModelProperty, value);
                }
            }
        }

        private void SetDataContext(IPlantActionViewModel value)
        {
            this.DataContext = value;
            UserControl content = null;
            if (value is IPlantWaterViewModel)
                this.Background = GetBg("/Assets/Bg/watering_bg.jpg");
            if (value is IPlantMeasureViewModel)
                content = new PlantMeasurementActionView();
            if (value is IPlantPhotographViewModel)
            {
                content = new PlantPhotographActionView();

            }

            if (content != null)
            {
                content.DataContext = value;
                this.Content = content;
            }

        }

        private ImageBrush GetBg(string path)
        {
            return new ImageBrush()
            {

                ImageSource = new BitmapImage()
                {
                    CreateOptions = BitmapCreateOptions.DelayCreation,
                    DecodePixelHeight = 800,
                    DecodePixelType = DecodePixelType.Logical,
                    UriSource = new Uri(path, UriKind.Relative),

                },
                Stretch = Stretch.UniformToFill
            };
        }




        public PlantActionView()
        {

            //this.Background = GetBg("/Assets/Bg/action_bg.jpg");

        }
    }
}
