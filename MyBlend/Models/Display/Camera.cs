using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MyBlend.Models.Display
{
    public class Camera
    {
        public Vector3 Eye { get; set; }
        public Vector3 Target { get; set; }
        public Vector3 Up { get; set; }
        private Matrix4x4 matrix = Matrix4x4.Identity;

        public float FOV { get; set; }
        public float Aspect { get; set; }
        public float zNear { get; set; }
        public float zFar { get; set; }
        
        public Camera(float fov, float aspect, float znear, float zfar, Vector3 eye, Vector3 target, Vector3 up)
        {
            Eye = eye;
            Target = target;
            Up = up;
            FOV = fov;
            Aspect = aspect;
            zNear = znear;  
            zFar = zfar;
        }

        public Matrix4x4 GetMatrix()
        {
            if (matrix.IsIdentity)
            {
                var zAxis = Vector3.Normalize(Eye - Target);
                var xAxis = Vector3.Normalize(Vector3.Cross(Up, zAxis));
                var yAxis = Vector3.Cross(zAxis, xAxis);
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
