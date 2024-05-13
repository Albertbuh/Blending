using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MyBlend.Models.Basic;

namespace MyBlend.Models.Textures
{
    public struct Texture
    {
        public RgbColor[,]? pixelBuffer { get; private set; }

        private int width;
        private int height;

        public int Width => width - 1;
        public int Height => height - 1;
        public Texture(string filename)
        {
            LoadBuffer(filename);
        }

        private void LoadBuffer(string filename)
        {
            var bmp = new Bitmap(filename);
            width = bmp.Width;
            height = bmp.Height;
            pixelBuffer = new RgbColor[height, width];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var pixel = bmp.GetPixel(i, j);
                    pixelBuffer[j, i] = new RgbColor(pixel.R, pixel.G, pixel.B, pixel.A);
                }
            }
        }

        public RgbColor Map(float tu, float tv)
        {
            if (pixelBuffer == null)
                return RgbColor.White;

            int u = Math.Abs((int)(tu * width) % width);
            int v = Math.Abs((int)(tv * height) % height);

            return pixelBuffer[v, u];
        }

        public RgbColor GetPixel(int u, int v)
        {
            if (pixelBuffer == null)
                return RgbColor.White;
            return pixelBuffer[v, u];
        }

    }
}
