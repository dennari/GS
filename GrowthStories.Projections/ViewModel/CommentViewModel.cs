using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.UI.ViewModel
{

    public class PlantActionViewModel : RoutableViewModel
    {
        public ActionBase State { get; protected set; }

        public string WeekDay { get { return State.Created.ToString("dddd"); } }
        public string Date { get { return State.Created.ToString("d"); } }
        public string Time { get { return State.Created.ToString("t"); } }

        public string Title { get { return "TITLE"; } }
        protected IconType _IconType;
        public Uri Icon { get { return this.App.BigIconUri[this._IconType]; } }

        public PlantActionViewModel(ActionBase state, IGSApp app)
            : base(app)
        {
            this.State = state;
        }

        public override string UrlPathSegment
        {
            get { throw new NotImplementedException(); }
        }
    }


    public static class PlantActionViewModelFactory
    {
        public static PlantActionViewModel GetVM(ActionBase state, IGSApp app)
        {
            return Create((dynamic)state, app);
        }

        public static IEnumerable<PlantActionViewModel> GetVM(IEnumerable<ActionBase> states, IGSApp app)
        {
            foreach (var state in states)
                yield return Create((dynamic)state, app);
        }

        public static PlantActionViewModel Create(Commented state, IGSApp app)
        {
            return new CommentViewModel(state, app);
        }

        public static PlantActionViewModel Create(Watered state, IGSApp app)
        {
            return new WaterViewModel(state, app);
        }

        public static PlantActionViewModel Create(Fertilized state, IGSApp app)
        {
            return new FertilizeViewModel(state, app);
        }

        public static PlantActionViewModel Create(Photographed state, IGSApp app)
        {
            return new PhotographViewModel(state, app);
        }

        public static PlantActionViewModel Create(Measured state, IGSApp app)
        {
            return new MeasureViewModel(state, app);
        }
    }


    public class CommentViewModel : PlantActionViewModel
    {


        public new string Title { get { return "COMMENTED"; } }

        public CommentViewModel(Commented state, IGSApp app)
            : base(state, app)
        {
            this._IconType = IconType.NOTE;
        }


    }

    public class MeasureViewModel : PlantActionViewModel
    {


        public new string Title { get { return "MEASURED"; } }

        public string Series { get { return ((Measured)this.State).Series.ToString("G"); } }
        public string Value { get { return ((Measured)this.State).Value.ToString("F1"); } }



        public MeasureViewModel(Measured state, IGSApp app)
            : base(state, app)
        {
            this._IconType = IconType.MEASURE;
        }


    }

    public class WaterViewModel : PlantActionViewModel
    {

        public new string Title { get { return "WATERED"; } }


        public WaterViewModel(Watered state, IGSApp app)
            : base(state, app)
        {
            this._IconType = IconType.WATER;
        }


    }

    public class FertilizeViewModel : PlantActionViewModel
    {

        public new string Title { get { return "NOURISHED"; } }

        public FertilizeViewModel(Fertilized state, IGSApp app)
            : base(state, app)
        {
            this._IconType = IconType.FERTILIZE;
        }


    }

    public class PhotographViewModel : PlantActionViewModel
    {
        public new string Title { get { return "PHOTOGRAPHED"; } }

        public string Path { get { return ((Photographed)State).Uri.ToString(); } }

        public PhotographViewModel(Photographed state, IGSApp app)
            : base(state, app)
        {
            this._IconType = IconType.PHOTO;
        }


    }
}
