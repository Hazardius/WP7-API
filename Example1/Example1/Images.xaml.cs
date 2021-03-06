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
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using RestSharp;
using System.Windows.Media.Imaging;

namespace Example1
{
    public partial class Images : PhoneApplicationPage
    {
        static public bool isLogged = false;

        public Images()
        {
            InitializeComponent();

            DataContext = App.ViewModel;
            this.Loaded += new RoutedEventHandler(Images_Loaded);
        }

        // Load data for the ViewModel Items
        private void Images_Loaded(object sender, RoutedEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((Pivot)sender).SelectedIndex == 1)
            {
                this.ApplicationBar.IsVisible = true;

                ShellTile PrimaryTile = ShellTile.ActiveTiles.First();

                if (PrimaryTile != null)
                {
                    StandardTileData tile = new StandardTileData();

                    tile.BackgroundImage = new Uri("/Images/1.jpg", UriKind.Relative);
                    //tile.Title = "Test";
                    tile.BackBackgroundImage = new Uri("/Images/2.jpg", UriKind.Relative);
                    //tile.BackTitle = "App";
                    //tile.BackContent = "Content";

                    PrimaryTile.Update(tile);
                }
            }
            else
            {
                this.ApplicationBar.IsVisible = false;
            }
        }

        private void test2(WriteableBitmap downloadedImage)
        {
            backImageTEST.Source = downloadedImage;
        }

        private void test(List<DotNetMetroWikiaAPI.Api.FileInfo> lista)
        {
            searchWikiBox.Text = lista.ElementAt(0).ToString();
            /// TODO: Need to have working imagecrop!!!
            lista.ElementAt(0).SetAddressOfFile("http://images1.wikia.nocookie.net/__cb20120703141918/glee/images/a/a2/Glee_Season_3.jpg");
            DotNetMetroWikiaAPI.Api.DownloadImage(test2, lista.ElementAt(0));
        }

        private void ListBoxItem_Tap(object sender, GestureEventArgs e)
        {
            DotNetMetroWikiaAPI.Api.GetNewFilesListFromWiki(test, "glee", 10);
        }

        private void Grid_DoubleTap(object sender, GestureEventArgs e)
        {
            ImageProcessing.saveTopImagesAsTiles((Grid)sender, "/Images/1.jpg");
        }

        private void LogOut(IRestResponse e, string sendData)
        {
            isLogged = false;
        }

        private void PhoneApplicationPage_LostFocus(object sender, RoutedEventArgs e)
        {
            DotNetMetroWikiaAPI.Api.LogOut(new Action<IRestResponse, string>(LogOut));
        }

        private void PhoneApplicationPage_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!isLogged)
            {
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            }
        }
    }
}