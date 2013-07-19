using System;
using System.Collections.Generic;
using System.Linq;

#if !NETFX_CORE
//using System.Windows.Navigation;
#endif

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
        ADD_FERT
    }


    public interface INavigationService
    {
        void GoBack();

        //Uri PlantViewUri { get; }
        //Uri AddPlantViewUri { get; }

        void NavigateTo(View view);
    }

    public class FakeNavigationService : INavigationService
    {

        protected IList<View> Stack = new List<View>();

        public View CurrentView { get { return Stack.Last(); } }

        public void GoBack()
        {
            if (Stack.Count > 0)
                Stack.RemoveAt(Stack.Count - 1);
        }

        public void NavigateTo(View view)
        {
            Stack.Add(view);
        }
    }
}