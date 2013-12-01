
using ReactiveUI;
using ReactiveUI.Mobile;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive;
using Ninject;
using Growthstories.UI.WindowsPhone;
using Growthstories.UI.ViewModel;
using Growthstories.Sync;
using System;
using System.Collections.Generic;
using Growthstories.Core;
using Growthstories.Domain.Entities;
using Growthstories.Domain;
using System.Threading.Tasks;
using Growthstories.Domain.Messaging;
using Growthstories.UI.WindowsPhone.ViewModels;
using EventStore.Persistence;
using EventStore;


namespace Growthstories.UI.WindowsPhone.ViewModels
{


    public class SignInRegisterViewModelDesign : SignInRegisterViewModel
    {
        public SignInRegisterViewModelDesign()
            : base(new MockAppViewModel(true))
        {

        }
    }

    public class GardenViewModelDesign : GardenViewModel
    {
        public GardenViewModelDesign()
            : base(null, new MockAppViewModel(true))
        {
            this.Plants = new ReactiveList<IPlantViewModel>()
            {
                new PlantViewModelDesign("Jare","Aloe Vera"),
                new PlantViewModelDesign("Kari","Phaleonopsis"),

            };
        }

        public new IReactiveList<IPlantViewModel> Plants { get; private set; }

    }

    public class PlantViewModelDesign : PlantViewModel
    {
        public PlantViewModelDesign()
            : base(null, new MockAppViewModel(true))
        {

        }

        public PlantViewModelDesign(string name, string species)
            : this()
        {
            this.Name = name;
            this.Species = species;
        }

        public new IReactiveList<IPlantViewModel> Plants { get; private set; }

    }

    public class PlantActionViewModelDesign : PlantActionViewModel
    {
        public PlantActionViewModelDesign(PlantActionType type)
            : base(type, new MockAppViewModel(true))
        {

        }

        public PlantActionViewModelDesign()
            : this(PlantActionType.WATERED)
        {

        }
    }


    public sealed class MockAppViewModel : Growthstories.UI.ViewModel.AppViewModel, IApplicationRootState
    {



        private Microsoft.Phone.Controls.SupportedPageOrientation _ClientSupportedOrientations;
        public Microsoft.Phone.Controls.SupportedPageOrientation ClientSupportedOrientations
        {
            get
            {
                return _ClientSupportedOrientations;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _ClientSupportedOrientations, value);
            }
        }



        public MockAppViewModel(bool IsRegistered = false)
            : base()
        {

            //this.Kernel = new StandardKernel(new BootstrapDesign());

            var userId = Guid.NewGuid();
            var gardenId = Guid.NewGuid();

            var commands = new IAggregateCommand[4];

            CreateUser u = null;
            if (IsRegistered)
                u = new CreateUser(userId, "RegUser", "unregpassword", string.Format("{0}{1}@growthstories.com", "jore", "_joojoo"));
            else
                u = new CreateUser(userId, "UnregUser", "unregpassword", string.Format("{0}{1}@growthstories.com", AuthUser.UnregEmailPrefix, Guid.NewGuid()));


            IAuthUser authUser = new AuthUser()
            {
                Id = userId,
                GardenId = gardenId,
                Username = u.Username,
                Password = u.Password,
                Email = u.Email
            };

            this.User = authUser;



        }





    }


}

