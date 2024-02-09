using MyBlend.Models.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace MyBlend.Graphics
{
    public class Renderer
    {
        Image image { get; set; }
        public int RenderColor { get; set; }
        public int dpiX { get; set; } = 96;
        public int dpiY { get; set; } = 96;

        public Renderer(Image image)
        {
            this.image = image;
            RenderColor = (255 << 16) | (255 << 8) | (255);
        }

        public void DrawEntityMesh(Matrix4x4 worldModel, Entity entity, float width, float height)
        {
            var wBitmap = new WriteableBitmap((int)width, (int)height, dpiX, dpiY, PixelFormats.Bgr32, null);
            image.Source = wBitmap;

            wBitmap.Lock();

            var poligons = entity.Poligons;
            var positions = entity.GetPositionsInWorldModel(worldModel);
            var bmpInfo = new WritableBitmapInfo(wBitmap.BackBuffer, wBitmap.BackBufferStride, wBitmap.Format.BitsPerPixel);
            Parallel.ForEach(poligons, (poligon) =>
            {
                for (int i = 0; i < poligon.Length - 1; i++)
                {
                    int x1 = (int)(width / 2 + positions[poligon[i].vIndex].X);
                    x1 = (int)Math.Max(0, Math.Min(x1, width - 1));
                    int y1 = (int)(height / 2 + positions[poligon[i].vIndex].Y);
                    y1 = (int)Math.Max(0, Math.Min(y1, height - 1));

                    for (int j = i + 1; j < poligon.Length; j++)
                    {
                        int x2 = (int)(width / 2 + positions[poligon[j].vIndex].X);
                        x2 = (int)Math.Max(0, Math.Min(x2, width - 1));
                        int y2 = (int)(height / 2 + positions[poligon[j].vIndex].Y);
                        y2 = (int)Math.Max(0, Math.Min(y2, height - 1));

                        DrawLine(bmpInfo, x1, y1, x2, y2);
                    }
                }

            });

            wBitmap.AddDirtyRect(new Int32Rect(0, 0, (int)width, (int)height));
            wBitmap.Unlock();
        }

        private record WritableBitmapInfo(nint BackBuffer, int BackBufferStride, int FormatBitsPerPixel);
        private void DrawLine(WritableBitmapInfo bmp, int x1, int y1, int x2, int y2)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;
            var steps = Math.Max(Math.Abs(dx), Math.Abs(dy));
            if (steps == 0)
                return;

            dx = dx / steps;
            dy = dy / steps;
            try
            {
                unsafe
                {
                    var backBuffer = bmp.BackBuffer;
                    int bmpStride = bmp.BackBufferStride;
                    int pixelSize = bmp.FormatBitsPerPixel / 8;
                    var startOfBuffer = backBuffer;
                    double row = y1, column = x1;

                    for (int i = 0; i < steps; i++)
                    {
                        backBuffer += (int)row * bmpStride;
                        backBuffer += (int)column * pixelSize;

                        (*(int*)backBuffer) = RenderColor;

                        column += dx;
                        row += dy;
                        backBuffer = startOfBuffer;
                    }
                }
            }
            finally { }
        }

        private void DrawPixel(WriteableBitmap bmp, int row, int column)
        {
            int pixelSize = bmp.Format.BitsPerPixel / 8;
            try
            {
                bmp.Lock();
                unsafe
                {
                    var backBuffer = bmp.BackBuffer;
                    backBuffer += row * bmp.BackBufferStride;
                    backBuffer += column * pixelSize;

                    (*(int*)backBuffer) = RenderColor;
                }
            }
            finally
            {
                bmp.Unlock();
            }
        }
    }
}
