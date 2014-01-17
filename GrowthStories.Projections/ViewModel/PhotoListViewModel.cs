using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI;

namespace Growthstories.UI.ViewModel
{
    public class PhotoListViewModel : RoutableViewModel, IPhotoListViewModel
    {

        public IList<IPlantPhotographViewModel> Photos { get; private set; }

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


        public PhotoListViewModel(IList<IPlantPhotographViewModel> photos, IGSAppViewModel app, IPlantPhotographViewModel selected = null)
            : base(app)
        {
            this.Photos = photos;
            Selected = selected ?? photos.First();

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
