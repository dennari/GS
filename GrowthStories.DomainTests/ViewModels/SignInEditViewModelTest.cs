using Growthstories.Domain.Messaging;
using Growthstories.Domain;
using Growthstories.UI.ViewModel;
using NUnit.Framework;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive;
using System.Threading;
using Growthstories.Domain.Entities;
using Growthstories.Sync;
using System.Collections.Generic;
using Ninject;
using System.Threading.Tasks;


namespace Growthstories.DomainTests
{


    public class SignInEditViewModelTest : ViewModelTestBase
    {

        protected PlantState State;

        [SetUp]
        public override void SetUp()
        {
            if (Kernel != null)
                Kernel.Dispose();
            Kernel = new StandardKernel(new SignInTestsSetup());
            App = new StagingAppViewModel(Kernel);

            //var u = App.User;
            //Assert.IsNotNull(u);
            //Assert.IsNotNull(u.Username);
            //Assert.IsNull(App.Model.State.User);
            //Handler = Get<IDispatchCommands>();
            //Handler.Handle(new CreateUser(u.Id, u.Username, u.Password, u.Email));
            //Handler.Handle(new AssignAppUser(u.Id, u.Username, u.Password, u.Email));
            //Handler.Handle(new CreateGarden(u.GardenId, u.Id));


            //Ctx = Get<IUserService>().CurrentUser;
            //Assert.IsNotNull(App.Model.State.User);


        }

        protected IPlantViewModel CreatePlantVM(IScheduleViewModel wateringSchedule = null, IScheduleViewModel fertilizingSchedule = null)
        {



            var name = "Sepi";
            var id = Guid.NewGuid();

            var plant = (Plant)Handler.Handle(new CreatePlant(id, name, U.GardenId, U.Id));
            this.State = plant.State;
            var vm = new PlantViewModel(State, wateringSchedule, fertilizingSchedule, App);
            Assert.AreEqual(id, vm.Id);
            Assert.AreEqual(name, vm.Name);
            Assert.AreEqual(id, State.Id);
            Assert.AreEqual(name, State.Name);
            return vm;


        }

        [Test]
        public void TestCreateUnregUser()
        {

            Assert.IsNull(App.User);
            var u = TestUtils.WaitForTask(App.Initialize());
            Assert.IsNotNull(App.User);

            Assert.AreEqual(u.Id, App.User.Id);
            Assert.IsFalse(u.IsRegistered());

            var restarted = new StagingAppViewModel(Kernel);


            var u2 = TestUtils.WaitForTask(restarted.Initialize());

            Assert.AreEqual(u.Id, u2.Id);
            Assert.IsFalse(u2.IsRegistered());




        }

        [Test]
        public void TestRegisterNewUserImmediately()
        {

            var u = TestUtils.WaitForTask(App.Initialize());

            ISignInRegisterViewModel reg = new SignInRegisterViewModel(App);
            Tuple<bool, RegisterResponse, SignInResponse> R = null;
            var oneResponse = reg.Response.Take(1);
            oneResponse.Subscribe(x =>
            {
                R = x;
            });
            bool canExecute = false;
            reg.OKCommand.CanExecuteObservable.Subscribe(x => canExecute = x);

            reg.Username = "Jaakko";
            reg.Password = "swordfish";
            reg.PasswordConfirmation = "swordfishh";
            reg.Email = "jee@joo.net";

            Assert.IsFalse(canExecute);

            reg.PasswordConfirmation = reg.Password;

            Assert.IsTrue(canExecute);

            reg.OKCommand.Execute(null);
            TestUtils.WaitForTask(Task.Run(async () => await oneResponse));
            Assert.AreEqual(RegisterResponse.success, R.Item2);


        }

        [Test]
        [Ignore]
        public void TestRegisterNewUserLater()
        {

            TestUtils.WaitForTask(App.Initialize());
            var restarted = new StagingAppViewModel(Kernel);
            TestUtils.WaitForTask(restarted.Initialize());

        }




    }
}
