using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MyBlend.Models.Display;

public class Screen
{
    private float width;
    private float height;
    public float Width
    {
        get => width;
        set => width = value;
    }
    public float Height
    {
        get => height;
        set => height = value;
    }

    public Image Display { get; set; }
    public Screen(Image image, float width, float height)
    {
        this.Display = image;
        this.width = width;
        this.height = height;
    }

    public Matrix4x4 GetMatrix(float xmin = 0, float ymin = 0) => Matrix4x4.CreateViewport(0, 0, width, height, 0, 10);
 

}
