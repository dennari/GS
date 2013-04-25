
using System.Linq;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using Microsoft.Phone.Data.Linq;
using Microsoft.Phone.Data.Linq.Mapping;
using System;
using Growthstories.WP8.Domain.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;
using Growthstories.PCL.Services;
using Ninject;
using System.ComponentModel;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Reflection;
using System.Collections.Specialized;
using Growthstories.WP8.ViewModel;
using Growthstories.PCL.Helpers;
using Ninject.Components;
using Ninject.Activation.Strategies;
using Ninject.Activation;
using Ninject.Parameters;

namespace Growthstories.WP8.Services
{


    public class EntityChanges<T>
    {

        private List<Tuple<T, DateTime>> _added;
        private List<Tuple<T, DateTime>> _removed;
        private List<Tuple<T, string, object, DateTime>> _modified;

        public EntityChanges()
        {
            _added = new List<Tuple<T, DateTime>>();
            _removed = new List<Tuple<T, DateTime>>();
            _modified = new List<Tuple<T, string, object, DateTime>>();
        }

        public List<Tuple<T, DateTime>> Added { get { return _added; } }
        public List<Tuple<T, DateTime>> Removed { get { return _removed; } }
        public List<Tuple<T, string, object, DateTime>> Modified { get { return _modified; } }


    }



    public class ActivationNotificationActivationStrategy : NinjectComponent, IActivationStrategy
    {
        public event Action<object> Activated;

        public void Activate(IContext context, InstanceReference reference)
        {
            if (this.Activated != null && reference.Instance is ModelBase)
            {
                this.Activated(reference.Instance);
            }
        }

        public void Deactivate(IContext context, InstanceReference reference)
        {
        }
    }


    public class FakeWP8DataService : IDataService
    {


        private readonly IKernel _kernel;


        private EntityChanges<ModelBase> _changes;

        public EntityChanges<ModelBase> Changes
        {
            get
            {
                return _changes;
            }
            private set
            {
                _changes = value;
            }
        }

        private static IDataService _instance;

        public static IDataService getDataService()
        {
            if (_instance == null)
            {
                _instance = new FakeWP8DataService(new StandardKernel());
            }
            return _instance;
        }

        public static void releaseDataService()
        {
            _instance = null;
        }

        protected FakeWP8DataService(IKernel kernel)
        {
            _kernel = kernel;
            _changes = new EntityChanges<ModelBase>();
            _bind();

        }

        protected void _bind()
        {
            var k = _kernel;

            k.Components.Add<IActivationStrategy, ActivationNotificationActivationStrategy>();
            k.Components.GetAll<IActivationStrategy>()
                .OfType<ActivationNotificationActivationStrategy>()
                .Single().Activated += OnActivate;


            k.Bind<GardenViewModel>().ToSelf().InSingletonScope();
            k.Bind<AddPlantViewModel>().ToSelf().InSingletonScope();
            k.Bind<ActionViewModel>().ToSelf().InSingletonScope();
            k.Bind<PlantViewModel>().ToSelf().InSingletonScope();

            k.Bind<User>().ToSelf().InSingletonScope();
            k.Bind<Garden>().ToSelf().InSingletonScope();
            k.Bind<Plant>().ToSelf();
            k.Bind<IPlantDataService>().To<FakePlantDataService>().InSingletonScope();
            k.Bind<IPictureService>().To<PictureService>().InSingletonScope();
            k.Bind<INavigationService>().To<FakeNavigationService>().InSingletonScope();
        }

        public T Get<T>(long? id = null)
        {

            return _kernel.Get<T>();

        }

        public T Get<T>(params IParameter[] parameters)
        {
            //var a = _kernel.Get<T>(new [] {new ConstructorArgument("uuh", "aah"), new ConstructorArgument("uuh", "aah")});
            return _kernel.Get<T>(parameters);

        }

        protected void OnActivate(object o)
        {

            try
            {
                var p = (ModelBase)o;
                if (p is User)
                {
                    return;
                }
                listen(p);
                var props = from prop in p.GetType().GetProperties()
                            where typeof(INotifyModelCollectionChanged).IsAssignableFrom(prop.PropertyType)
                            select prop;

                foreach (var prop in props)
                {
                    var coll = (INotifyModelCollectionChanged)prop.GetMethod.Invoke(p, null);
                    coll.ModelBaseCollectionChanged += coll_ModelBaseCollectionChanged;
                }

                _changes.Added.Add(Tuple.Create(p, DateTime.UtcNow));
            }
            catch (InvalidCastException)
            {

            }



        }





        void coll_ModelBaseCollectionChanged(object sender, ModelCollectionChangedEventArgs<ModelBase> e)
        {

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                _changes.Removed.Add(Tuple.Create(e.Items[0], DateTime.UtcNow));
            }
        }

        void coll_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

        }


        private ModelBase listen(ModelBase model)
        {
            model.PropertyChanged += model_PropertyChanged;

            return model;
        }

        void model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var m = sender as ModelBase;
            try
            {
                PropertyInfo p = m.GetType().GetProperty(e.PropertyName);
                Type rType = p.GetMethod.ReturnType;
                object v = p.GetMethod.Invoke(m, null);
                if (p != null)
                {
                    _changes.Modified.Add(Tuple.Create(m, e.PropertyName, v, DateTime.UtcNow));
                }

            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<Garden> LoadGarden(User u)
        {
            throw new NotImplementedException();
        }

        public async Task<IList<Garden>> LoadGardens(User u)
        {
            throw new NotImplementedException();
        }
    }
}
