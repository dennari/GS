using Growthstories.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.UI
{
    public enum View
    {
        EXCEPTION,
        GARDEN,
        PLANT,
        ADD_PLANT,
        ADD_COMMENT,
        ADD_WATER,
        ADD_PHOTO,
        ADD_FERT,
        SELECT_PROFILE_PICTURE
    }

    public enum IconType
    {
        ADD,
        CHECK,
        CANCEL,
        DELETE,
        CHECK_LIST
    }

    public static class GSApp
    {
        public static readonly IDictionary<View, Uri> ViewUri = new Dictionary<View, Uri>()
        {
            {View.GARDEN,new Uri("/Views/GardenView.xaml", UriKind.Relative)},
            {View.PLANT,new Uri("/Views/PlantView.xaml", UriKind.Relative)},
            {View.ADD_PLANT,new Uri("/Views/AddPlantView.xaml", UriKind.Relative)},
            {View.SELECT_PROFILE_PICTURE,new Uri("/Views/SelectProfilePictureView.xaml", UriKind.Relative)}
        };

        public static readonly IDictionary<IconType, Uri> IconUri = new Dictionary<IconType, Uri>()
        {
            {IconType.ADD,new Uri("/Assets/Icons/appbar.add.png", UriKind.RelativeOrAbsolute)},
            {IconType.CHECK,new Uri("/Assets/Icons/appbar.check.png", UriKind.RelativeOrAbsolute)},
            {IconType.DELETE,new Uri("/Assets/Icons/appbar.delete.png", UriKind.RelativeOrAbsolute)},
            {IconType.CHECK_LIST,new Uri("/Assets/Icons/appbar.list.check.png", UriKind.RelativeOrAbsolute)}
        };

        public const string APPBAR_MODE_MINIMIZED = "Minimized";
        public const string APPBAR_MODE_DEFAULT = "Default";


    }
}
