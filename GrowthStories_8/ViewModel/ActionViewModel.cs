﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Growthstories.PCL.Helpers;
using Growthstories.WP8.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Growthstories.WP8.ViewModel
{
    public class ActionViewModel : ViewModelBase
    {
        private INavigationService _nav;

        private PlantAction _action;

        public const string ActionPageName = "ActionPage";
        public static Uri ActionPageUri = new Uri(string.Format("/View/{0}.xaml", ActionPageName), UriKind.Relative);

        public ActionViewModel(INavigationService nav)
        {
            this._nav = nav;

        }

        public PlantAction CurrentAction
        {
            get
            {
                return _action;
            }
            set
            {
                Set("CurrentAction", ref _action, value);
            }
        }



        public void SelectedActionChanged(object sender, PropertyChangedEventArgs e)
        {
            PlantViewModel s = sender as PlantViewModel;
            if (sender != null && e.PropertyName == "SelectedAction")
            {
                CurrentAction = s.SelectedAction;
            }
        }

    }
}
