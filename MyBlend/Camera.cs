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

        public Camera(Vector3 eye, Vector3 target, Vector3 up)
        {
            Eye = eye;
            Target = target;
            Up = up;
        }

        private Vector3 Normalize(Vector3 v)
        {
            var magnitude = v.X * v.X + v.Y * v.Y + v.Z * v.Z;
            return new Vector3(v.X / magnitude, v.Y / magnitude, v.Z / magnitude);
        }

        private float Dot(Vector3 v1, Vector3 v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z; 
        }

        private Matrix4x4 GetMatrix()
        {
            var zAxis = Normalize(Eye - Target);
            var xAxis = Normalize(Up * zAxis);
            var yAxis = Up;
            return new Matrix4x4(
                xAxis.X, xAxis.Y, xAxis.Z, -Dot(xAxis, Eye),
                yAxis.X, yAxis.Y, yAxis.Z, -Dot(yAxis, Eye),
                zAxis.X, zAxis.Y, zAxis.Z, -Dot(zAxis, Eye),
                0, 0, 0, 1
                );
        }

    }
}
