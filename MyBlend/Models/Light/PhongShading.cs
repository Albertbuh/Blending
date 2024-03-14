using MyBlend.Models.Basic;
using MyBlend.Models.Display;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MyBlend.Models.Light
{
    public class PhongShading : Shading
    {
        private Light light;
        private Screen screen;

        public PhongShading(Light light, Screen screen)
        {
            this.light = light;
            this.screen = screen;
        }

        public override float GetColorIntensity(Vertex va, Vertex vb, Vertex vc, Vector3 p)
        {
            var phong = GetColorByPhong(va, vb, vc, p);
            return (float)phong / 255;
            
        }

        int AddAmbientColor()
        {
            const int ia = 255;
            const float ka = 0.05f;
            var clr = (int)(ka * ia);
            return clr;
        }
        int AddDiffuseColor(Vector3 normal, Vector3 cur)
        {
            var dir = Vector3.Normalize(light.Position);
            const float kd = 0.4f;
            const int id = 255;
            var clr = (int)(kd * CalculateNormalDotLight(normal, dir) * id);
            return clr;
        }

        int AddSpecularColor(Vector3 normal, Vector3 cur)
        {
            normal = Vector3.Normalize(normal);
            var dir = Vector3.Normalize(light.Position);
            const float ks = 0.4f;
            const int iS = 255;
            const float alpha = 32f;
            var R = Vector3.Normalize(dir - 2 * Vector3.Dot(dir, normal) * normal);
            if (R == Vector3.Zero)
                R = dir;
            var rv = Vector3.Dot(Vector3.Normalize(screen.Camera!.Eye), R);
            var clr = (int)(ks * Math.Pow(rv, alpha) * iS);
            return Math.Max(0, clr);
        }

        int GetColorByPhong(Vertex va, Vertex vb, Vertex vc, Vector3 cur)
        {
            FindBarycentricCoordinates(va.WorldPosition, vb.WorldPosition, vc.WorldPosition, cur, out var u, out var v, out var w);
            var normal = u * va.Normal + v * vb.Normal + w * vc.Normal;
            return AddAmbientColor() + AddDiffuseColor(normal, cur) + AddSpecularColor(normal, cur);
        }

        //void CalculateBarycentricCoordinates(Vector3 A, Vector3 B, Vector3 C, Vector3 P, out float v1, out float v2, out float v3)
        //{
        //    Vector3 v0 = B - A;
        //    Vector3 v1 = C - A;
        //    Vector3 v2 = P - A;

        //    float d00 = Vector3.Dot(v0, v0);
        //    float d01 = Vector3.Dot(v0, v1);
        //    float d11 = Vector3.Dot(v1, v1);
        //    float d20 = Vector3.Dot(v2, v0);
        //    float d21 = Vector3.Dot(v2, v1);

        //    float denom = d00 * d11 - d01 * d01;

        //    v2 = (d11 * d20 - d01 * d21) / denom;
        //    v3 = (d00 * d21 - d01 * d20) / denom;
        //    v1 = 1 - v2 - v3;
        //}

        void FindBarycentricCoordinates(Vector3 A, Vector3 B, Vector3 C, Vector3 P, out float u, out float v, out float w)
        {
            float S = (A.X * (B.Y - C.Y) + B.X * (C.Y - A.Y) + C.X * (A.Y - B.Y)) / 2.0f;
            u = ((B.Y - C.Y) * (P.X - C.X) + (C.X - B.X) * (P.Y - C.Y)) / (2.0f * S);
            v = ((C.Y - A.Y) * (P.X - C.X) + (A.X - C.X) * (P.Y - C.Y)) / (2.0f * S);
            w = 1.0f - u - v;
        }

    }
}
