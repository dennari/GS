using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Growthstories.WP8.Services
{

    public delegate void ObjectCreatedEventHandler(Type T, object O);

    public class Factory
    {

        public Factory(IKernel kernel)
        {

        }

        public event ObjectCreatedEventHandler ObjectCreatedHandler;

    }
}
