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
using System.Reflection.Emit;
using System.Data.Common;
using MyBlend.Models.Basic;
using MyBlend.Models.Display;
using MyBlend.Parser;
using MyBlend.Graphics;
using static System.Formats.Asn1.AsnWriter;
using System.Diagnostics;
using System.Windows.Threading;

namespace MyBlend
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IParser parser;
        private Entity entity;
        private Screen screen;
        private Camera camera;
        private Renderer renderer;
        float width, height;
        float scale = 3f;

        private delegate void RendererMethod(Matrix4x4 m, Entity entity);
        private RendererMethod renderMethod;

        private Matrix4x4 WorldModel = Matrix4x4.Identity;

        private DispatcherTimer timer;
        public MainWindow()
        {
            InitializeComponent();

            entity = new ObjEntity();
            parser = new ObjParser((ObjEntity)entity);
            //parser.Parse(@"D:\Univer\acg\russian-archipelago-frigate-svjatoi-nikolai\source\SM_Ship01A_02_OBJ.obj");
            //parser.Parse(@"C:\Users\alber\Downloads\Telegram Desktop\shrek.obj");
            parser.Parse(@"D:\Univer\acg\Shovel Knight\shovel_low.obj");


            width = (float)Application.Current.MainWindow.Width;
            height = (float)Application.Current.MainWindow.Height;

            var eye = new Vector3(0, 25, 35);
            var target = new Vector3(0,0,0);
            var up = new Vector3(0, 1, 0);
            screen = new Screen(img, width, height);
            camera = new Camera(DegToRad(120), height/width, 0.1f, 10f, eye, target, up);

            renderer = new Renderer(screen);
            renderMethod = renderer.RasterizeEntity;
            UpdateWorldModel(Matrix4x4.Identity);
            renderer.RasterizeEntity(WorldModel, entity);

            KeyDown += RerenderScreen;
            MouseMove += RerenderScreen;
            MouseWheel += RerenderScreen;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            timer.Start();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.D1:
                    renderMethod = renderer.DrawEntityMesh;
                    break;
                case Key.D2:
                    renderMethod = renderer.RasterizeEntity;
                    break;
            }
        }
        
        private float DegToRad(float angle)
        {
            return (float)(Math.PI / 180 * angle);
        }

        private int frameCount = 0;
        private void RerenderScreen(object sender, EventArgs e)
        {
            UpdateWorldModel(Matrix4x4.Identity);
            renderMethod?.Invoke(WorldModel, entity);
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            fps.Text = $"{renderer.GetFrameCount()} fps";
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            float delta = (float)e.Delta / 1000;
            scale *= (1 + delta);
        }

        private Point prevMousePosition = default;
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            var currentMousePosition = e.GetPosition(this);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (prevMousePosition != default)
                {
                    var dy = (float)(currentMousePosition.Y - prevMousePosition.Y) * scale;
                    MoveCamera(0, dy);
                }
            }
            else if(e.RightButton == MouseButtonState.Pressed)
            {
                if (prevMousePosition != default)
                {
                    var dy = (float)(currentMousePosition.Y - prevMousePosition.Y);
                    var dx = (float)(currentMousePosition.X - prevMousePosition.X);
                    RotateCamera(dx, dy);
                }
            }
            prevMousePosition = currentMousePosition;
        }

        private void RotateCamera(float dx, float dy)
        {
            camera.Zeta += (dx * 0.005f);
            camera.Phi -= (dy * 0.005f);
        }

        private void MoveCamera(float dx, float dy)
        {
            camera.UpdateTarget(new Vector3(-dx, dy, 0));
        }

        public void UpdateWorldModel(Matrix4x4 transform)
        {
            WorldModel = Matrix4x4.CreateScale(new Vector3(scale, scale, scale)) *
                         transform *
                         camera.GetLookAtMatrix() *
                         camera.GetPerspectiveMatrix() *
                         screen.GetMatrix();
        }

        
    }
}