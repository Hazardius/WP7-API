using System;
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
using System.IO;
using System.Text;
using System.Collections.ObjectModel;
using System.Threading;
using Microsoft.Phone;

namespace Example1
{
    public partial class Images : PhoneApplicationPage
    {
        /// <summary>Is user logged into the wikia.com .</summary>
        static public bool isLogged = false;
        private List<DotNetMetroWikiaAPI.Api.FileInfo> ListOfFiles = new
            List<DotNetMetroWikiaAPI.Api.FileInfo>();
        /// <summary>Consist names and prefixes of 10 remembered wikis.</summary>
        public ObservableCollection<PairOfNames> TenWikias { get; set; }
        public ObservableCollection<LastImage> LastImages { get; set; }
        private string prefix = "";
        private int tempCounter = 0;

        public Images()
        {
            InitializeComponent();

            DataContext = App.ViewModel;
            this.Loaded += new RoutedEventHandler(Images_Loaded);
        }

        private void GetLastImagesFromFiles()
        {
            LastImages = new ObservableCollection<LastImage>();

            Pictures.ItemsSource = LastImages;

            LastImages.Add(new LastImage());
            LastImages.Add(new LastImage());
            LastImages.Add(new LastImage());
            LastImages.Add(new LastImage());
            LastImages.Add(new LastImage());
            LastImages.Add(new LastImage());
            LastImages.Add(new LastImage());
            LastImages.Add(new LastImage());
            LastImages.Add(new LastImage());
            LastImages.Add(new LastImage());
            for (int i = 0; i < 10; i++)
            {
                IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();
                /// TODO: TUTAJ KURDE
                if (isf.FileExists("Shared/ShellContent/" + (i + 1) + ".jpg"))
                {
                    LastImages[i].Number = i + 1;
                    WriteableBitmap downloadedImage = LastImages[i].Picture;
                    LastImages[i].Picture = downloadedImage;
                    LastImages[i].Subtitle = "Temp " + (i + 1);
                }
            }
        }

        /// <summary>Loads dictionary of Ten Wikias into the application. If it's first
        /// time run - creates file with default ones.</summary>
        private void GetTenWikisFromFile()
        {
            TenWikias = new ObservableCollection<PairOfNames>();

            Wikis.ItemsSource = TenWikias;

            IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();

            string path = "Resources" + System.IO.Path.DirectorySeparatorChar
                + "TenWikis.dat";

            if (isf.FileExists(path))
            {
                try
                {
                    using (IsolatedStorageFileStream rawStream = isf.OpenFile(path,
                        System.IO.FileMode.Open))
                    {
                        StreamReader reader = new StreamReader(rawStream, Encoding.UTF8);

                        for (int i = 0; ((i<10)&&(!reader.EndOfStream)); i++)
                        {
                            string name = reader.ReadLine();
                            string prefix = reader.ReadLine();
                            string stars = reader.ReadLine();
                            if (stars.Equals("**********"))
                            {
                                TenWikias.Add(new PairOfNames());
                                TenWikias[i].NameOfWiki = name;
                                TenWikias[i].PrefixOfWiki = prefix;
                            }
                        }

                        reader.Close();
                    }
                }
                catch (Exception e)
                {
                    searchWikiBox.Text = e.Message;
                    // TODO: Do something with that Exception.
                }
            }
            else
            {
                TenWikias.Add(new PairOfNames());
                TenWikias.Add(new PairOfNames());
                TenWikias.Add(new PairOfNames());
                TenWikias.Add(new PairOfNames());
                TenWikias.Add(new PairOfNames());
                TenWikias.Add(new PairOfNames());
                TenWikias.Add(new PairOfNames());
                TenWikias.Add(new PairOfNames());
                TenWikias.Add(new PairOfNames());
                TenWikias.Add(new PairOfNames());
                TenWikias[0].NameOfWiki = "glee wiki";
                TenWikias[0].PrefixOfWiki = "glee";
                TenWikias[1].NameOfWiki = "BATTLEFIELD WIKI";
                TenWikias[1].PrefixOfWiki = "battlefield";
                TenWikias[2].NameOfWiki = "Wiedźmińska Wiki";
                TenWikias[2].PrefixOfWiki = "wiedzmin";
                TenWikias[3].NameOfWiki = "Logopedia";
                TenWikias[3].PrefixOfWiki = "logos";
                TenWikias[4].NameOfWiki = "Academic Jobs";
                TenWikias[4].PrefixOfWiki = "academicjobs";
                TenWikias[5].NameOfWiki = "Borderlands Wiki";
                TenWikias[5].PrefixOfWiki = "borderlands";
                TenWikias[6].NameOfWiki = "Solar Cookers World";
                TenWikias[6].PrefixOfWiki = "solarcooking";
                TenWikias[7].NameOfWiki = "Snow White and the Huntsman Wiki";
                TenWikias[7].PrefixOfWiki = "snowwhiteandthehuntsman";
                TenWikias[8].NameOfWiki = "The One Wiki to Rule Them All";
                TenWikias[8].PrefixOfWiki = "lotr";
                TenWikias[9].NameOfWiki = "CALL OF DUTY";
                TenWikias[9].PrefixOfWiki = "callofduty";

                using (isf)
                {
                    if (!isf.DirectoryExists("Resources"))
                    {
                        isf.CreateDirectory("Resources");
                    }

                    using (IsolatedStorageFileStream rawStream = isf.CreateFile(path))
                    {
                        StreamWriter writer = new StreamWriter(rawStream);

                        foreach (PairOfNames pair in TenWikias)
                        {
                            writer.WriteLine(pair.NameOfWiki, Encoding.UTF8);
                            writer.WriteLine(pair.PrefixOfWiki, Encoding.UTF8);
                            writer.WriteLine("**********", Encoding.UTF8);
                        }

                        writer.Close();
                    }
                }
            }
        }

