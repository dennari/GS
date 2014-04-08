using System;
using System.Linq;
using System.Reactive.Linq;
using EventStore.Logging;
using Growthstories.UI.ViewModel;
using ReactiveUI;

namespace Growthstories.UI.WindowsPhone
{

    public class FriendsPivotViewBase : GSView<IFriendsViewModel>
    {

    }


    public partial class FriendsPivotView : FriendsPivotViewBase
    {

        private static ILog Logger = LogFactory.BuildLogger(typeof(FriendsPivotView));

        Guid id;
        static ReactiveCommand Constructed = new ReactiveCommand();


        public FriendsPivotView()
        {
            id = Guid.NewGuid();
            InitializeComponent();

            Logger.Info("initializing new friendspivotview {0}", id);

            this.WhenAnyValue(x => x.ViewModel.Friends).Where(x => x != null).Subscribe(x =>
            {
                this.Friends.ItemsSource = null;
                this.Friends.ItemsSource = this.ViewModel.Friends.ToArray();
            });

            this.WhenAnyObservable(x => x.ViewModel.Friends.CountChanged).Subscribe(x =>
            {
                this.Friends.ItemsSource = null;
                this.Friends.ItemsSource = this.ViewModel.Friends.ToArray();
            });

            Constructed.Execute(null);
            Constructed.Take(1).Subscribe(_ => CleanUp());
        }


        private void CleanUp()
        {
            this.ViewModel.Log().Info("cleaning up friendspivot {0}", id);

            Friends.ItemsSource = null;
            //Friends.SelectedItem = null;
            ViewHelpers.ClearPivotDependencyValues(Friends);

            //LayoutRoot.Children.Clear();
        }


        ~FriendsPivotView()
        {
            NotifyDestroyed(id.ToString());
        }

    }
}