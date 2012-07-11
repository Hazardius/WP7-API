using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO.IsolatedStorage;
using System.IO;
using System.Threading;

namespace Example1
{
    static public class ImageProcessing
    {
        static public WriteableBitmap GetImageFromLastImage(LastImage li)
        {
            try
            {
                var image = li.Picture;
                var avatar = li.Avatar;
                var merge = new WriteableBitmap(178, 178);
                merge.Clear(Colors.White);
                merge.Blit(new Rect(0, 0, 178, 148), image, new Rect(0, 0, image.PixelWidth, image.PixelHeight));
                merge.Blit(new Rect(0, 148, 30, 30), avatar, new Rect(0, 0, avatar.PixelWidth, avatar.PixelHeight));
                return merge;
            }
            catch
            {
                /// TODO: Think about doing something with an Exception here.
                return null;
            }
        }

        static public void saveTopImagesAsTiles(Grid target, string name)
        {
            try
            {
                IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();
                System.Windows.Media.Imaging.WriteableBitmap wb = new System.Windows.Media.Imaging.WriteableBitmap(target, null);
                isf.DeleteFile(name);
                using (IsolatedStorageFileStream rawStream = isf.CreateFile(name))
                {
                    wb.SaveJpeg(rawStream, wb.PixelWidth, wb.PixelHeight, 0, 100);
                }
            }
            catch
            {
                Thread.Sleep(2000);
            }
        }
    }
}
