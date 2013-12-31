using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using Microsoft.Phone.Tasks;
using Growthstories.UI.ViewModel;
using ReactiveUI;
using Growthstories.UI.WindowsPhone.ViewModels;
using System.Reactive.Disposables;
using System.Windows.Input;

namespace Growthstories.UI.WindowsPhone
{

    public class AddPlantViewBase : GSView<IAddEditPlantViewModel>
    {

    }


    public partial class AddPlantView : AddPlantViewBase
    {

        public AddPlantView()
        {
            InitializeComponent();
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