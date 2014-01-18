using System;
using Growthstories.UI.ViewModel;



namespace Growthstories.UI.WindowsPhone
{


    public class PlantSingularViewBase : GSView<IPlantSingularViewModel>
    {

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
        }


    }


}