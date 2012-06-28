﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using DotNetMetroWikiaAPI;

namespace Example1
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        private void successInLogin()
        {
            NavigationService.Navigate(new Uri("/Images.xaml", UriKind.Relative));
            button1.IsEnabled = true;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Site wikia = new Site("http://www.wikia.com", textBox1.Text, passwordBox1.Password, successInLogin);

                button1.IsEnabled = false;
                //NavigationService.Navigate(new Uri("/Images.xaml", UriKind.Relative));
            }
            catch
            {
                //Something to do.
            }
        }
    }
}