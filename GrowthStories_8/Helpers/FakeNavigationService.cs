using System;
using System.Collections.Generic;

namespace Growthstories.PCL.Helpers
{



    public class FakeNavigationService : INavigationService
    {

        public List<Uri> Stack = new List<Uri>();

        public Uri CurrentLocation
        {
            get
            {
                return Stack[Stack.Count - 1];
            }
            private set
            {

            }
        }

        public void GoBack()
        {
            Stack.RemoveAt(Stack.Count - 1);
        }

        public void NavigateTo(Uri pageUri)
        {
            Stack.Add(pageUri);
        }

    }
}