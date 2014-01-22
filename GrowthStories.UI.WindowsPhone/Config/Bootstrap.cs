
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
using System.Windows;
using Windows.Storage;
using System.IO;
using Growthstories.Configuration;
using Growthstories.Domain.Services;
using Growthstories.Core;


namespace Growthstories.UI.WindowsPhone
{


    public class Bootstrap : BaseSetup
    {
        protected readonly App PhoneApp;
        protected const string BUGSENSE_TOKEN = "e73c0669";

        public Bootstrap(App phoneApp)
        {
            PhoneApp = phoneApp;

        }

        public override void Load()
        {
            base.Load();

            ApplyGSAccentColor();
            BAConfiguration();
            ViewModelConfiguration();
            ViewConfiguration();


            var excMgr = new ExceptionManager((Application)PhoneApp);
            BugSenseHandler.Instance.InitAndStartSession(excMgr, BUGSENSE_TOKEN);
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

        protected override void RxUIConfiguration()
        {
            base.RxUIConfiguration();
            RxUIResolver.GetService<ISuspensionHost>().SetupDefaultSuspendResume(RxUIResolver.GetService<ISuspensionDriver>());

        }


        protected override void FileSystemConfiguration()
        {
            Bind<IPhotoHandler>().To<WP8PhotoHandler>();

        }


        protected override void AppConfiguration()
        {
            Bind<IIAPService>().To<GSIAPMock>().InSingletonScope();
            Bind<IScheduleService>().To<GardenScheduler>()
                .InSingletonScope()
                .WithConstructorArgument(new TimeSpan(0, 0, 10));

            Bind<ISynchronizer>().To<NonAutoSyncingSynchronizer>();

        }


        protected override void SQLiteConnectionConfiguration(string dbname = "GS.sqlite")
        {
            var dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, dbname);
            base.SQLiteConnectionConfiguration(dbpath);
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

                var ebb = (SolidColorBrush)PhoneApp.Resources["PhoneTextBoxEditBorderBrush"];
                ebb.Color = ac;
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

            Type machine = Type.GetType("Growthstories.UI.WindowsPhone.BootstrapMachine", false);
            if (machine != null)
            {
                return (INinjectModule)Activator.CreateInstance(machine, app);
            }

            return new BootstrapProduction(app);
        }



        protected virtual void ViewModelConfiguration()
        {
            Bind<IGSAppViewModel, IApplicationRootState, IScreen>().To<ClientAppViewModel>().InSingletonScope();
            Bind<TestingViewModel>().To<ClientTestingViewModel>().InSingletonScope();
            Bind<IAboutViewModel>().To<AboutViewModel>().InSingletonScope();
            Bind<ISearchUsersViewModel>().To<SearchUsersViewModel>().InSingletonScope();
            Bind<FriendsViewModel>().To<FriendsViewModel>().InSingletonScope();

            RxUIResolver.RegisterLazySingleton(() => KernelInstance.GetService(typeof(IApplicationRootState)), typeof(IApplicationRootState));
            RxUIResolver.RegisterLazySingleton(() => KernelInstance.GetService(typeof(ISearchUsersViewModel)), typeof(ISearchUsersViewModel));
            RxUIResolver.RegisterLazySingleton(() => KernelInstance.GetService(typeof(IAboutViewModel)), typeof(IAboutViewModel));
            RxUIResolver.RegisterLazySingleton(() => KernelInstance.GetService(typeof(FriendsViewModel)), typeof(FriendsViewModel));
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

            //RxUIResolver.RegisterLazySingleton(() => new FriendsPivotView(), typeof(IViewFor<IFriendsViewModel>));
            RxUIResolver.Register(() => new FriendsPivotView(), typeof(IViewFor<IFriendsViewModel>));

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


