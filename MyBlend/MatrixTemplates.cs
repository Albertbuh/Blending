using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace MyBlend
{
    public static class MatrixTemplates
    {

        public static Matrix4x4 Identity()
        {
            return new Matrix4x4(
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1
                );
        }

        public static Matrix4x4 Movement(Vector3 translation)
        {
            return new Matrix4x4(
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                translation.X, translation.Y, translation.Z, 1
                );
        }

        public static Matrix4x4 Movement(float x, float y, float z)
        {
            return new Matrix4x4(
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                x, y, z, 1
                );
        }

        public static Matrix4x4 Scale(Vector3 scale)
        {
            return new Matrix4x4(
                scale.X, 0, 0, 0,
                0, scale.Y, 0, 0,
                0, 0, scale.Z, 0,
                0, 0, 0, 1
                );
        }

        public static Matrix4x4 RotateX(double angle)
        {
            angle = DegToRad(angle);
            var cos = (float)Math.Cos(angle);
            var sin = (float)Math.Sin(angle);
            return new Matrix4x4(
                1, 0, 0, 0,
                0, cos, -sin, 0,
                0, sin, cos, 0,
                0, 0, 0, 1
                );
        }
        public static Matrix4x4 RotateY(double angle)
        {
            angle = DegToRad(angle);
            var cos = (float)Math.Cos(angle);
            var sin = (float)Math.Sin(angle);
            return new Matrix4x4(
                cos, 0, sin, 0,
                0, 1, 0, 0,
               -sin, 0, cos,0,
                0, 0, 0, 1
                );
        }

        public static Matrix4x4 RotateZ(double angle)
        {
            angle = DegToRad(angle);
            var cos = (float)Math.Cos(angle);
            var sin = (float)Math.Sin(angle);
            return new Matrix4x4(
                cos, -sin, 0, 0,
                sin, cos, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1
                );
        }

        public static Matrix4x4 Projection(float fov, float aspect, float znear, float zfar)
        {
            var fovTan = (float)(Math.Tan((DegToRad(fov)) / 2));
            return new Matrix4x4(
                1 / (aspect * fovTan), 0,0,0,
                0, 1 / fovTan, 0, 0,
                0, 0, zfar / (znear - zfar), (znear * zfar) / (znear - zfar),
                0, 0, -1, 0
                );
        }

        private static double DegToRad(double angle)
        {
            return (Math.PI / 180) * angle;
        }
    }
}
