
using Growthstories.UI.ViewModel;
using GrowthStories.UI.WindowsPhone.Views;
using ReactiveUI;
using ReactiveUI.Mobile;

namespace GrowthStories.UI.WindowsPhone.ViewModels
{

    public class AppViewModel : ReactiveObject, IApplicationRootState, IScreen
    {

        RoutingState _Router;

        public IRoutingState Router
        {
            get { return _Router; }
            set { _Router = (RoutingState)value; } // XXX: This is dumb.
        }


        public AppViewModel()
        {
            this.Router = new RoutingState();
            var resolver = RxApp.MutableResolver;

            resolver.RegisterConstant(this, typeof(IApplicationRootState));
            resolver.RegisterConstant(this, typeof(IScreen));
            resolver.RegisterConstant(this.Router, typeof(IRoutingState));

            resolver.Register(() => new GardenView(), typeof(IViewFor<GardenViewModel>));

            //Router.Navigate.Execute(new TestPage1ViewModel(this));
        }
    }
}
