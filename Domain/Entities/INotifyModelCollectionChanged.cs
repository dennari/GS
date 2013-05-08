using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Growthstories.WP8.Domain.Entities
{
    public interface INotifyModelCollectionChanged
    {
        event EventHandler<ModelCollectionChangedEventArgs<ModelBase>> ModelBaseCollectionChanged;
    }
}
