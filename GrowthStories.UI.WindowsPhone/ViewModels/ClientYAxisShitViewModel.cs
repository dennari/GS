using Growthstories.Domain.Entities;
using Growthstories.Sync;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using ReactiveUI;
using System.Windows.Media.Imaging;
using Growthstories.Domain.Messaging;
//using Telerik.Windows.Controls;
using System.Windows.Media;
using Telerik.Windows.Controls;
using Growthstories.UI.ViewModel;

namespace Growthstories.UI.WindowsPhone.ViewModels
{


    public class ClientYAxisShitViewModel : YAxisShitViewModel
    {



        public ClientYAxisShitViewModel(IPlantViewModel plantVM, IGSAppViewModel app)
            : base(plantVM, app)
        {


            this.CategoryBinding = new GenericDataPointBinding<IPlantMeasureViewModel, DateTime>()
            {
                ValueSelector = x => new DateTime(x.Created.Ticks, DateTimeKind.Utc)
            };
            this.ValueBinding = new GenericDataPointBinding<IPlantMeasureViewModel, Double>()
            {
                ValueSelector = x => x.Value.Value
            };



        }

        protected override void SetSeries(MeasurementType type, IReadOnlyReactiveList<IPlantMeasureViewModel> series)
        {
            base.SetSeries(type, series);
            this.LineBrush = new SolidColorBrush(MeasurementTypeHelper.Options[type].SeriesColor.ToColor());
        }

        private Brush _LineBrush;
        public Brush LineBrush
        {
            get
            {
                return _LineBrush;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _LineBrush, value);
            }
        }

        private GenericDataPointBinding<IPlantMeasureViewModel, DateTime> _CategoryBinding;
        public GenericDataPointBinding<IPlantMeasureViewModel, DateTime> CategoryBinding
        {
            get
            {
                return _CategoryBinding;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _CategoryBinding, value);
            }
        }

        private GenericDataPointBinding<IPlantMeasureViewModel, double> _ValueBinding;
        public GenericDataPointBinding<IPlantMeasureViewModel, double> ValueBinding
        {
            get
            {
                return _ValueBinding;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _ValueBinding, value);
            }
        }








    }




}
