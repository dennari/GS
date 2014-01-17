using Growthstories.UI.ViewModel;
using System;

namespace Growthstories.UI.WindowsPhone
{

    public class PlantPhotoPivotViewBase : GSView<IPhotoListViewModel>
    {

    }


    public partial class PlantPhotoPivotView : PlantPhotoPivotViewBase
    {

        public PlantPhotoPivotView()
        {
            InitializeComponent();

            if (Height != Double.NaN)
            {
                Height = Double.NaN;
            }
        }


    }
}