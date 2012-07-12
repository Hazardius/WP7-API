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
                //var avatar = li.Avatar;
                var text = li.Subtitle;
                var merge = new WriteableBitmap(173, 173);
                merge.Clear(Colors.Red);
                int smallSizeW, smallSizeH;
                if (image.PixelWidth > image.PixelHeight)
                {
                    smallSizeW = 173;
                    smallSizeH = (173 * image.PixelHeight) / image.PixelWidth;
                }
                else
                {
                    smallSizeW = (173 * image.PixelWidth) / image.PixelHeight;
                    smallSizeH = 173;
                }
                merge.Blit(new Rect((173 - smallSizeW)/2, 173 - smallSizeH, smallSizeW, smallSizeH), image, new Rect(0, 0, image.PixelWidth, image.PixelHeight));
                //merge.Blit(new Rect(0, 148, 30, 30), avatar, new Rect(0, 0, avatar.PixelWidth, avatar.PixelHeight));
                return merge;
            }
            catch (Exception e)
            {
                e.ToString();
                /// TODO: Think about doing something with an Exception here.
                return null;
            }
        }
    }
}
