using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI;
using System.Reactive.Linq;

namespace Growthstories.UI.ViewModel
{

    public class PhotoListViewModel : RoutableViewModel, IPhotoListViewModel
    {

        public IReactiveDerivedList<IPlantPhotographViewModel> Photos { get; private set; }


        private ObservableAsPropertyHelper<IPlantPhotographViewModel> _Selected;
        public IPlantPhotographViewModel Selected
        {
            get
            {
                return _Selected.Value;
            }
        }


        private object _SelectedItem;
        public object SelectedItem
        {
            get
            {
                return _SelectedItem;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _SelectedItem, value);
            }
        }



        public PhotoListViewModel(IReactiveDerivedList<IPlantPhotographViewModel> photos, IGSAppViewModel app, IPlantPhotographViewModel selected = null)
            : base(app)
        {
            this.Log().Info("initializing photolistviewmodel");

            this.Photos = photos;
            if (selected == null)
            {
                this.Log().Info("selected is null");
                selected = photos.First();

            }
            else
            {
                this.Log().Info("selected is #{0}, {1}", selected.ActionIndex, selected.PlantActionId);
            }

            this.WhenAnyValue(x => x.SelectedItem)
                .OfType<IPlantPhotographViewModel>()
                .ToProperty(this, x => x.Selected, out _Selected);

            SelectedItem = selected;

        }


        public override string UrlPathSegment
        {
            get { throw new NotImplementedException(); }
        }


        public ApplicationBarMode AppBarMode
        {
            get { return ApplicationBarMode.DEFAULT; }
        }


        public bool AppBarIsVisible
        {
            get { return false; }
        }

        public SupportedPageOrientation SupportedOrientations
        {
            get { return SupportedPageOrientation.PortraitOrLandscape; }
        }
    }
}
