using System;
using Growthstories.UI.ViewModel;
using ReactiveUI;
using System.Windows.Media.Animation;
using System.Windows;
using System.Windows.Data;
using System.Reactive.Linq;

namespace Growthstories.UI.WindowsPhone
{


    public class PlantSingularViewBase : GSView<IPlantSingularViewModel>
    {
        public PlantSingularViewBase()
        {
            this.SetBinding(ViewModelProperty, new Binding());
        }
    }


    public partial class PlantSingularView : PlantSingularViewBase
    {

        // maybe just use the viewmodel's Log() extension method?
        //private static ILog Logger = LogFactory.BuildLogger(typeof(SearchUsersViewModel));


        public PlantSingularView()
        {
            InitializeComponent();

            if (Height != Double.NaN)
            {
                Height = Double.NaN;
            }
        }


        protected override void OnViewModelChanged(IPlantSingularViewModel vm)
        {
            base.OnViewModelChanged(vm);

            vm.WhenAnyValue(x => x.Plant.Name).Where(x => x != null).Take(1).Subscribe(x =>
            {
                ViewGrid.Title = x;
                ThePlantView.Visibility = Visibility.Visible;
            });


            //if (vm.Plant.Name == null)
            //{
            //    vm.Plant.Name = "loading";
            //}

            //vm.WhenAnyValue(x => x.Plant.Name).Subscribe(name =>
            //{
            //    if (name != null)
            //    {
            //        FadeIn();
            //    }
            //});
        }


        //private void FadeIn()
        //{
        //    DoubleAnimation wa = new DoubleAnimation();
        //    wa.Duration = new Duration(TimeSpan.FromSeconds(1.2));
        //    wa.BeginTime = TimeSpan.FromSeconds(0.2);
        //    wa.From = 0;
        //    wa.To = 1.0;
        //    wa.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut };

        //    Storyboard sb = new Storyboard();
        //    sb.Children.Add(wa);

        //    Storyboard.SetTarget(wa, LayoutRoot);
        //    Storyboard.SetTargetProperty(wa, new PropertyPath("Opacity"));

        //    sb.Begin();
        //}


    }


}