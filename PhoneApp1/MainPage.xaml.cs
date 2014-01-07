using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;




using Microsoft.Phone.Controls;

namespace PhoneApp1
{

    public class Data
    {
        public string Name
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public string IconUri
        {
            get;
            set;
        }

        public string Type
        {
            get;
            set;
        }
    }

    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            List<Data> list = new List<Data>();
            Data item0 = new Data() { Name = "Tomato", IconUri = "Images/Tomato.png", Type = "Healthy" };
            Data item1 = new Data() { Name = "Beer", IconUri = "Images/Beer.png", Type = "NotDetermined" };
            Data item2 = new Data() { Name = "Fries", IconUri = "Images/fries.png", Type = "Unhealthy" };
            Data item3 = new Data() { Name = "Sandwich", IconUri = "Images/Hamburger.png", Type = "Unhealthy" };
            Data item4 = new Data() { Name = "Ice-cream", IconUri = "Images/icecream.png", Type = "Healthy" };
            Data item5 = new Data() { Name = "Pizza", IconUri = "Images/Pizza.png", Type = "Unhealthy" };
            Data item6 = new Data() { Name = "Pepper", IconUri = "Images/Pepper.png", Type = "Healthy" };
            list.Add(item0);
            list.Add(item1);
            list.Add(item2);
            list.Add(item3);
            list.Add(item4);
            list.Add(item5);
            list.Add(item6);

            this.listBox.ItemsSource = list;

        }

    }
}