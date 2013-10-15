using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Growthstories.UI.WindowsPhone
{
    public class AddPlantActionView : ContentControl
    {

        public static readonly DependencyProperty NoteVisibilityProperty =
          DependencyProperty.Register("NoteVisibility", typeof(System.Windows.Visibility), typeof(AddPlantActionView), new PropertyMetadata(System.Windows.Visibility.Visible));


        public System.Windows.Visibility NoteVisibility
        {
            get { return (System.Windows.Visibility)GetValue(NoteVisibilityProperty); }
            set
            {
                if (value != null)
                {
                    SetValue(NoteVisibilityProperty, value);
                }
            }
        }

        public AddPlantActionView()
        {

            this.Background = new ImageBrush()
            {

                ImageSource = new BitmapImage()
                {
                    CreateOptions = BitmapCreateOptions.DelayCreation,
                    DecodePixelHeight = 800,
                    DecodePixelType = DecodePixelType.Logical,
                    UriSource = new Uri("/Assets/Bg/action_bg.jpg", UriKind.Relative),

                },
                Stretch = Stretch.UniformToFill
            };

        }
    }
}
