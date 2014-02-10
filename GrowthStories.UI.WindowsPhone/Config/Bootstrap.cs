
using System;
using System.Diagnostics;
using System.Windows.Media;

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
using Microsoft.Phone.Controls;
using System.Collections.Generic;
using EventStore.Logging;
using System.IO.IsolatedStorage;

namespace Growthstories.UI.WindowsPhone
{


    public class Bootstrap : BaseSetup
    {

        private static ILog Logger = LogFactory.BuildLogger(typeof(Bootstrap));


        protected readonly App PhoneApp;
        protected const string BUGSENSE_TOKEN = "e73c0669";

        public Bootstrap(App phoneApp)
        {
            PhoneApp = phoneApp;
            PhoneApp.UnhandledException += HandleUnhandledExceptions;
        }

        public override void Load()
        {
            base.Load();

            PrintLastUnhandledException();

            ApplyGSAccentColor();
            BAConfiguration();
            ViewModelConfiguration();
            ViewConfiguration();
        }

        protected virtual void PrintLastUnhandledException()
        {
            var settings = IsolatedStorageSettings.ApplicationSettings;
            if (settings.Contains("lastException"))
            {
                PhoneApp.Log().Info("Exception on last crash was {0}", settings["lastException"]);
            }
            else
            {
                PhoneApp.Log().Info("No crashes recorded");
            }
        }

        protected virtual void HandleUnhandledExceptions(object sender, ApplicationUnhandledExceptionEventArgs ee)
        {
            // try to log the Exception
            try
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["lastException"] = ee.ExceptionObject.ToStringExtended();
                settings.Save();

                PhoneApp.Log().DebugExceptionExtended("Unhandled", ee.ExceptionObject);
            }
            catch { }
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
            ImagingExtensions.Handler = new WP8PhotoHandler();
        }


        protected override void AppConfiguration()
        {
            Bind<IIAPService>().To<GSIAPMock>().InSingletonScope();
            Bind<IScheduleService>().To<GardenScheduler>()
                .InSingletonScope()
                .WithConstructorArgument(new TimeSpan(0, 0, 10));

            Bind<ISynchronizer>().To<NonAutoSyncingSynchronizer>();

        }




        protected override string SQLPersistenceDBName()
        {
            //return base.SQLPersistenceDBName();
            return Path.Combine(ApplicationData.Current.LocalFolder.Path, "GS.sqlite");
        }

        protected override string UIPersistenceDBName()
        {
            return Path.Combine(ApplicationData.Current.LocalFolder.Path, "GSUI.sqlite");
        }

        private void ApplyGSAccentColor()
        {
            try
            {
                ThemeManager.OverrideOptions = ThemeManagerOverrideOptions.SystemTrayColors;
                ThemeManager.ToDarkTheme();

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
                Logger.Warn("could not apply gs theme");
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

            RxUIResolver.Register(() => new GardenPivotView(), typeof(IViewFor<IGardenPivotViewModel>));

            //RxUIResolver.RegisterLazySingleton(() => new PlantPhotoPivotView(), typeof(IViewFor<IPhotoListViewModel>));

            // the rad slider filmstrip mode does have some messy state, and therefore we want to start clean each time
            RxUIResolver.Register(() => new PlantPhotoPivotView(), typeof(IViewFor<IPhotoListViewModel>));


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


