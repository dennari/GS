using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.WP8.Domain.Entities
{

    public class ModelCollectionChangedEventArgs<T> : EventArgs where T : ModelBase
    {
        public NotifyCollectionChangedAction Action { get { return _action; } set { _action = value; } }
        public ModelBase Owner { get { return _owner; } set { _owner = value; } }
        public IList<T> Items { get { return _items; } set { _items = value; } }
        public int Index { get { return _index; } set { _index = value; } }

        private ModelBase _owner;
        private IList<T> _items;
        private int _index;
        private NotifyCollectionChangedAction _action;

        public ModelCollectionChangedEventArgs(IList<T> items, NotifyCollectionChangedAction action)
        {
            _items = items;
            _action = action;
        }


        //public NotifyCollectionChangedAction Action;

    }



    public class ModelCollection<T> : ObservableCollection<T>, INotifyModelCollectionChanged, INotifyCollectionChanged where T : ModelBase
    {
        private ModelBase _owner;

        public event EventHandler<ModelCollectionChangedEventArgs<ModelBase>> ModelBaseCollectionChanged;


        public ModelCollection(ModelBase owner)
            : base()
        {
            _owner = owner;
        }

        public ModelCollection(ModelBase owner, IEnumerable<T> l)
            : base(l)
        {
            _owner = owner;
        }

        public ModelCollection(ModelBase owner, List<T> l)
            : base(l)
        {
            _owner = owner;
        }


        protected override void InsertItem(int index, T item)
        {
            if (_owner != null)
            {
                item.Parent = _owner;
            }
            base.InsertItem(index, item);
            if (ModelBaseCollectionChanged != null)
            {
                ModelBaseCollectionChanged(this, new ModelCollectionChangedEventArgs<ModelBase>(new List<ModelBase>(new[] { item as ModelBase }), NotifyCollectionChangedAction.Add)
                {
                    Owner = _owner,
                    Index = index
                });
            }
        }

        protected override void RemoveItem(int index)
        {
            T item = this[index];
            base.RemoveItem(index);
            if (ModelBaseCollectionChanged != null)
            {
                ModelBaseCollectionChanged(this, new ModelCollectionChangedEventArgs<ModelBase>(new List<ModelBase>(new[] { item as ModelBase }), NotifyCollectionChangedAction.Remove)
                {
                    Owner = _owner,
                    Index = index
                });
            }
        }



    }

    public class ModelCollection : ObservableCollection<ModelBase>, INotifyModelCollectionChanged, INotifyCollectionChanged
    {
        private ModelBase _owner;

        public event EventHandler<ModelCollectionChangedEventArgs<ModelBase>> ModelBaseCollectionChanged;


        public ModelCollection(ModelBase owner)
            : base()
        {
            _owner = owner;
        }

        public ModelCollection(ModelBase owner, IEnumerable<ModelBase> l)
            : base(l)
        {
            _owner = owner;
        }

        public ModelCollection(ModelBase owner, List<ModelBase> l)
            : base(l)
        {
            _owner = owner;
        }


        protected override void InsertItem(int index, ModelBase item)
        {
            if (_owner != null)
            {
                item.Parent = _owner;
            }
            base.InsertItem(index, item);
            ModelBaseCollectionChanged(this, new ModelCollectionChangedEventArgs<ModelBase>(new List<ModelBase>(new[] { item }), NotifyCollectionChangedAction.Add)
            {
                Owner = _owner,
                Index = index
            });
        }

        protected override void RemoveItem(int index)
        {
            ModelBase item = this[index];
            base.RemoveItem(index);
            ModelBaseCollectionChanged(this, new ModelCollectionChangedEventArgs<ModelBase>(new List<ModelBase>(new[] { item }), NotifyCollectionChangedAction.Remove)
            {
                Owner = _owner,
                Index = index
            });
        }



    }
}


