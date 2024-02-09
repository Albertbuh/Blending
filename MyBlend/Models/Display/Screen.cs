using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MyBlend.Models.Display;

public class Screen
{
    public float Width {  get; set; }
    public float Height { get; set; }
    private Matrix4x4 matrix = Matrix4x4.Identity;
    public Screen(float width, float height)
    {
        Width = width;
        Height = height;
    }

    public Matrix4x4 GetMatrix(float xmin = 0, float ymin = 0)
    {
        if(matrix.IsIdentity || (xmin != 0 || ymin != 0))
        {
            matrix = new Matrix4x4(
                    Width / 2, 0, 0, xmin + Width/2,
                    0, - Height / 2, 0, ymin + Height/2,
                    0, 0, 1, 0,
                    0, 0, 0, 1
                    );
        }
        return matrix;
    }



}
