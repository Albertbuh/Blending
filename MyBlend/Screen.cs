using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MyBlend
{
    public class Screen
    {
        public int Width {  get; set; }
        public int Height { get; set; }
        private Matrix4x4 matrix = Matrix4x4.Identity;
        public Screen(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public Matrix4x4 GetMatrix(int xmin = 0, int ymin = 0)
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
}
