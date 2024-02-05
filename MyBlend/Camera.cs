using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MyBlend
{
    public class Camera
    {
        public Vector3 Eye { get; set; }
        public Vector3 Target { get; set; }
        public Vector3 Up { get; set; }
        private Matrix4x4 matrix = Matrix4x4.Identity;

        public Camera(Vector3 eye, Vector3 target, Vector3 up)
        {
            Eye = eye;
            Target = target;
            Up = up;
        }

        public Matrix4x4 GetMatrix()
        {
            if(matrix.IsIdentity)
            {
                var zAxis = Vector3.Normalize(Eye - Target);
                var xAxis = Vector3.Normalize(Vector3.Cross(Up, zAxis));
                var yAxis = Up;
                matrix = new Matrix4x4(
                    xAxis.X, xAxis.Y, xAxis.Z, -Vector3.Dot(xAxis, Eye),
                    yAxis.X, yAxis.Y, yAxis.Z, -Vector3.Dot(yAxis, Eye),
                    zAxis.X, zAxis.Y, zAxis.Z, -Vector3.Dot(zAxis, Eye),
                    0, 0, 0, 1
                    );
            }

            return matrix;
            
        }

    }
}
