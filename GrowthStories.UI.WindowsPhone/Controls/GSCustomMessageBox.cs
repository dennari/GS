
namespace Growthstories.UI.WindowsPhone
{


    class GSCustomMessageBox : CustomMessageBox
    {


        public GSCustomMessageBox()
            : base()
        {
            DismissOnBackButton = true;
        }


        public bool DismissOnBackButton { get; set; }


        protected override void OnBackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DismissOnBackButton)
            {
                base.OnBackKeyPress(sender, e);
            }

            // we don't wish to cancel the event, 
            // so back navigation is triggered instead
            // of dismissing the back button
        }


    }






}