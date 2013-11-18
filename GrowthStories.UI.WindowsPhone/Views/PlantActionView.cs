using Growthstories.UI.ViewModel;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Growthstories.UI.WindowsPhone
{
    public enum DisplayMode
    {
        Timeline = 0,
        Detail = 1
    }

    public class PlantActionView : ContentControl, IViewFor<IPlantActionViewModel>
    {

        object IViewFor.ViewModel { get { return this.ViewModel; } set { this.ViewModel = (IPlantActionViewModel)value; } }


        public static readonly DependencyProperty NoteVisibilityProperty =
          DependencyProperty.Register("NoteVisibility", typeof(System.Windows.Visibility), typeof(PlantActionView), new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty HeaderVisibilityProperty =
            DependencyProperty.Register("HeaderVisibility", typeof(System.Windows.Visibility), typeof(PlantActionView), new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty ContentVisibilityProperty =
            DependencyProperty.Register("ContentVisibility", typeof(System.Windows.Visibility), typeof(PlantActionView), new PropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty ViewModelProperty =
          DependencyProperty.Register("ViewModel", typeof(IPlantActionViewModel), typeof(PlantActionView), new PropertyMetadata(null, ViewModelValueChanged));

        public static readonly DependencyProperty DisplayModeProperty =
         DependencyProperty.Register("DisplayMode", typeof(DisplayMode), typeof(PlantActionView), new PropertyMetadata(DisplayMode.Timeline, DisplayModeValueChanged));

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(PlantActionView), new PropertyMetadata(null, CommandValueChanged));

        static void ViewModelValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                var view = (PlantActionView)sender;
                view.SetDataContext((IPlantActionViewModel)e.NewValue, view.DisplayMode);

            }
            catch { }
        }

        static void DisplayModeValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                var view = (PlantActionView)sender;
                view.SetDataContext(view.ViewModel, (DisplayMode)e.NewValue);

            }
            catch { }

        }


        static void CommandValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                var view = (PlantActionView)sender;
                //view.SetDataContext(view.ViewModel, (DisplayMode)e.NewValue);
                if (e.NewValue != null && view.Command != e.NewValue)
                    view.Command = (ICommand)e.NewValue;

            }
            catch { }

        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set
            {
                if (value != null)
                    SetValue(CommandProperty, value);
            }
        }

        public DisplayMode DisplayMode
        {
            get { return (DisplayMode)GetValue(DisplayModeProperty); }
            set
            {
                SetValue(DisplayModeProperty, value);
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

        public Visibility ContentVisibility
        {
            get { return (Visibility)GetValue(ContentVisibilityProperty); }
            set
            {
                SetValue(ContentVisibilityProperty, value);
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

        private void SetDataContext(IPlantActionViewModel value, DisplayMode mode)
        {

            if (value == null)
                return;


            DataTemplate contentTemplate = null;
            Brush bg = null;

            if (mode == DisplayMode.Timeline)
            {
                if (value is IPlantWaterViewModel)
                    bg = GetBg("/Assets/Bg/watering_bg.jpg");
                else
                    bg = GetBg("/Assets/Bg/action_bg.jpg");
                if (value is IPlantPhotographViewModel)
                    contentTemplate = Application.Current.Resources["TimelinePhotoTemplate"] as DataTemplate;
                if (value is IPlantMeasureViewModel)
                    contentTemplate = Application.Current.Resources["TimelineMeasureTemplate"] as DataTemplate;

            }
            else
            {
                if (value is IPlantPhotographViewModel)
                {
                    contentTemplate = Application.Current.Resources["DetailPhotoTemplate"] as DataTemplate;
                }

            }

            this.DataContext = value;

            if (contentTemplate != null)
            {
                this.ContentVisibility = System.Windows.Visibility.Visible;
                this.ContentTemplate = contentTemplate;
            }
            if (bg != null)
            {
                this.Background = bg;
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


        protected override void OnTap(GestureEventArgs e)
        {
            base.OnTap(e);

        }

        protected override void OnDoubleTap(GestureEventArgs e)
        {
            base.OnDoubleTap(e);
            var cmd = Command;
            if (cmd != null)
            {
                cmd.Execute(null);
            }
        }

        public PlantActionView()
        {

            //this.Background = GetBg("/Assets/Bg/action_bg.jpg");

        }
    }
}
