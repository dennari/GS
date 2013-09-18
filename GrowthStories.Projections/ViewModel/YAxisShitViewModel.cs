using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using Growthstories.Domain;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;

namespace Growthstories.UI.ViewModel
{
    public interface IYAxisShitViewModel : IGSRoutableViewModel, IHasAppBarButtons, IControlsPageOrientation
    {

    }

    public class YAxisShitViewModel : RoutableViewModel, IYAxisShitViewModel
    {

        private readonly PlantState State;


        public YAxisShitViewModel(PlantState state, IGSApp app)
            : base(app)
        {

            this.State = state;
            this.App.WhenAny(x => x.Orientation, x => x.GetValue()).Subscribe(x =>
            {
                if ((x & PageOrientation.Portrait) == PageOrientation.Portrait)
                {
                    App.Router.NavigateBack.Execute(null);
                }
            });

        }
        protected ReactiveList<ButtonViewModel> _AppBarButtons;
        public ReactiveList<ButtonViewModel> AppBarButtons
        {
            get
            {
                if (_AppBarButtons == null)
                    _AppBarButtons = new ReactiveList<ButtonViewModel>()
                    {
                        new ButtonViewModel(null)
                        {
                            Text = "check",
                            IconUri = App.IconUri[IconType.CHECK]
                        }
                    };
                return _AppBarButtons;
            }
        }


        public ApplicationBarMode AppBarMode
        {
            get { return ApplicationBarMode.DEFAULT; }
        }

        public bool AppBarIsVisible
        {
            get { return true; }
        }

        public override string UrlPathSegment
        {
            get { throw new NotImplementedException(); }
        }

        public SupportedPageOrientation SupportedOrientations
        {
            get { return SupportedPageOrientation.PortraitOrLandscape; }
        }


    }
}
