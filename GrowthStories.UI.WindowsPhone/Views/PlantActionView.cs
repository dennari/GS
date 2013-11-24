using Growthstories.Domain.Entities;
using Growthstories.UI.ViewModel;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Growthstories.UI.WindowsPhone
{


    public class PlantActionView : GSContentControl<IPlantActionViewModel>
    {


        public static readonly DependencyProperty NoteVisibilityProperty =
          DependencyProperty.Register("NoteVisibility", typeof(System.Windows.Visibility), typeof(PlantActionView), new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty HeaderVisibilityProperty =
            DependencyProperty.Register("HeaderVisibility", typeof(System.Windows.Visibility), typeof(PlantActionView), new PropertyMetadata(Visibility.Visible));


        public static readonly DependencyProperty CommandProperty =
           DependencyProperty.Register("Command", typeof(ICommand), typeof(PlantActionView), new PropertyMetadata(null, CommandValueChanged));

        public static readonly DependencyProperty ContentVisibilityProperty =
           DependencyProperty.Register("ContentVisibility", typeof(System.Windows.Visibility), typeof(PlantActionView), new PropertyMetadata(Visibility.Collapsed));


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
                if (value != null && value != Command)
                    SetValue(CommandProperty, value);
            }
        }


        public Visibility NoteVisibility
        {
            get { return (Visibility)GetValue(NoteVisibilityProperty); }
            set
            {
                if (value != NoteVisibility)
                    SetValue(NoteVisibilityProperty, value);
            }
        }

        public Visibility ContentVisibility
        {
            get { return (Visibility)GetValue(ContentVisibilityProperty); }
            set
            {

                //if (value != ContentVisibility)
                SetValue(ContentVisibilityProperty, value);
            }
        }

        public Visibility HeaderVisibility
        {
            get { return (Visibility)GetValue(HeaderVisibilityProperty); }
            set
            {
                if (value != HeaderVisibility)
                    SetValue(HeaderVisibilityProperty, value);
            }
        }

        protected override void OnViewModelChanged(IPlantActionViewModel vm)
        {

            if (vm == null)
                return;

            DataTemplate contentTemplate = null;
            Brush bg = GetBg("/Assets/Bg/action_bg.jpg");
            //base.OnViewModelChanged(vm);
            if (vm.ActionType == PlantActionType.PHOTOGRAPHED)
            {
                contentTemplate = Application.Current.Resources["DetailPhotoTemplate"] as DataTemplate;
            }
            if (vm.ActionType == PlantActionType.MEASURED)
            {
                contentTemplate = Application.Current.Resources["DetailMeasureTemplate"] as DataTemplate;
            }
            if (vm.ActionType == PlantActionType.WATERED)
            {
                bg = GetBg("/Assets/Bg/watering_bg.jpg");

            }

            if (contentTemplate != null)
            {
                if (contentTemplate != this.ContentTemplate)
                    this.ContentTemplate = contentTemplate;
                this.ContentVisibility = System.Windows.Visibility.Visible;
            }
            else
            {
                this.ContentVisibility = System.Windows.Visibility.Collapsed;
            }

            //this.DataContext = vm;
            this.Background = bg;

        }


        protected ImageBrush GetBg(string path)
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
            this.WhenNavigatedTo(ViewModel, () =>
            {
                if (this.ViewModel.ActionType == PlantActionType.PHOTOGRAPHED)
                {
                    var vm = this.ViewModel as IPlantPhotographViewModel;
                    if (vm != null && vm.PlantActionId == default(Guid))
                        vm.PhotoChooserCommand.Execute(null);
                }

                return Disposable.Empty;
            });
        }



    }


    public class TimelineActionView : PlantActionView
    {
        public TimelineActionView()
        {

        }

        protected override void OnViewModelChanged(IPlantActionViewModel vm)
        {
            if (vm == null)
                return;
            DataTemplate contentTemplate = null;



            if (vm.ActionType == PlantActionType.PHOTOGRAPHED)
                contentTemplate = Application.Current.Resources["TimelinePhotoTemplate"] as DataTemplate;
            else if (vm.ActionType == PlantActionType.MEASURED)
                contentTemplate = Application.Current.Resources["TimelineMeasureTemplate"] as DataTemplate;


            if (contentTemplate != null)
            {
                if (contentTemplate != this.ContentTemplate)
                    this.ContentTemplate = contentTemplate;
                this.ContentVisibility = System.Windows.Visibility.Visible;

            }
            else
            {
                this.ContentVisibility = System.Windows.Visibility.Collapsed;
            }


        }

        //public static readonly DependencyProperty ContentVisibilityProperty =
        // DependencyProperty.Register("ContentVisibility", typeof(System.Windows.Visibility), typeof(TimelineActionView), new PropertyMetadata(Visibility.Collapsed));

        //public Visibility ContentVisibility
        //{
        //    get { return (Visibility)GetValue(ContentVisibilityProperty); }
        //    set
        //    {

        //        if (value != ContentVisibility)
        //            SetValue(ContentVisibilityProperty, value);
        //    }
        //}


    }

}
