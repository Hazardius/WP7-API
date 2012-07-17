// Authors:
// Hazardius (2012-*) hazardiusam at gmail dot com
//
// Distributed under the terms of the license:
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along
// with this program; if not, write to the Free Software Foundation, Inc.,
// 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
// http://www.gnu.org/copyleft/gpl.html

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
                var text = li.Subtitle;
                var merge = new WriteableBitmap(173, 173);
                merge.Clear(Colors.Blue);

                int smallSizeW, smallSizeH;
                /// TODO: Find way to cut out a part of image.
                if (image.PixelHeight * 173 > image.PixelWidth * 115)
                {
                    smallSizeW = (image.PixelWidth * 115) / 173;
                    smallSizeH = (image.PixelHeight - smallSizeW) / 2;
                    merge.Blit(new Rect(0, 0, 173, 115),
                        image, new Rect(0, smallSizeH, image.PixelWidth, smallSizeW));
                }
                else
                {
                    smallSizeH = (image.PixelHeight * 173) / 115;
                    smallSizeW = (image.PixelWidth - smallSizeH) / 2;
                    merge.Blit(new Rect(0, 0, 173, 115),
                        image, new Rect(smallSizeW, 0, smallSizeH, image.PixelHeight));
                }

                var blackRect = new WriteableBitmap(1, 1);
                blackRect.Clear(Colors.Blue);
                merge.Blit(new Rect(58, 115, 173, 58), blackRect, new Rect(0, 0, 1, 1));

                merge.Blit(new Rect(0, 115, 58, 58), avatar, new Rect(0, 0, avatar
                    .PixelWidth, avatar.PixelHeight));

                int middle = text.IndexOf('\n');
                var text1Image = SetupRenderedTextBitmap(text.Substring(0,middle), 29.0);
                var text2Image = SetupRenderedTextBitmap(text.Substring(middle+1), 29.0);

                smallSizeW = text1Image.PixelWidth;
                smallSizeH = text1Image.PixelHeight;
                if (text1Image.PixelWidth > 115)
                {
                    smallSizeW = 115;
                    smallSizeH = (115 * text1Image.PixelHeight) / text1Image.PixelWidth;
                }
                if (smallSizeH > 29)
                {
                    smallSizeW = (29 * smallSizeW) / smallSizeH;
                    smallSizeH = 29;
                }
                merge.Blit(new Rect(((115 - smallSizeW) / 2)+58, 114, smallSizeW,
                    smallSizeH), text1Image, new Rect(0, 0, text1Image.PixelWidth,
                    text1Image.PixelHeight));

                smallSizeW = text2Image.PixelWidth;
                smallSizeH = text2Image.PixelHeight;
                if (text2Image.PixelWidth > 115)
                {
                    smallSizeW = 115;
                    smallSizeH = (115 * text2Image.PixelHeight) / text2Image.PixelWidth;
                }
                if (smallSizeH > 29)
                {
                    smallSizeW = (29 * smallSizeW) / smallSizeH;
                    smallSizeH = 29;
                }
                merge.Blit(new Rect(((115 - smallSizeW) / 2) + 58, 143, smallSizeW,
                    smallSizeH), text2Image, new Rect(0, 0, text2Image.PixelWidth,
                    text2Image.PixelHeight));

                return merge;
            }
            catch (Exception e)
            {
                e.ToString();
                /// TODO: Think about doing something with an Exception here.
                return null;
            }
        }

        /// <summary>Text rendering method found on:
        /// http://www.smartypantscoding.com/content/rendering-text-writeablebitmap-silverlight-3s-bitmap-api
        /// Author: Roger "SmartyP" Peters</summary>
        /// <param name="text">Text you need to transform into bitmap.</param>
        /// <returns>Bitmap version of text.</returns>
        private static WriteableBitmap SetupRenderedTextBitmap(string text,
            double fontSize)
        {
            // setup the textblock we will render to a bitmap
            TextBlock txt1 = new TextBlock();
            txt1.Text = text;
            // set the font size before using the Actual Width / Height
            txt1.FontSize = fontSize;
            
            // create our first bitmap we will render to
            WriteableBitmap bitmap = new WriteableBitmap((int)txt1.ActualWidth, 
                (int)txt1.ActualHeight);
            
            // put a black textblock under the white one to create a simple dropshadow
            txt1.Foreground = new SolidColorBrush(Colors.Black);
            bitmap.Render(txt1, new TranslateTransform() { X = -2, Y = -2 });
            
            txt1.Foreground = new SolidColorBrush(Colors.White);
            bitmap.Render(txt1, new TranslateTransform());
            
            // invalidate the bitmap so that it is rendered
            bitmap.Invalidate();
            
            return bitmap;
        }
    }
}
