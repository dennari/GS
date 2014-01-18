﻿
using System;
using System.Diagnostics;
using System.Windows.Media;
using BugSense;
using BugSense.Core.Model;
using Growthstories.Domain;
using Growthstories.Sync;
using Growthstories.UI.Services;
using Growthstories.UI.ViewModel;
using Growthstories.UI.WindowsPhone.ViewModels;
using Ninject.Modules;
using ReactiveUI;
using ReactiveUI.Mobile;

namespace Growthstories.UI.WindowsPhone
{


    public class Bootstrap : BaseSetup
    {
        protected readonly App PhoneApp;
        protected readonly IMutableDependencyResolver RxUIResolver;
        protected const string BUGSENSE_TOKEN = "e73c0669";

        public Bootstrap(App phoneApp)
        {
            PhoneApp = phoneApp;
            RxUIResolver = RxApp.MutableResolver;

        }

        public override void Load()
        {
            base.Load();

            RxUIConfiguration();
            ApplyGSAccentColor();
            BAConfiguration();
            ViewModelConfiguration();
            ViewConfiguration();


            BugSenseHandler.Instance.InitAndStartSession(new ExceptionManager(PhoneApp), BUGSENSE_TOKEN);
            BugSenseHandler.Instance.HandleWhileDebugging = true;




            PhoneApp.UnhandledException += (o, e) =>
            {
                // try to log the Exception
                try
                {
                    PhoneApp.Log().DebugExceptionExtended("Unhandled", e.ExceptionObject);
                }
                catch { }
            };
        }



        protected virtual void BAConfiguration()
        {
            BAUtils.RegisterScheduledTask();
        }


        protected virtual void RxUIConfiguration()
        {
            RxUIResolver.GetService<ISuspensionHost>().SetupDefaultSuspendResume(RxUIResolver.GetService<ISuspensionDriver>());

            Bind<IMutableDependencyResolver>().ToConstant(this.RxUIResolver);
            Bind<IRoutingState>().To<RoutingState>().InSingletonScope();
            RxUIResolver.RegisterLazySingleton(() => KernelInstance.GetService(typeof(IScreen)), typeof(IScreen));
            RxUIResolver.RegisterLazySingleton(() => KernelInstance.GetService(typeof(IRoutingState)), typeof(IRoutingState));
            RxUIResolver.RegisterLazySingleton(() => GSViewLocator.Instance, typeof(IViewLocator));

        }

        protected override void LogConfiguration()
        {
            base.LogConfiguration();
            RxUIResolver.Register(() => GSNullLog.Instance, typeof(ILogger));

        }

        private void ApplyGSAccentColor()
        {
            try
            {
                PhoneApp.Resources.Remove("PhoneAccentColor");
                PhoneApp.Resources.Add("PhoneAccentColor", PhoneApp.Resources["GSAccentColor"]);

                var ab = (SolidColorBrush)PhoneApp.Resources["PhoneAccentBrush"];
                var ac = (Color)PhoneApp.Resources["PhoneAccentColor"];
                ab.Color = ac;

            }
            catch
            {
                Debugger.Break();
            }
        }

        internal static INinjectModule GetModule(App app)
        {
            if (DesignModeDetector.IsInDesignMode())
                return new BootstrapDesign();
#if DEBUG
            Type machine = Type.GetType("Growthstories.UI.WindowsPhone.BootstrapMachine", false);
            if (machine != null)
                return (INinjectModule)Activator.CreateInstance(machine, app);

            return new Bootstrap(app);
#endif
            return new BootstrapProduction(app);

        }



        protected virtual void ViewModelConfiguration()
        {
            Bind<IGSAppViewModel, IApplicationRootState, IScreen>().To<ClientAppViewModel>().InSingletonScope();
            Bind<TestingViewModel>().To<ClientTestingViewModel>().InSingletonScope();
            Bind<IAboutViewModel>().To<AboutViewModel>().InSingletonScope();
            Bind<ISearchUsersViewModel>().To<SearchUsersViewModel>().InSingletonScope();


            RxUIResolver.RegisterLazySingleton(() => KernelInstance.GetService(typeof(IApplicationRootState)), typeof(IApplicationRootState));
            RxUIResolver.RegisterLazySingleton(() => KernelInstance.GetService(typeof(ISearchUsersViewModel)), typeof(ISearchUsersViewModel));
            RxUIResolver.RegisterLazySingleton(() => KernelInstance.GetService(typeof(IAboutViewModel)), typeof(IAboutViewModel));
            RxUIResolver.Register(() =>
            {
                return KernelInstance.GetService(typeof(IAddEditPlantViewModel));
            }, typeof(IAddEditPlantViewModel));
            RxUIResolver.RegisterLazySingleton(() => KernelInstance.GetService(typeof(TestingViewModel)), typeof(TestingViewModel));

        }


        protected virtual void ViewConfiguration()
        {

            RxUIResolver.RegisterLazySingleton(() => new MainView(), typeof(IViewFor<MainViewModel>));
            RxUIResolver.RegisterLazySingleton(() => new ScheduleView(), typeof(IViewFor<IScheduleViewModel>));
            RxUIResolver.RegisterLazySingleton(() => new SignInRegisterView(), typeof(IViewFor<ISignInRegisterViewModel>));
            RxUIResolver.RegisterLazySingleton(() => new SettingsView(), typeof(IViewFor<ISettingsViewModel>));
            RxUIResolver.RegisterLazySingleton(() => new AboutView(), typeof(IViewFor<IAboutViewModel>));
            RxUIResolver.RegisterLazySingleton(() => new AddPlantView(), typeof(IViewFor<IAddEditPlantViewModel>));
            RxUIResolver.RegisterLazySingleton(() => new PlantSingularView(), typeof(IViewFor<IPlantSingularViewModel>));
            RxUIResolver.RegisterLazySingleton(() => new PlantActionAddEditView(), typeof(IViewFor<IPlantActionViewModel>));
            RxUIResolver.RegisterLazySingleton(() => new YAxisShitView(), typeof(IViewFor<IYAxisShitViewModel>));
            RxUIResolver.RegisterLazySingleton(() => new ListUsersView(), typeof(IViewFor<ISearchUsersViewModel>));
            RxUIResolver.RegisterLazySingleton(() => new PlantActionListView(), typeof(IViewFor<IPlantActionListViewModel>));
            RxUIResolver.RegisterLazySingleton(() => new GardenPivotView(), typeof(IViewFor<IGardenPivotViewModel>));
            RxUIResolver.RegisterLazySingleton(() => new PlantPhotoPivotView(), typeof(IViewFor<IPhotoListViewModel>));
            RxUIResolver.RegisterLazySingleton(() => new FriendsPivotView(), typeof(IViewFor<IFriendsViewModel>));

            GSViewLocator.Instance.ViewModelToViewModelInterfaceFunc = T =>
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

            };
        }




    }

    public class BootstrapDesign : NinjectModule
    {


        public override void Load()
        {
            Bind<IUserService>().To<NullUserService>().InSingletonScope();
            Bind<IUIPersistence>().To<NullUIPersistence>().InSingletonScope();
            //Bind<IDispatchCommands>().To<NullCommandHandler>().InSingletonScope();

        }

    }


}

