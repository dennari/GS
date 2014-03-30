
using System;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Media;
using EventStore.Logging;
using Growthstories.Configuration;
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Sync;
using Growthstories.UI.Services;
using Growthstories.UI.ViewModel;
using Growthstories.UI.WindowsPhone.ViewModels;
using Microsoft.Phone.Controls;
using Ninject.Modules;
using ReactiveUI;
using ReactiveUI.Mobile;
using Windows.Storage;

namespace Growthstories.UI.WindowsPhone
{


    public class Bootstrap : BaseSetup
    {

        private static ILog Logger = LogFactory.BuildLogger(typeof(Bootstrap));


        protected readonly GSAutoSuspendApplication PhoneApp;
        protected const string BUGSENSE_TOKEN = "e73c0669";

        public Bootstrap(GSAutoSuspendApplication phoneApp)
        {
            PhoneApp = phoneApp;
            //PhoneApp.RootVisual.D
            //Deployment.Current.Dispatcher.

            //            RxApp.MainThreadScheduler.Schedule(PhoneApp, (sched, state) =>
            //            {
            //<<<<<<< Updated upstream
            //                PhoneApp.UnhandledException += HandleUnhandledExceptions;
            //                return Disposable.Empty;
            //            });

            //            RxApp.MainThreadScheduler.Schedule(PhoneApp, (sched, state) =>
            //            {
            //                ApplyGSAccentColor();
            //=======
            //                state.UnhandledException += HandleUnhandledExceptions;
            //>>>>>>> Stashed changes
            //                return Disposable.Empty;
            //            });
        }


        public override void Load()
        {
            base.Load();

            PrintLastUnhandledException();

            BAConfiguration();
            ViewModelConfiguration();

            // delay viewconfiguration for debugging purposes
            //var cmd = new ReactiveCommand();
            //cmd.Delay(TimeSpan.FromSeconds(5)).Subscribe(_ => ViewConfiguration());
            //cmd.Execute(null);

            ViewConfiguration();
        }


        protected virtual void PrintLastUnhandledException()
        {
            var settings = IsolatedStorageSettings.ApplicationSettings;
            if (settings.Contains("lastException"))
            {
                PhoneApp.Log().Info("Exception on last crash was: " + settings["lastException"]);
            }
            else
            {
                PhoneApp.Log().Info("No crashes recorded");
            }

            settings.Remove("lastException");
            settings.Save();
        }


        public static void HandleUnhandledExceptions(Exception e, GSAutoSuspendApplication app)
        {
            // try to log the Exception

            try
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["lastException"] = e.ToStringExtended();
                settings.Save();
                app.Log().DebugExceptionExtended("Unhandled", e);


            }
            catch (Exception)
            {
                if (Debugger.IsAttached)
                    Debugger.Break();
            }
        }


        protected virtual void BAConfiguration()
        {
            BAUtils.RegisterScheduledTask();
        }

        protected override void RxUIConfiguration()
        {
            base.RxUIConfiguration();
            //RxUIResolver.GetService<ISuspensionHost>().SetupDefaultSuspendResume(RxUIResolver.GetService<ISuspensionDriver>());

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


        public static void ApplyGSAccentColor(ResourceDictionary r)
        {
            if (r == null)
                return;
            try
            {
                ThemeManager.OverrideOptions = ThemeManagerOverrideOptions.SystemTrayColors;
                ThemeManager.ToDarkTheme();


                r.Remove("PhoneAccentColor");
                r.Add("PhoneAccentColor", r["GSAccentColor"]);

                var ab = (SolidColorBrush)r["PhoneAccentBrush"];
                var ac = (Color)r["PhoneAccentColor"];
                ab.Color = ac;

                var ebb = (SolidColorBrush)r["PhoneTextBoxEditBorderBrush"];
                ebb.Color = ac;
            }
            catch
            {
                Logger.Warn("could not apply gs theme");
            }
        }


        internal static INinjectModule GetModule(GSAutoSuspendApplication app)
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

            //RxUIResolver.RegisterLazySingleton(() => new GardenPivotView(), typeof(IViewFor<IGardenPivotViewModel>));

            RxUIResolver.Register(() => new GardenPivotView(), typeof(IViewFor<IGardenPivotViewModel>));

            //RxUIResolver.RegisterLazySingleton(() => new PlantPhotoPivotView(), typeof(IViewFor<IPhotoListViewModel>));

            // the rad slider filmstrip mode does have some messy state, and therefore we want to start clean each time
            RxUIResolver.Register(() => new PlantPhotoPivotView(), typeof(IViewFor<IPhotoListViewModel>));

            //RxUIResolver.RegisterLazySingleton(() => new FriendsPivotView(), typeof(IViewFor<IFriendsViewModel>));
            RxUIResolver.Register(() => new FriendsPivotView(), typeof(IViewFor<IFriendsViewModel>));

            //Task.Run(async () =>
            //{
            //    await Task.Delay(12000);
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
            //});

            Logger.Info("gsviewlocator viewmodeltoviewmodelinterfacefunc is now set");
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


