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

        private Key pressedBefore = Key.LineFeed;
        private Dictionary<Key, Action<Renderer>> lightClickHandlers = new()
        {
            [Key.D0] = (renderer) => renderer.LightStyle = Graphics.RendererEnums.LightStyle.None,
            [Key.D1] = (renderer) => renderer.LightStyle = Graphics.RendererEnums.LightStyle.Flat,
            [Key.D2] = (renderer) => renderer.LightStyle = Graphics.RendererEnums.LightStyle.Phong,
            [Key.D3] = (renderer) => renderer.LightStyle = Graphics.RendererEnums.LightStyle.BlinnPhong,
            [Key.D4] = (renderer) => renderer.LightStyle = Graphics.RendererEnums.LightStyle.CelShading,
        };
        private Dictionary<Key, Action<Renderer>> textureClickHandlers = new()
        {
            [Key.D0] = (renderer) => renderer.TextureStyle = Graphics.RendererEnums.TextureStyle.None,
            [Key.D1] = (renderer) => renderer.TextureStyle = Graphics.RendererEnums.TextureStyle.Basic,
            [Key.D2] = (renderer) => renderer.TextureStyle = Graphics.RendererEnums.TextureStyle.Bilinear,
            [Key.N] = (renderer) => renderer.WithNormal = !renderer.WithNormal,
        };

        private void ProcessCompoundKeypress(Key previous, Key key)
        {
            switch(previous)
            {
                case Key.L:
                    lightClickHandlers.TryGetValue(key, out var lightAction);
                    lightAction?.Invoke(renderer);
                    break;
                case Key.T:
                    textureClickHandlers.TryGetValue(key, out var textureAction);
                    textureAction?.Invoke(renderer);
                    break;
            }
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.M:
                    renderer.RenderStyle = Graphics.RendererEnums.RenderStyle.Mesh;
                    break;
                case Key.O:
                    if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                        OpenFile();
                    else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                        renderer.RenderStyle = Graphics.RendererEnums.RenderStyle.Old;
                    else
                        renderer.WithOutline = !renderer.WithOutline;
                    break;
                case Key.Escape:
                case Key.T:
                case Key.L:
                    renderer.RenderStyle = Graphics.RendererEnums.RenderStyle.Basic;
                    pressedBefore = e.Key;
                    UpdateHintText(pressedBefore);
                    break;
                default:
                    //renderer.RenderStyle = Graphics.RendererEnums.RenderStyle.Basic;
                    ProcessCompoundKeypress(pressedBefore, e.Key);
                    //pressedBefore = e.Key;
                    break;
            }
        }

        private void UpdateHintText(Key key)
        {
            switch(key)
            {
                case Key.L:
                    var lightModeText = new StringBuilder();
                    lightModeText.AppendLine("Light modes:");
                    lightModeText.AppendLine("0 - none");
                    lightModeText.AppendLine("1 - flat");
                    lightModeText.AppendLine("2 - phong");
                    lightModeText.AppendLine("3 - blinn-phong");
                    lightModeText.AppendLine("4 - cel");
                    tbHint.Text = lightModeText.ToString();
                    break;
                case Key.T:
                    var textureModeText = new StringBuilder();
                    textureModeText.AppendLine("Texture modes:");
                    textureModeText.AppendLine("0 - none");
                    textureModeText.AppendLine("1 - basic");
                    textureModeText.AppendLine("2 - blinear filter");
                    textureModeText.AppendLine("n - toggle normal map");
                    tbHint.Text = textureModeText.ToString();
                    break;
                default:
                    tbHint.Text = "";
                    break;
            }
        }

        private void OpenFile()
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
            else
            {
                placeholder.Content = previousContent;
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