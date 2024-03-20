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
using MyBlend.Models.Light;

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
        float scale = 2f;

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
            //parser.Parse(@"C:\Users\alber\Downloads\Telegram Desktop\cube.obj");
            parser.Parse(@"D:\Univer\acg\Shovel Knight\shovel_low.obj");
            //parser.Parse(@"D:\Univer\acg\DoomCombatScene.obj");


            width = (float)Application.Current.MainWindow.Width;
            height = (float)Application.Current.MainWindow.Height;

            var eye = new Vector3(0, 20, 35);
            var target = new Vector3(0,0,0);
            var up = new Vector3(0, 1, 0);
            screen = new Screen(img, width, height);
            camera = new Camera(DegToRad(60), width/height, 0.1f, 10f, eye, target, up);
            screen.Camera = camera;

            var lights = new List<Light>()
            {
                new Light(eye, new RgbColor(255,0,255)),
                //new Light(new Vector3(0, 10, 10), 255)
            };

            renderer = new Renderer(screen, lights);
            renderer.UpdateShader(new PhongShading(screen));
            renderMethod = renderer.RasterizeEntityWithTexture;

            UpdateWorldModel(Matrix4x4.Identity);
            renderer.RasterizeEntityWithTexture(WorldModel, entity);

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
                    renderer.UpdateShader(null);
                    renderMethod = renderer.RasterizeEntity;
                    break;
                case Key.D3:
                    renderer.UpdateShader(new FlatShading());
                    renderMethod = renderer.RasterizeEntity;
                    break;
                case Key.D4:
                    renderer.UpdateShader(new PhongShading(screen));
                    renderMethod = renderer.RasterizeEntity;
                    break;
                case Key.D5:
                    renderer.UpdateShader(new PhongShading(screen));
                    renderMethod = renderer.RasterizeEntityWithTexture;
                    break;
                case Key.N:
                    var list = renderer.Lights.ToList();
                    list.Add(new Light(camera.Eye, 255));
                    renderer.Lights = list;
                    break;

            }
        }
        
        private float DegToRad(float angle)
        {
            return (float)(Math.PI / 180 * angle);
        }

        private void RerenderScreen(object sender, EventArgs e)
        {
            UpdateWorldModel(Matrix4x4.Identity);
            renderMethod?.Invoke(WorldModel, entity);
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            //fps.Text = $"{renderer.GetFrameCount()} fps";
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
                    MoveCamera(0, dy * 0.2f);
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