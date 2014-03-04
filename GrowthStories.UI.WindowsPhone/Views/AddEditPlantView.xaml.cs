using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using EventStore.Logging;
using Growthstories.UI.ViewModel;


namespace Growthstories.UI.WindowsPhone
{

    public class AddPlantViewBase : GSView<IAddEditPlantViewModel>
    {

    }


    public partial class AddPlantView : AddPlantViewBase
    {

        private static ILog Logger = LogFactory.BuildLogger(typeof(AddPlantView));


        public AddPlantView()
        {
            InitializeComponent();
            this.TabItems = new List<Control>()
            {
                this.NameTextBox,
                this.SpeciesTextBox
            };
        }


        private void GSTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            SIPHelper.SIPGotVisible(SIPPlaceHolder);
        }

        private void GSTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SIPHelper.SIPGotHidden(SIPPlaceHolder);
        }

        protected override void OnViewModelChanged(IAddEditPlantViewModel vm)
        {


        }

        private async Task ScrollToTopAfterDelay(TimeSpan delay)
        {
            await Task.Delay(delay);
            try
            {
                var scroller = this.ViewGrid.ScrollViewer;
                if (scroller != null)
                {
                    scroller.ScrollToVerticalOffset(0);
                }
            }
            catch { Logger.Warn("could not scroll addeditplantview"); }
        }


        private void ViewGrid_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Schedule_Click(object sender, RoutedEventArgs e)
        {
            var s = sender as FrameworkElement;
            if (s != null)
            {
                this.ViewModel.ScheduleIntervalTappedCommand.Execute(s.DataContext);
            }
        }


        /*
        private void TagBox_IconTapped(object sender, EventArgs e)
        {

            var text = TagBox.Text;

            if (!string.IsNullOrWhiteSpace(text))
            {
                this.ViewModel.AddTag.Execute(text);
                TagBox.Text = null;
                this.Focus();

            }

        }
        
        private void TagBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.TagBox_IconTapped(sender, e);
            }
        }
         
        */

    }


}