        // Load data for the ViewModel Items
        private void Images_Loaded(object sender, RoutedEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }

            GetTenWikisFromFile();
            GetLastImagesFromFiles();
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

                    if (IsolatedStorageFile.GetUserStoreForApplication().FileExists("Shared/ShellContent/1.jpg"))
                        tile.BackgroundImage = new Uri("isostore:/Shared/ShellContent/1.jpg");
                    tile.Title = "";
                    tile.BackBackgroundImage = new Uri("isostore:/Shared/ShellContent/2.jpg");
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

        private void SaveImage(WriteableBitmap downloadedImage, DotNetMetroWikiaAPI.Api.FileInfo info)
        {
            LastImage li = new LastImage();
            LastImages[tempCounter] = li;
            li.PubTime = info.GetTime();
            li.Number = tempCounter + 1;
            li.Picture = downloadedImage;
            li.Subtitle = info.GetUsername() + " " + DateProcessing.HowLongAgo(info.GetTime());
            tempCounter++;
            if (tempCounter == ListOfFiles.Count)
            {
                LastImage.SortByDate(LastImages);
                Wikis.IsEnabled = true;
            }
        }

        private async void CheckIsItNow()
        {
            tempCounter++;
            if (tempCounter == ListOfFiles.Count)
            {
                tempCounter = 0;
                foreach (DotNetMetroWikiaAPI.Api.FileInfo fi in ListOfFiles)
                {
                    DotNetMetroWikiaAPI.Api.DownloadImage(SaveImage, fi);
                }
            }
        }

        private async void WholeListDownloaded(List<DotNetMetroWikiaAPI.Api.FileInfo> lista)
        {
            ListOfFiles = lista;
            tempCounter = 0;
            foreach (DotNetMetroWikiaAPI.Api.FileInfo fi in ListOfFiles)
            {
                await DotNetMetroWikiaAPI.Api.GetAddressOfTheFile
                    (new Action(CheckIsItNow), fi, prefix);
            }
        }

        private async void ListBoxItem_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Wikis.IsEnabled = false;
            //LastImages.Clear();
            string name = ((TextBlock)((StackPanel)sender).Children.ElementAt(0)).Text;
            foreach (PairOfNames pon in TenWikias)
            {
                if (pon.NameOfWiki == name)
                {
                    prefix = pon.PrefixOfWiki;
                    break;
                }
            }
            await DotNetMetroWikiaAPI.Api.GetNewFilesListFromWiki(WholeListDownloaded, prefix, 10);
        }

        private void Image_DoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            int index = ((ListBox)sender).SelectedIndex;
            if (index >= 0)
            {
                bigPicture.Source = LastImages[index].Picture;
                TextOfImage.Text = LastImages[index].Subtitle;
            }
        }

        /// <summary>Make application to remember that user is logged out. It's
        /// a callback function.</summary>
        /// <param name="e">Response from the server.</param>
        /// <param name="sendData"></param>
        /// 
        /// TODO: Find out if it can be hidden inside an api and still be useable.
        /// Btw. what is going on here?
        private void LogOut(IRestResponse e, string sendData)
        {
            isLogged = false;
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

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