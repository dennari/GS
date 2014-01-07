using System;

#if !NETFX_CORE
//using System.Windows.Navigation;
#endif

namespace Growthstories.PCL.Helpers
{
    public interface INavigationService
    {
        void GoBack();

#if NETFX_CORE
        void NavigateTo(Type pageType);
        void NavigateTo(Type sourcePageType, object parameter);
#else
        void NavigateTo(Uri pageUri);
#endif
    }
}