using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
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
using Microsoft.Win32;
using System.IO;

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


            width = (float)Application.Current.MainWindow.Width;
            height = (float)Application.Current.MainWindow.Height;

            var eye = new Vector3(0, 20, 35);
            var target = new Vector3(0, 0, 0);
            var up = new Vector3(0, 1, 0);
            screen = new Screen(img, width, height);
            camera = new Camera(DegToRad(60), width / height, 0.1f, 10f, eye, target, up);
            screen.Camera = camera;

            var lights = new List<Light>()
            {
                new Light(new Vector3(5, 10, 20), new RgbColor(255,0,255)),
                //new Light(new Vector3(0, 1000, 50), new RgbColor(255,0,255)),
                //new Light(new Vector3(0,0, 100), new RgbColor(122,122,122)),
                //new Light(new Vector3(0, -1000, -1000), new RgbColor(255,255,0)),
                //new Light(new Vector3(1000, 1000, 1000), new RgbColor(255,255,255)),
                //new Light(new Vector3(0, 10, 10), 255)
            };

            renderer = new Renderer(screen, lights);
            renderer.UpdateShader(new PhongShading(screen));
            renderMethod = renderer.RasterizeEntity;

            UpdateWorldModel(Matrix4x4.Identity);
            renderMethod.Invoke(WorldModel, entity);

            KeyDown += RerenderScreen;
            MouseMove += RerenderScreen;
            MouseWheel += RerenderScreen;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.D1:
                    renderer.RenderStyle = Graphics.RendererEnums.RenderStyle.Mesh;
                    break;
                case Key.D2:
                    renderer.RenderStyle = Graphics.RendererEnums.RenderStyle.Old;
                    break;
                case Key.D3:
                    renderer.TextureStyle = Graphics.RendererEnums.TextureStyle.None;
                    renderer.LightStyle = Graphics.RendererEnums.LightStyle.BlinnPhong;
                    renderer.RenderStyle = Graphics.RendererEnums.RenderStyle.Basic;
                    break;
                case Key.D4:
                    renderer.TextureStyle = Graphics.RendererEnums.TextureStyle.Basic;
                    renderer.LightStyle = Graphics.RendererEnums.LightStyle.Phong;
                    renderer.RenderStyle = Graphics.RendererEnums.RenderStyle.Basic;
                    break;
                case Key.D5:
                    renderer.TextureStyle = Graphics.RendererEnums.TextureStyle.Basic;
                    renderer.LightStyle = Graphics.RendererEnums.LightStyle.BlinnPhong;
                    renderer.RenderStyle = Graphics.RendererEnums.RenderStyle.Basic;
                    break;
                case Key.D6:
                    renderer.TextureStyle = Graphics.RendererEnums.TextureStyle.Bilinear;
                    renderer.LightStyle = Graphics.RendererEnums.LightStyle.BlinnPhong;
                    renderer.RenderStyle = Graphics.RendererEnums.RenderStyle.Basic;
                    break;
                case Key.D7:
                    renderer.TextureStyle = Graphics.RendererEnums.TextureStyle.Basic;
                    renderer.LightStyle = Graphics.RendererEnums.LightStyle.CelShading;
                    renderer.RenderStyle = Graphics.RendererEnums.RenderStyle.Basic;
                    break;
                case Key.O:
                    if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                    {
                        var previousContent = placeholder.Content;
                        placeholder.Content = "Loading...";
                        var openFileDialog = new OpenFileDialog();
                        openFileDialog.Filter = "OBJ Files (*.obj)|*.obj";
                        var result = openFileDialog.ShowDialog();
                        if (result == true)
                        {
                            var path = openFileDialog.FileName;
                            parser.Parse(path);
                            placeholder.Visibility = Visibility.Hidden;
                            renderMethod.Invoke(WorldModel, entity);
                        }
                        else {
                            placeholder.Content = previousContent;
                        }
                    }
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
            else if (e.RightButton == MouseButtonState.Pressed)
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

        public Matrix4x4 GetViewerSpace() =>
             Matrix4x4.CreateScale(new Vector3(scale, scale, scale)) *
                         camera.GetLookAtMatrix();


    }
}