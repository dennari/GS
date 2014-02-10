using ReactiveUI;
using System;
using Growthstories.UI.ViewModel;
using System.Collections.Generic;

namespace Growthstories.UI.Services
{
    public class GSViewLocator : IViewLocator
    {

        public Func<object, Type> ViewModelToViewModelInterfaceFunc { get; set; }

        private static GSViewLocator _Instance;
        public static GSViewLocator Instance
        {
            get { return _Instance ?? (_Instance = new GSViewLocator()); }
        }
        public GSViewLocator()
        {

        }


        private Dictionary<IGardenPivotViewModel, IViewFor> pivotViews = new Dictionary<IGardenPivotViewModel, IViewFor>();


        /// <summary>
        /// Returns the View associated with a ViewModel, deriving the name of
        /// the Type via ViewModelToViewFunc, then discovering it via
        /// ServiceLocator.
        /// </summary>
        /// <param name="viewModel">The ViewModel for which to find the
        /// associated View.</param>
        /// <returns>The View for the ViewModel.</returns>
        public IViewFor ResolveView<T>(T viewModel, string contract = null)
            where T : class
        {
            var viewType = typeof(IViewFor<>);

            var gvm = viewModel as IGardenPivotViewModel;
            if (gvm != null)
            {
                if (!pivotViews.ContainsKey(gvm))
                {
                    this.Log().Info("creating new gardenpivotviewmodel for {0}", gvm.Username);
                    // only cache the latest one, as otherwise we wil use too much memory
                    pivotViews.Clear(); 
                    pivotViews[gvm] = attemptToResolveView(viewType.MakeGenericType(ViewModelToViewModelInterfaceFunc(viewModel)), null);
                }
                else
                {
                    this.Log().Info("using cached gardenpivotviewmodel for {0}", gvm.Username);
                }
                return pivotViews[gvm];
            }

            return attemptToResolveView(viewType.MakeGenericType(ViewModelToViewModelInterfaceFunc(viewModel)), null);
        }


        public void Reset()
        {
            pivotViews.Clear();
        }


        IViewFor attemptToResolveView(Type type, string contract)
        {
            if (type == null) return null;

            var ret = default(IViewFor);

            try
            {
                ret = (IViewFor)RxApp.DependencyResolver.GetService(type, contract);
            }
            catch (Exception)
            {
                //this.Log().ErrorException("Failed to instantiate view: " + type.FullName, ex);
                throw;
            }

            return ret;
        }


    }

}
