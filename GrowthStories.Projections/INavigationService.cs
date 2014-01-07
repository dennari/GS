using System;
using System.Collections.Generic;
using System.Linq;

#if !NETFX_CORE
//using System.Windows.Navigation;
#endif

namespace Growthstories.UI
{




    public interface INavigationService
    {
        void GoBack();
        IDictionary<IconType, Uri> IconUri { get; }
        IDictionary<View, Uri> ViewUri { get; }
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


        public IDictionary<IconType, Uri> IconUri
        {
            get { throw new NotImplementedException(); }
        }

        public IDictionary<View, Uri> ViewUri
        {
            get { throw new NotImplementedException(); }
        }
    }
}