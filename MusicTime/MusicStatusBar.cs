using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicTime
{
    class MusicStatusBar
    {
        private  IVsStatusbar statusbar;
        private uint cookie;

        public MusicStatusBar(IVsStatusbar statusbar)
        {
            this.statusbar = statusbar;
        }

        public void SetStatus(bool status)
        {
            bool connected = status ? true : false;

          
            if(SoftwareCoUtil.isWindows())
            {

                if (connected)
                {
                    statusbar.SetText("Connected");
                }
                else
                {
                    statusbar.SetText("Connect Spotify");
                }

            }
           
        }
        public void SetTrackName(string currentTrack)
        {

            statusbar.SetText(currentTrack);
        }

        //private Image DrawText(String text, Font font, Color textColor, Color backColor)
        //{
        //    //first, create a dummy bitmap just to get a graphics object
        //    Image img = new Bitmap(10, 1);
        //    Graphics drawing = Graphics.FromImage(img);

        //    //measure the string to see how big the image needs to be
        //    SizeF textSize = drawing.MeasureString(text, font);

        //    //free up the dummy image and old graphics object
        //    img.Dispose();
        //    drawing.Dispose();

        //    //create a new image of the right size
        //    img = new Bitmap((int)textSize.Width, (int)textSize.Height);

        //    drawing = Graphics.FromImage(img);

        //    //paint the background
        //    drawing.Clear(backColor);

        //    //create a brush for the text
        //    Brush textBrush = new SolidBrush(textColor);
        //    drawing.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
        //    drawing.DrawString(text, font, textBrush, 0, 0);

        //    drawing.Save();

        //    textBrush.Dispose();
        //    drawing.Dispose();

        //    return img;

        //}
    }
}
