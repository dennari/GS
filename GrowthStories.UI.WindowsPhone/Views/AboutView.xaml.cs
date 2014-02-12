using Growthstories.UI.ViewModel;
using Microsoft.Phone.Tasks;

namespace Growthstories.UI.WindowsPhone

{
    public class AboutViewBase : GSView<IAboutViewModel>
    {

    }


    public partial class AboutView : AboutViewBase
    {

        public AboutView()
        {
            InitializeComponent();
        }

        
        private void About_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            WebBrowserTask browserTask = new WebBrowserTask();
            browserTask.Uri = new System.Uri("http://www.growthstories.com/about#contact");
            browserTask.Show();
        }


        private void Privacy_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            WebBrowserTask browserTask = new WebBrowserTask();
            browserTask.Uri = new System.Uri("http://www.growthstories.com/legal#privacy");
            browserTask.Show();
        }


        ~AboutView()
        {
            NotifyDestroyed("");
        }

    }


}