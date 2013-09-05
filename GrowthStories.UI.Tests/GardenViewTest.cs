using Growthstories.Domain.Messaging;
using Growthstories.UI.ViewModel;
using NUnit.Framework;
using System;
using System.Linq;


namespace Growthstories.UI.Tests
{
    public class GardenViewModelTest : ViewModelTestBase
    {


        Guid PlantId;
        GardenViewModel GVM;
        PlantViewModel PVM;


        //    [Test]
        //    public void TestGardenViewModel()
        //    {

        //        GVM = Get<GardenViewModel>();
        //        PlantId = Guid.NewGuid();
        //        var PlantName = "Jore";

        //        //GVM.NewPlantId = PlantId;
        //        //GVM.NewPlantName = PlantName;
        //        //GVM.AddPlantCommand.Execute(null);

        //        Assert.AreEqual(1, GVM.Plants.Count);
        //        Assert.AreEqual(PlantName, GVM.Plants[0].Name);

        //    }

        //    [Test]
        //    public void TestPlantViewModel()
        //    {

        //        TestGardenViewModel();
        //        GVM.ShowDetailsCommand.Execute(GVM.Plants[0]);

        //        PVM = Get<PlantViewModel>();
        //        Assert.IsNotNull(PVM.Plant);
        //        Assert.AreEqual(PlantId, PVM.Plant.EntityId);
        //        //var UserId = Guid.NewGuid();
        //        //var PlantId = Guid.NewGuid();
        //        var PlantId2 = Guid.NewGuid();

        //        var Note = "EI NAIN!";
        //        var uri = new Uri("http://www.growthstories.com");
        //        //var uCmd = new CreateUser(UserId, "Alice", "swordfish", "alice@wonderland.net");

        //        //Handler.Handle<User, CreateUser>(uCmd);
        //        //Handler.Handle<User, Comment>(new Comment(UserId, PlantId, Note));
        //        //Handler.Handle<User, Photograph>(new Photograph(UserId, PlantId, Note, uri));
        //        PVM.AddCommentCommand.Execute(Note);
        //        PVM.AddPhotoCommand.Execute(uri);
        //        PVM.AddFertilizerCommand.Execute(null);
        //        PVM.AddWaterCommand.Execute(null);



        //        //Handler.Handle<Plant, CreatePlant>(new CreatePlant(PlantId, Name));
        //        var list = PVM.Actions;
        //        Assert.AreEqual(4, list.Count);
        //        var comment = list[0] as Commented;
        //        Assert.IsInstanceOf<Commented>(comment);
        //        Assert.AreEqual(Note, comment.Note);
        //        var ph = list[1] as Photographed;
        //        Assert.IsInstanceOf<Photographed>(ph);
        //        Assert.AreSame(uri, ph.Uri);
        //        Assert.IsInstanceOf<Fertilized>(list[2]);
        //        Assert.IsInstanceOf<Watered>(list[3]);



        //        PVM.Actions.Clear();
        //        Assert.AreEqual(0, PVM.Actions.Count);

        //        var proj = Get<ActionProjection>();
        //        var actions = proj.LoadWithPlantId(PlantId).ToArray();

        //        Assert.AreEqual(4, actions.Length);
        //        comment = actions[0] as Commented;
        //        Assert.IsInstanceOf<Commented>(comment);
        //        Assert.AreEqual(Note, comment.Note);

        //        ph = actions[1] as Photographed;
        //        Assert.IsInstanceOf<Photographed>(ph);
        //        Assert.AreNotSame(uri, ph.Uri);
        //        Assert.AreEqual(uri.ToString(), ph.Uri.ToString());

        //        Handler.Handle(new Comment(Ctx.Id, PlantId2, Note));
        //        actions = proj.LoadWithUserId(Ctx.Id).ToArray();
        //        Assert.AreEqual(5, actions.Length);
        //        Assert.AreEqual(PlantId, actions[0].PlantId);
        //        Assert.AreEqual(PlantId2, actions.Last().PlantId);


        //    }

    }
}
