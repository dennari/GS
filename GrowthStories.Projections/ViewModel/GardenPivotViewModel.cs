
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Reactive.Disposables;

namespace Growthstories.UI.ViewModel
{


    public class GardenPivotViewModel : MultipageViewModel, IGardenPivotViewModel
    {

        protected IPlantViewModel _SelectedPlant;
        public IPlantViewModel SelectedPlant { get { return _SelectedPlant; } set { this.RaiseAndSetIfChanged(ref _SelectedPlant, value); } }



        private IYAxisShitViewModel CurrentChartViewModel;

        private readonly IGardenViewModel Vm;
        /// </summary>
        public GardenPivotViewModel(IGardenViewModel vm)
            : base(vm.App)
        {
            this.Vm = vm;


            this.WhenAny(x => x.SelectedPage, x => x.GetValue())
                .Where(x => x != null)
                .OfType<IPlantViewModel>()
                .Subscribe(x => this.SelectedPlant = x);

            this.App.WhenAny(x => x.Orientation, x => x.GetValue())
                .Where(x => (x & PageOrientation.Landscape) == PageOrientation.Landscape && App.Router.GetCurrentViewModel() == this)
                .Subscribe(_ =>
                {
                    this.CurrentChartViewModel = App.YAxisShitViewModelFactory(this.SelectedPlant);
                    App.Router.Navigate.Execute(this.CurrentChartViewModel);
                });

            this.App.WhenAny(x => x.Orientation, x => x.GetValue())
                .Where(x => (x & PageOrientation.Portrait) == PageOrientation.Portrait && App.Router.GetCurrentViewModel() == this.CurrentChartViewModel)
                .Subscribe(_ =>
                {
                    App.Router.Navigate.Execute(this);
                });



        }

        public Guid Id
        {
            get { return Vm.Id; }
        }

        public IReactiveCommand SelectedItemsChanged
        {
            get { return Vm.SelectedItemsChanged; }
        }

        public IAuthUser User
        {
            get { return Vm.User; }
        }

        public IReadOnlyReactiveList<IPlantViewModel> Plants
        {
            get { return Vm.Plants; }
        }

        public string Username
        {
            get { return Vm.Username; }
        }

        public SupportedPageOrientation SupportedOrientations
        {
            get { return SupportedPageOrientation.PortraitOrLandscape; }
        }

        public override string UrlPathSegment
        {
            get { throw new NotImplementedException(); }
        }
    }
}