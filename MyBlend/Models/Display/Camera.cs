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
        private Vector3 eye;
        public Vector3 Eye
        {
            get => eye;
            set => eye += value;
        }

        private Vector3 target;
        public Vector3 Target
        {
            get => target;
            private set => target = value;
        }


        public Vector3 Up { get; set; }

        private const float minFov = 5;
        private const float maxFov = 178;
        private float fov;
        public float FOV {
            get => fov;
            set => fov = Math.Min(Math.Max(value, minFov), maxFov);
        }

        public float Aspect { get; set; }
        public float zNear { get; set; }
        public float zFar { get; set; }


        private float phi;
        private const float minPhi = (float)Math.PI / 180 * 2;
        private const float maxPhi = (float)Math.PI / 180 * 178;
        public float Phi
        {
            get => phi;
            set => phi = value % ((float)Math.PI * 2);
        }
        private float zeta;
        public float Zeta
        {
            get => zeta;
            set => zeta = value % ((float)Math.PI * 2);
        }
        private float radius { get; set; }
        public Camera(float fov, float aspect, float znear, float zfar, Vector3 eye, Vector3 target, Vector3 up)
        {
            this.eye = eye;
            Target = target;
            Up = up;
            this.fov = fov;
            Aspect = aspect;
            zNear = znear;  
            zFar = zfar;

            radius = (float)Math.Sqrt(this.eye.X * eye.X + eye.Z * eye.Z + eye.Y * eye.Y);
            zeta = (float)Math.Atan2(this.eye.Z, this.eye.X);
            phi = (float)Math.Acos(this.eye.Y / radius);
        }

        public Matrix4x4 GetLookAtMatrix() => Matrix4x4.CreateLookAt(UpdateEyePosition(), target, Up);
        public Matrix4x4 GetPerspectiveMatrix() => Matrix4x4.CreatePerspectiveFieldOfView(fov, Aspect, zNear, zFar);
        private Vector3 UpdateEyePosition()
        {
            if (phi >= maxPhi)
            {
                phi = maxPhi;
            }
            else if(phi <= minPhi)
            {
                phi = minPhi;
            }

            eye.X = (float)(radius * Math.Sin(phi) * Math.Cos(zeta));
            eye.Z = (float)(radius * Math.Sin(phi) * Math.Sin(zeta));
            eye.Y = (float)(radius * Math.Cos(phi));
            return eye;
        }

        public void UpdateTarget(Vector3 delta)
        {
            target.X += delta.X;
            target.Y += delta.Y;
            target.Z += delta.Z;
        }


    }
}
