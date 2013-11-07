using Growthstories.UI.ViewModel;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.UI.Services
{
    public class GSViewLocator : IViewLocator
    {


        public GSViewLocator()
        {

        }

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

            if (viewModel is IPlantActionViewModel)
                return attemptToResolveView(viewType.MakeGenericType(typeof(IPlantActionViewModel)), contract);
            if (viewModel is IYAxisShitViewModel)
                return attemptToResolveView(viewType.MakeGenericType(typeof(IYAxisShitViewModel)), contract);

            return attemptToResolveView(viewType.MakeGenericType(viewModel.GetType()), contract);
        }


        IViewFor attemptToResolveView(Type type, string contract)
        {
            if (type == null) return null;

            var ret = default(IViewFor);

            try
            {
                ret = (IViewFor)RxApp.DependencyResolver.GetService(type, contract);
            }
            catch (Exception ex)
            {
                //this.Log().ErrorException("Failed to instantiate view: " + type.FullName, ex);
                throw;
            }

            return ret;
        }


    }

}
