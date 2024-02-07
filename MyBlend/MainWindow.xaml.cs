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

namespace MyBlend
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WriteableBitmap wBitmap;
        private IParser parser;
        private Entity entity;
        private Screen screen;
        private Camera camera;
        private Renderer renderer;
        float width, height;

        private Matrix4x4 WorldModel = Matrix4x4.Identity;

        public MainWindow()
        {
            InitializeComponent();
            entity = new ObjEntity();
            parser = new ObjParser((ObjEntity)entity);
            parser.Parse(@"D:\Univer\acg\russian-archipelago-frigate-svjatoi-nikolai\source\SM_Ship01A_02_OBJ.obj");
            //parser.Parse(@"C:\Users\alber\Downloads\Telegram Desktop\shuttle.obj");

            //parser.Parse(@"C:\Users\alber\Downloads\Telegram Desktop\teapot.obj");

            width = (float)Application.Current.MainWindow.Width;
            height = (float)Application.Current.MainWindow.Height;

            var eye = new Vector3(0, 0, 10);
            var target = new Vector3(0, 0, 0);
            var up = new Vector3(0, 1, 0);
            screen = new Screen(width, height);
            camera = new Camera(70, width/height, 0.1f, 10, eye, target, up);

            var scale = 0.02f;

            WorldModel = MatrixTemplates.Scale(new Vector3(scale, scale, scale)) *
                         MatrixTemplates.RotateZ(-90) *
                         camera.GetMatrix() *
                         MatrixTemplates.Projection(camera.FOV, camera.Aspect, camera.zNear, camera.zFar) *
                         screen.GetMatrix();



            img.Source = wBitmap;
            renderer = new Renderer(img);

            renderer.DrawEntityMesh(WorldModel, entity, width, height);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.D:
                    UpdateWorldModel(MatrixTemplates.RotateX(5));
                    renderer.DrawEntityMesh(WorldModel, entity, width, height);
                    break;
                case Key.A:
                    UpdateWorldModel(MatrixTemplates.RotateX(-5));
                    renderer.DrawEntityMesh(WorldModel, entity, width, height);
                    break;
                case Key.W:
                    UpdateWorldModel(MatrixTemplates.RotateZ(-5));
                    renderer.DrawEntityMesh(WorldModel, entity, width, height);
                    break;
                case Key.S:
                    UpdateWorldModel(MatrixTemplates.RotateZ(5));
                    renderer.DrawEntityMesh(WorldModel, entity, width, height);
                    break;
                case Key.Q:
                    UpdateWorldModel(MatrixTemplates.RotateY(-5));
                    renderer.DrawEntityMesh(WorldModel, entity, width, height);
                    break;
                case Key.E:
                    UpdateWorldModel(MatrixTemplates.RotateY(5));
                    renderer.DrawEntityMesh(WorldModel, entity, width, height);
                    break;
                case Key.Add or Key.OemPlus:
                    UpdateWorldModel(MatrixTemplates.Scale(new Vector3(1.1f, 1.1f, 1.1f)));
                    renderer.DrawEntityMesh(WorldModel, entity, width, height);
                    break;
                case Key.Subtract or Key.OemMinus:
                    UpdateWorldModel(MatrixTemplates.Scale(new Vector3(0.9f,0.9f,0.9f)));
                    renderer.DrawEntityMesh(WorldModel, entity, width, height);
                    break;
            }
        }

        public void UpdateWorldModel(Matrix4x4 m)
        {
            WorldModel = m * WorldModel;
        }
        
    }
}