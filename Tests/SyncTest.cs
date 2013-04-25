using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Growthstories.WP8.Domain.Entities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ninject;
using Growthstories.PCL.Services;
using GalaSoft.MvvmLight;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Growthstories.PCL.Helpers;
using Growthstories.WP8.Helpers;
using Growthstories.WP8.Services;
using System.Diagnostics;
using System.Linq;
using Ninject.Parameters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace Growthstories.Tests
{



    [TestClass]
    public class SyncTest
    {

        public IDataService kernel;
        public IDataService repository;

        [TestInitialize]
        public void TestInitialize()
        {
            kernel = FakeWP8DataService.getDataService();
            repository = kernel;

        }

        [TestCleanup]
        public void TestCleanup()
        {
            FakeWP8DataService.releaseDataService();

        }

        public string toJson(object o)
        {
            var settings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            return JsonConvert.SerializeObject(o, Formatting.Indented, settings);
        }

        [TestMethod]
        public void TestAddPlant()
        {
            //Garden g = repository.GetNew<Garden>();
            Garden g = repository.Get<Garden>();

            Plant p = repository.Get<Plant>();

            g.Plants.Add(p);


            Assert.AreSame(g, p.Parent, "plant's parent is not set");
            Assert.AreEqual(2, repository.Changes.Added.Count());
            Assert.AreEqual(repository.Changes.Added.Count, 2, "ADDED count");
            Assert.AreSame(repository.Changes.Added[0].Item1, g, "added 0 " + repository.Changes.Added[0].Item1.GetType());
            Assert.AreSame(repository.Changes.Added[1].Item1, p, "added 1 " + repository.Changes.Added[1].Item1.GetType());

            g.Plants.Remove(p);

            Assert.AreEqual(repository.Changes.Removed.Count, 1, "removed count");
            Assert.AreSame(repository.Changes.Removed[0].Item1, p, "removed 1");
            // parent has been set for the plant
            Assert.AreEqual(repository.Changes.Modified.Count, 1, "modified count");
            Assert.AreEqual(repository.Changes.Modified[0].Item2, "Parent", "modified paramname");
            Assert.AreSame(repository.Changes.Modified[0].Item3, g, "modified paramvalue");

            Garden g2 = repository.Get<Garden>();

            Assert.AreSame(g, g2, "garden is not a singleton");


        }

        [TestMethod]
        public void TestJsonAdd()
        {
            string input = @"{
               'instruction': 'add',
               'id': '234252',
               'type': 'Plant',
               'created': '2012-03-19T07:22Z',
               'name': 'Sepi',
               'genus': 'Aloe Vera',
               'actions': [
                {
                   'type': 'WateringAction',
                   'id': '4264564',
                   'created': '2012-03-19T07:22Z'
                },
                {
                   'type': 'WateringAction',
                   'id': '4264564',
                   'created': '2012-03-19T07:22Z'
                }
                ]    
            }";



        }

        [TestMethod]
        public void TestJsonModify()
        {
            string input = @"{
               'instruction': 'modify',
               'id': '234252',
               ''
            
            }";
        }





    }
}

