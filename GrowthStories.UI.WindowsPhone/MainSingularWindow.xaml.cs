using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using Growthstories.Core;
using Growthstories.Domain.Messaging;
using Growthstories.UI.ViewModel;
using Growthstories.UI.WindowsPhone.ViewModels;
using ReactiveUI;

namespace Growthstories.UI.WindowsPhone
{



    public partial class MainSingularWindow : MainWindowBase, IEnableLogger
    {



        public MainSingularWindow()
        {
            this.Log().Info("MainSingularWindow constructor");
            InitializeComponent();
        }



        protected override void HandleResuming(Unit _)
        {
            base.HandleResuming(_);

            // when navigated from secondary tile we also need to update infos on tiles
            if (Pvm != null)
            {
                var cpvm = Pvm as ClientPlantViewModel;
                if (cpvm != null && cpvm.TileHelper != null)
                {
                    this.Log().Info("updating whether only plant has tile");
                    cpvm.TileHelper.UpdateHasTile();
                }
            }
        }




        IPlantViewModel Pvm;

        protected override void OnViewModelChanged(IGSAppViewModel vm)
        {

            //this.Log().Info("MainWindowBase loaded {0}, MainViewBase Loaded {1}", MainWindowBaseMS, MainViewBaseMS);

            if (vm == null || MainViewModel != null)
                return;
            IDictionary<string, string> qs = this.NavigationContext.QueryString;

            Guid plantId = default(Guid);

            if (!qs.ContainsKey("id") || !Guid.TryParse(qs["id"], out plantId))
            {
                return;
            }

            // this should actually return immediately
            this.Log().Info("Navigated from tile");

            try
            {
                //this.Log().Info("Loading plant"); 
                this.Log().Info("Loading plant started");

                Pvm = ViewModel.GetSinglePlant(plantId);
                var a = Pvm.Actions; // just to start loading
                ViewModel.Bus.Listen<IEvent>()
                    .OfType<AggregateDeleted>()
                    .Where(x => x.AggregateId == plantId)
                    .Take(1)
                    .Delay(TimeSpan.FromMilliseconds(500), RxApp.MainThreadScheduler)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x =>
                    {
                        this.Log().Info("terminating app");
                        Application.Current.Terminate();
                    });

            }
            catch (Exception e)
            {

            }

            MainViewModel = new PlantSingularViewModel(Pvm, vm);
            base.OnViewModelChanged(vm);
            vm.Log().Info("setting selected plant to {0}");
            Pvm.ShouldBeFullyLoaded = true;
            
            if (UILoaded)
            {
                UIAndVMLoaded();
            }
        }


        private bool UILoaded = false;


        IPlantSingularViewModel MainViewModel;

        private void UIAndVMLoaded()
        {

            ViewModel.Log().Info("MainWindow Loaded in {0}", GSAutoSuspendApplication.LifeTimer.ElapsedMilliseconds);
            ViewModel.MainWindowLoadedCommand.Execute(MainViewModel);

            this.ApplicationBar.IsVisible = true;

            //this.MainView.ViewModel = MainViewModel;

            //this.DataContext = ViewModel;
            //this.DataContext = ViewModel;
        }


        protected void MainWindowBase_Loaded(object sender, RoutedEventArgs e)
        {
            if (UILoaded)
                return;
            UILoaded = true;
            if (MainViewModel != null)
            {
                UIAndVMLoaded();
            }


        }
    }
}