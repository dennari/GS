using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI;

namespace Growthstories.UI.ViewModel
{

    public class PhotoListViewModel : RoutableViewModel, IPhotoListViewModel
    {

        public IReactiveDerivedList<IPlantPhotographViewModel> Photos { get; private set; }


        private IPlantPhotographViewModel _Selected;
        public IPlantPhotographViewModel Selected
        {
            get
            {
                return _Selected;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _Selected, value);
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
                Selected = selected;
            }
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
