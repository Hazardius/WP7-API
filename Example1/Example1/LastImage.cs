using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;
using System.IO;
using System.Collections.ObjectModel;

namespace Example1
{
    public class LastImage : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private BitmapImage _TileImage;
        public BitmapImage TileImage
        {
            get 
            {  
                using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())  
                {
                    if (isoStore.FileExists(ImageName))  
                    {  
                        BitmapImage bitMapImage = new BitmapImage();  
                        using (IsolatedStorageFileStream fileStream = isoStore.OpenFile(@ImageName, FileMode.Open, FileAccess.Read))  
                        {  
                            bitMapImage.SetSource(fileStream);  
                        }  
                        return bitMapImage;  
                    }  
                    else 
                        return null;
                }  
            }
            set
            {
                SaveTile();
                NotifyPropertyChanged("TileImage");
            }
        }

        public static void SortByDate(ObservableCollection<LastImage> list)
        {
            //Bubble sort.
            for (int time = list.Count - 1; time > 0; time--)
            {
                for (int bubble = 0; bubble < time; bubble++)
                {
                    if (list[bubble].PubTime.CompareTo(list[bubble + 1].PubTime) > 0)
                    {
                        LastImage temp = list[bubble];
                        int tempW = list[bubble]._Picture_Width;
                        int tempH = list[bubble]._Picture_Height;
                        WriteableBitmap tempPicture = 
                        list[bubble] = list[bubble + 1];
                        list[bubble].Number = bubble + 1;
                        list[bubble]._Picture_Height = list[bubble + 1]._Picture_Height;
                        list[bubble]._Picture_Width = list[bubble + 1]._Picture_Width;
                        list[bubble].Picture = list[bubble + 1].Picture;
                        list[bubble+1] = temp;
                        list[bubble+1].Number = bubble;
                        list[bubble+1]._Picture_Height = tempH;
                        list[bubble+1]._Picture_Width = tempW;
                        list[bubble+1].Picture = tempPicture;
                    }
                }
            }
        }

        private DateTime _PubTime;
        public DateTime PubTime
        {
            get
            {
                return _PubTime;
            }
            set
            {
                _PubTime = value;
            }
        }

        private void LoadPicture()
        {
            string BigName = _ImageName + ".big";
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isoStore.FileExists(BigName))
                {
                    if (_Picture_Width != -1)
                        _Picture = new WriteableBitmap(_Picture_Width, _Picture_Height);
                    else
                        _Picture = new WriteableBitmap(512, 512);
                    _Picture.Clear(Colors.Red);
                    using (IsolatedStorageFileStream fileStream = isoStore.OpenFile(@BigName, FileMode.Open, FileAccess.Read))
                    {
                        _Picture.LoadJpeg(fileStream);
                    }
                }
                else
                    _Picture = null;
            }  
        }

        private int _Number = 0;
        public int Number
        {
            set
            {
                _Number = value;
                ImageName = _Number + ".jpg";
                NotifyPropertyChanged("Number");
                if ((_Number != 0) && (_Picture != null))
                {
                    TileImage = new BitmapImage();
                }
            }
        }

        private string _ImageName = "Shared/ShellContent/0.jpg";
        public string ImageName
        {
            get
            {
                if (_Number != 0)
                {
                    return _ImageName;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                _ImageName = "Shared/ShellContent/" + value;
                NotifyPropertyChanged("ImageName");
            }
        }

        private int _Picture_Width = -1;
        private int _Picture_Height = -1;
        private WriteableBitmap _Picture = null;
        public WriteableBitmap Picture
        {
            get
            {
                LoadPicture();
                return _Picture;
            }
            set
            {
                _Picture = value;
                SavePicture();
                NotifyPropertyChanged("Picture");
                if ((_Number != 0) && (_Picture != null))
                {
                    TileImage = new BitmapImage();
                }
            }
        }

        private WriteableBitmap _Avatar = new WriteableBitmap(30, 30);
        public WriteableBitmap Avatar
        {
            get
            {
                return _Avatar;
            }
            set
            {
                _Avatar = value;
                NotifyPropertyChanged("Avatar");
                if ((_Number != 0) && (_Picture != null))
                {
                    TileImage = new BitmapImage();
                }
            }
        }

        private string _Subtitle = "Unknown - Unknown";
        public string Subtitle
        {
            get
            {
                return _Subtitle;
            }
            set
            {
                _Subtitle = value;
                NotifyPropertyChanged("Subtitle");
                if ((_Number != 0)&&(_Picture != null))
                {
                    TileImage = new BitmapImage();
                }
            }
        }

        private void SaveTile()
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isoStore.FileExists(ImageName))
                    isoStore.DeleteFile(ImageName);
                using (IsolatedStorageFileStream isoStream = isoStore.CreateFile(@ImageName))
                {
                    Extensions.SaveJpeg(ImageProcessing.GetImageFromLastImage(this), isoStream, 173, 173, 0, 100);
                }
            }
        }

        private void SavePicture()
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                string BigName = _ImageName + ".big";
                if (isoStore.FileExists(BigName))
                    isoStore.DeleteFile(BigName);
                using (IsolatedStorageFileStream isoStream = isoStore.CreateFile(BigName))
                {
                    _Picture_Width = _Picture.PixelWidth;
                    _Picture_Height = _Picture.PixelHeight;
                    Extensions.SaveJpeg(_Picture, isoStream, _Picture_Width, _Picture_Height, 0, 100);
                }
            }
        }

        // Create the OnPropertyChanged method to raise the event
        protected void NotifyPropertyChanged(String name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
