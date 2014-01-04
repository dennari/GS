using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.ComponentModel;
using Growthstories.UI.ViewModel;
using ReactiveUI;
using System.Windows.Data;
using EventStore.Logging;
using System.Windows.Media.Animation;
using System.Windows.Media;


namespace Growthstories.UI.WindowsPhone
{
    public class GardenViewBase : GSView<IGardenViewModel>
    {

    }


    public partial class GardenView : GardenViewBase
    {


        private static ILog Logger = LogFactory.BuildLogger(typeof(GardenView));


   
        public GardenView()
        {
            InitializeComponent();

            if (Height != Double.NaN)
            {
                Height = Double.NaN;
            }

            Logger.Info("initialized garden view");
        }


        
        protected override void OnViewModelChanged(IGardenViewModel vm)
        {

            Logger.Info("gardenview onViewModelChanged");


            foreach (PlantViewModel pvm in ViewModel.Plants)
            {

                Logger.Info("subscribing for plant " + pvm.Name);


                pvm.DeleteCommand
                    .Subscribe(_ => PlantView.DeleteTile(pvm));
                //pvm.DeleteCommand.Execute(null);
            }    
        }

        
        public void handleDelete(PlantViewModel pvm)
        {

            PlantView.DeleteTile(pvm);

            //MessageBoxResult res = MessageBox.Show("Are you sure you wish to delete the plant " + pvm.Name + "?");
        }


        private void PlantsSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.ViewModel.SelectedItemsChanged.Execute(Tuple.Create(e.AddedItems, e.RemovedItems));
        }



        private void Img_ImageOpened(object sender, RoutedEventArgs e)
        {
            var img = sender as System.Windows.Controls.Image;

            DoubleAnimation wa = new DoubleAnimation();
            wa.Duration = new Duration(TimeSpan.FromSeconds(0.6));
            wa.From = 0;
            wa.To = 1.0;
            
            Storyboard sb = new Storyboard();
            sb.Children.Add(wa);
            
            var sp = FindParent<StackPanel>(img);
            
            if (sp != null)
            {
                Storyboard.SetTarget(wa, sp);
                Storyboard.SetTargetProperty(wa, new PropertyPath("Opacity"));
            }

            if (Math.Abs(sp.Opacity - 1.0) > 0.001)
            {
                sb.Begin();
            }
            
        }


    
        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {      
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);    
            
            if (parentObject == null) return null;
            
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindParent<T>(parentObject);
        }



        public static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                T ch = child as T;
                if (ch != null) {
                    return ch;
                } else {
                    T result = FindVisualChild<T>(child);
                    if (result != null)
                        return result;
                }
            }
            return null;
        }

    }
}