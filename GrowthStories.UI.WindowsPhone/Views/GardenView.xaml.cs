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

    



    }
}