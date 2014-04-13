using ReactiveUI;
using System;
using Growthstories.UI.ViewModel;
using System.Collections.Generic;
using Growthstories.Core;
using System.Reactive.Disposables;
using System.Reactive;
using System.Reactive.Linq;


namespace Growthstories.UI.Services
{
    public class GSViewLocator : IViewLocator
    {


        //private Func<object, Type> _ViewModelToViewModelInterfaceFunc;

        //public Func<object, Type> ViewModelToViewModelInterfaceFunc 
        //{
        //    get
        //    {
        //        return _ViewModelToViewModelInterfaceFunc;
        //    }
        //    set
        //    {
        //        this.Log().Info("setting ViewModelTolViewModelInterfacefunc to {0} for viewlocator {1}", value, id);
        //        _ViewModelToViewModelInterfaceFunc = value;
        //        if (ViewModelToViewModelInterfaceFunc == null)
        //        {
        //            this.Log().Info("viewlocator setter: viewmodeltoviewmodelinterfacefunc is null, {0}", id);
        //        }
        //        else
        //        {
        //            this.Log().Info("viewlocator setter: viewmodeltoviewmodelinterfacefunc is not null, {0}", id);
        //        }
        //    }
        //}


        public Type ViewModelToViewModelInterfaceFunc(object T)
        {
            if (T is IGardenPivotViewModel)
                return typeof(IGardenPivotViewModel);
            if (T is ISettingsViewModel)
                return typeof(ISettingsViewModel);
            if (T is IAboutViewModel)
                return typeof(IAboutViewModel);
            if (T is IAddEditPlantViewModel)
                return typeof(IAddEditPlantViewModel);
            if (T is ISignInRegisterViewModel)
                return typeof(ISignInRegisterViewModel);
            if (T is IPhotoListViewModel)
                return typeof(IPhotoListViewModel);
            if (T is IPlantActionViewModel)
                return typeof(IPlantActionViewModel);
            if (T is IYAxisShitViewModel)
                return typeof(IYAxisShitViewModel);
            if (T is IScheduleViewModel)
                return typeof(IScheduleViewModel);
            if (T is ISearchUsersViewModel)
                return typeof(ISearchUsersViewModel);
            if (T is IGardenViewModel)
                return typeof(IGardenViewModel);
            if (T is IPlantViewModel)
                return typeof(IPlantViewModel);
            if (T is IPlantSingularViewModel)
                return typeof(IPlantSingularViewModel);
            if (T is IFriendsViewModel)
                return typeof(IFriendsViewModel);
            if (T is IPlantActionListViewModel)
                return typeof(IPlantActionListViewModel);
            return T.GetType();
        }


        private static GSViewLocator _Instance;
        public static GSViewLocator Instance
        {
            get { return _Instance ?? (_Instance = new GSViewLocator()); }
        }

        public int id;

        public GSViewLocator()
        {
            id = new Random().Next(0, 10000);
            this.Log().Info("instantiating new gsviewlocator {0}", id);
        }


        private Dictionary<IGardenPivotViewModel, IViewFor> pivotViews = new Dictionary<IGardenPivotViewModel, IViewFor>();

        private AsyncLock ResolveLock = new AsyncLock();

        private IDisposable subs = Disposable.Empty;

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
            this.Log().Info("in resolveview for {0}, {1}", viewModel, contract);

            using (var ret = ResolveLock.LockAsync().Result)
            {
                var viewType = typeof(IViewFor<>);
                this.Log().Info("viewtype is {0}", viewType);

                var gvm = viewModel as IGardenPivotViewModel;
                if (gvm != null)
                {
                    if (!pivotViews.ContainsKey(gvm))
                    {
                        this.Log().Info("creating new gardenpivotviewmodel for {0}", gvm.Username);
                        pivotViews.Clear(); // only cache the latest one, as otherwise we will use too much memory
                        pivotViews[gvm] = attemptToResolveView(viewType.MakeGenericType(ViewModelToViewModelInterfaceFunc(viewModel)), null);
                        subs.Dispose();
                        
                        // re-instantiation is needed when items are removed or added as pivot 
                        // creates all kinds of problems otherwise
                        gvm.WhenAnyValue(x => x.Plants).Where(x => x != null).Take(1).Subscribe(__ =>
                        {
                            subs = gvm.Plants.CountChanged.Subscribe(_ =>
                            {
                                this.Log().Info("gardenpivotviewmodel for {0} will be re-instantiated", gvm.Username);
                                pivotViews.Clear();

                                //pivotViews[gvm] = attemptToResolveView(viewType.MakeGenericType(ViewModelToViewModelInterfaceFunc(viewModel)), null);
                            }

                            );
                        });
                    }
                    else
                    {
                        this.Log().Info("using cached gardenpivotviewmodel for {0}", gvm.Username);
                    }
                    return pivotViews[gvm];
                }

                this.Log().Info("viewtype is {0}", viewType);

                //if (ViewModelToViewModelInterfaceFunc == null)
                //{
                //    this.Log().Info("viewlocator: viewmodeltoviewmodelinterfacefunc is null, {0}", id);
                //}
                //else
                //{
                //    this.Log().Info("viewlocator: viewmodeltoviewmodelinterfacefunc is not null, {0}", id);
                //}

                var vmif = ViewModelToViewModelInterfaceFunc(viewModel);
                this.Log().Info("vmif is {0}", vmif);
                var gt = viewType.MakeGenericType(vmif);
                this.Log().Info("gt is {0}", gt);
                var r = attemptToResolveView(gt, null);
                this.Log().Info("r is {0}", r);
                return r;
            }
        }


        public void Reset()
        {
            using (var ret = ResolveLock.LockAsync().Result)
            {
                pivotViews.Clear();
            }
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
