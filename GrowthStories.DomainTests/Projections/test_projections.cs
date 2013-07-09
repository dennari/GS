using System;
using System.Linq;

using NUnit.Framework;
using Growthstories.Domain.Messaging;
using Ninject;
using Growthstories.Core;
using CommonDomain.Persistence;
using Growthstories.Sync;
using Ninject.Parameters;
using SimpleTesting;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using System.Net.Http;
using System.Collections.Generic;
using Growthstories.Domain.Entities;
using CommonDomain;
using EventStore;

using CommonDomain.Persistence.EventStore;
using EventStore.Dispatcher;
using Growthstories.Domain;
using EventStore.Logging;
using Growthstories.UI;

namespace Growthstories.DomainTests
{
    public class ProjectionTest
    {

        IKernel kernel;
        [SetUp]
        public void SetUp()
        {
            if (kernel != null)
                kernel.Dispose();
            kernel = new StandardKernel(new TestModule());
            Log.Info("-----------------------------------------------------------------------------");
        }
        private ILog Log = new LogTo4Net(typeof(ProjectionTest));

        public T Get<T>() { return kernel.Get<T>(); }
        public IDispatchCommands Handler { get { return Get<IDispatchCommands>(); } }
        public ISynchronizerService Synchronizer { get { return Get<ISynchronizerService>(); } }
        public IStoreSyncHeads SyncStore { get { return Get<IStoreSyncHeads>(); } }
        public IRequestFactory RequestFactory { get { return Get<IRequestFactory>(); } }

        public ITransportEvents Transporter { get { return Get<ITransportEvents>(); } }
        public string toJSON(object o) { return Get<IJsonFactory>().Serialize(o); }
        public IRepository Repository { get { return Get<IRepository>(); } }
        public IStoreEvents EventStore { get { return Get<IStoreEvents>(); } }


        public IDispatchCommits Dispatcher { get { return Get<IDispatchCommits>(); } }
        public IAuthUser CurrentUser { get { return Get<IUserService>().CurrentUser; } }
        public FakeHttpClient HttpClient { get { return kernel.Get<IHttpClient>() as FakeHttpClient; } }
        public CompareObjects Comparer { get { return new CompareObjects(); } }

        [Test]
        public void TestPlantProjection()
        {

            var proj = kernel.Get<PlantProjection>();
            var PlantId = Guid.NewGuid();
            var UserId = Guid.NewGuid();

            var Name = "Jore";

            Handler.Handle<Plant, CreatePlant>(new CreatePlant(PlantId, Name, UserId));
            Assert.AreEqual(1, proj.PlantNames.Count);
            Assert.AreEqual(Name, proj.PlantNames[0]);
        }

        [Test]
        public void TestActionProjection()
        {

            var proj = kernel.Get<ActionProjection>();
            var UserId = Guid.NewGuid();
            var PlantId = Guid.NewGuid();
            var PlantId2 = Guid.NewGuid();

            var Note = "EI NAIN!";
            var uri = new Uri("http://www.growthstories.com");
            var uCmd = new CreateUser(UserId, "Alice", "swordfish", "alice@wonderland.net");

            Handler.Handle<User, CreateUser>(uCmd);
            Handler.Handle<User, Comment>(new Comment(UserId, PlantId, Note));
            Handler.Handle<User, Photograph>(new Photograph(UserId, PlantId, Note, uri));


            //Handler.Handle<Plant, CreatePlant>(new CreatePlant(PlantId, Name));
            var list = proj.Actions[PlantId];
            Assert.AreEqual(2, list.Count);
            var comment = list[0] as Commented;
            Assert.IsInstanceOf<Commented>(comment);
            Assert.AreEqual(Note, comment.Note);
            var ph = proj.Actions[PlantId][1] as Photographed;
            Assert.IsInstanceOf<Photographed>(ph);
            Assert.AreSame(uri, ph.Uri);

            proj.Actions.Clear();
            Assert.AreEqual(0, proj.Actions.Count);
            var actions = proj.LoadWithPlantId(PlantId).ToArray();

            Assert.AreEqual(2, actions.Length);
            comment = actions[0] as Commented;
            Assert.IsInstanceOf<Commented>(comment);
            Assert.AreEqual(Note, comment.Note);

            ph = actions[1] as Photographed;
            Assert.IsInstanceOf<Photographed>(ph);
            Assert.AreNotSame(uri, ph.Uri);
            Assert.AreEqual(uri.ToString(), ph.Uri.ToString());

            Handler.Handle<User, Comment>(new Comment(UserId, PlantId2, Note));
            actions = proj.LoadWithUserId(UserId).ToArray();
            Assert.AreEqual(3, actions.Length);
            Assert.AreEqual(PlantId, actions[0].PlantId);
            Assert.AreEqual(PlantId2, actions[2].PlantId);


        }

    }
}
