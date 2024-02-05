using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Numerics;

namespace MyBlend
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WriteableBitmap wBitmap;

        public MainWindow()
        {
            InitializeComponent();
            var entity = new ObjEntity();
            var parser = new ObjParser(entity);
            entity = parser.ParseFile(@"D:\Univer\acg\russian-archipelago-frigate-svjatoi-nikolai\source\SM_Ship01A_02_OBJ.obj");

            const int width = 800, height = 600;
            const int FOV = 70;
            const float aspect = width / height;
            const float znear = 0.1f, zfar = 10;

            var eye = new Vector3(0, 0, 1);
            var target = new Vector3(0, 0, 0);
            var up = new Vector3(0, 1, 0);
            var screen = new Screen(width, height);
            var camera = new Camera(eye, target, up);

            var scale = 0.02f;

            var model1 = MatrixTemplates.Scale(new Vector3(scale, scale, scale)) * 
                         MatrixTemplates.RotateZ(-90) *
                         camera.GetMatrix() *
                         MatrixTemplates.Projection(FOV, aspect, znear, zfar) *
                         screen.GetMatrix();


            var dpi = 96;
            var color = new byte[] { 255, 255, 255, 255 };
            wBitmap = new WriteableBitmap(width, height, dpi, dpi, PixelFormats.Bgr32, null);
            img.Source = wBitmap;

            for (int i = 0; i < entity.Positions.Count; i++)
            {
                entity.Positions[i] = MultiplyVectorByMatrix(new Vector4(entity.Positions[i], 1), model1);
                int x = width / 2 + (int)entity.Positions[i].X;
                int y = height / 2 + (int)entity.Positions[i].Y;
                //DrawPixel(wBitmap, x, y);
            }

            for(int i = 0; i < 100; i++)
            {
                for(int j = 0; j < entity.Poligons[i].Count - 1; j++)
                {
                    int x1 = width / 2 + (int)entity.Poligons[i][j].position.Value.X;
                    int y1 = height / 2 + (int)entity.Poligons[i][j].position.Value.Y;
                    for (int k = j + 1; k < entity.Poligons[i].Count; k++)
                    {
                        int x2 = width / 2 + (int)entity.Poligons[i][k].position.Value.X;
                        int y2 = height / 2 + (int)entity.Poligons[i][k].position.Value.Y;
                        DrawLine(wBitmap, x1, y1, x2, y2);
                    }
                }
            }

        }

        private void DrawLine(WriteableBitmap bmp, int x1, int y1, int x2, int y2)
        {
            var dx = x2 - x1;
            var dy = y2 - y1;
            var steps = Math.Max(Math.Abs(dx), Math.Abs(dy));
            if (steps == 0)
                return;

            dx = dx / steps;
            dy = dy / steps;
            try
            {
                bmp.Lock();
                unsafe
                {
                    var backBuffer = bmp.BackBuffer;
                    int bmpStride = bmp.BackBufferStride;
                    int pixelSize = bmp.Format.BitsPerPixel / 8;
                    var startOfBuffer = backBuffer;
                    int row = x1, column = y1;

                    for (int i = 0; i < steps; i++)
                    {
                        backBuffer += row * bmpStride;
                        backBuffer += column * pixelSize;

                        var color_data = (255 << 16) | (255 << 8) | (255);
                        (*(int*)backBuffer) = color_data;

                        row += dx;
                        column += dy;
                        backBuffer = startOfBuffer;
                    }
                }
                var invalidateRect = new Int32Rect(
                    Math.Min(x1, x2), Math.Min(y1, y2),
                    Math.Abs(x2 - x1), Math.Abs(y2 - y1)
                    );
                bmp.AddDirtyRect(invalidateRect);
            }
            finally
            { 
                bmp.Unlock(); 
            }
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

                    var color_data = 255 << 16; //R
                    color_data |= 255 << 8; //G
                    color_data |= 255; //B
                    (*(int*)backBuffer) = color_data;
                }
                bmp.AddDirtyRect(new Int32Rect(column, row, 1, 1));
            }
            finally
            {
                bmp.Unlock();
            }
        }


        private Vector3 MultiplyVectorByMatrix(Vector4 v, Matrix4x4 m)
        {
            return new Vector3(
                v.X * m.M11 + v.Y * m.M21 + v.Z * m.M31 + v.W * m.M41,
                v.X * m.M12 + v.Y * m.M22 + v.Z * m.M32 + v.W * m.M42,
                v.X * m.M13 + v.Y * m.M23 + v.Z * m.M33 + v.W * m.M43
                );
        }

        


    }
}