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
using FileParsing;

namespace MyBlend
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObjModel fileParser;
        public MainWindow()
        {
            InitializeComponent();
            fileParser = new ObjModel();
            fileParser.ParseFile(@"D:\Univer\acg\russian-archipelago-frigate-svjatoi-nikolai\source\SM_Ship01A_02_OBJ.obj");
        }
    }
}