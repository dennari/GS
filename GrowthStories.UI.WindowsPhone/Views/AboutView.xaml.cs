using Growthstories.UI.ViewModel;
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
    }
}