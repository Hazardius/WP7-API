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
                    /// "Images" + System.IO.Path.DirectorySeparatorChar
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

        private int _Number = 0;
        public int Number
        {
            set
            {
                _Number = value;
                ImageName = _Number + ".jpg";
                NotifyPropertyChanged("Number");
                if (_Number != 0)
                {
                    TileImage = new BitmapImage();
                }
            }
        }

        private string _ImageName = "0.jpg";
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
                _ImageName = value;
                NotifyPropertyChanged("ImageName");
            }
        }

        private WriteableBitmap _Picture = new WriteableBitmap(143, 173);
        public WriteableBitmap Picture
        {
            get
            {
                return _Picture;
            }
            set
            {
                _Picture = value;
                NotifyPropertyChanged("Picture");
                if (_Number != 0)
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
                if (_Number != 0)
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
                if (_Number != 0)
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
                    //ImageProcessing.GetImageFromLastImage(this).SaveJpeg(isoStream, 173, 173, 0, 100);
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
