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
using System.Windows.Shapes;

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
            
            void DrawPoligon(Face[] poligon)
            {
                int i = 0;
                int x1, x2, y1, y2;
                for (i = 0; i < poligon.Length - 1; i++)
                {
                    x1 = (int)Math.Max(0, Math.Min(width / 2 + positions[poligon[i].vIndex].X, width - 1));
                    y1 = (int)Math.Max(0, Math.Min(height / 2 + positions[poligon[i].vIndex].Y, height - 1));

                    x2 = (int)Math.Max(0, Math.Min(width / 2 + positions[poligon[i + 1].vIndex].X, width - 1));
                    y2 = (int)Math.Max(0, Math.Min(height / 2 + positions[poligon[i + 1].vIndex].Y, height - 1));

                    DrawLine(bmpInfo, x1, y1, x2, y2);
                }

                x1 = (int)Math.Max(0, Math.Min(width / 2 + positions[poligon[i].vIndex].X, width - 1));
                y1 = (int)Math.Max(0, Math.Min(height / 2 + positions[poligon[i].vIndex].Y, height - 1));

                x2 = (int)Math.Max(0, Math.Min(width / 2 + positions[poligon[0].vIndex].X, width - 1));
                y2 = (int)Math.Max(0, Math.Min(height / 2 + positions[poligon[0].vIndex].Y, height - 1));

                DrawLine(bmpInfo, x1, y1, x2, y2);
            }
            Parallel.ForEach(poligons, (Face[] poligon) => DrawPoligon(poligon));

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
