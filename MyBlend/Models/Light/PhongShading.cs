using MyBlend.Models.Basic;
using MyBlend.Models.Display;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace MyBlend.Models.Light
{
    public class PhongShading : Shading
    {
        private Screen screen;

        public PhongShading(Screen screen)
        {
            this.screen = screen;
        }

        public override float GetColorIntensity(Vector3 light, Vertex va, Vertex vb, Vertex vc, Vector3 p)
        {
            var phong = GetColorByPhong(light, va, vb, vc, p);
            return (float)phong / 255;
        }

        int AddAmbientColor()
        {
            const int ia = 255;
            const float ka = 0.035f;
            var clr = (int)(ka * ia);
            return clr;
        }
        int AddDiffuseColor(Vector3 light, Vector3 normal, Vector3 cur)
        {
            const float kd = 0.3f;
            const int id = 255;
            var clr = (int)(kd * CalculateNormalDotLight(normal, light) * id);
            return clr;
        }

        int AddSpecularColor(Vector3 light, Vector3 normal, Vector3 cur)
        {
            normal = Vector3.Normalize(normal);
            var dir = Vector3.Normalize(light);
            const float ks = 0.4f;
            const int iS = 255;
            const float alpha = 32f;
            var R = Vector3.Normalize(dir - 2 * Vector3.Dot(dir, normal) * normal);
            var rv = Vector3.Dot(Vector3.Normalize(screen.Camera!.Eye), R);
            var clr = (int)(ks * Math.Pow(rv, alpha) * iS);
            return Math.Max(0, clr);
        }

        int GetColorByPhong(Vector3 light, Vertex va, Vertex vb, Vertex vc, Vector3 cur)
        {
            FindBarycentricCoordinates(va.GlobalPosition, vb.GlobalPosition, vc.GlobalPosition, cur, out var u, out var v, out var w);
            var normal = u * va.Normal + v * vb.Normal + w * vc.Normal;
            return AddAmbientColor() + AddDiffuseColor(light, normal, cur) + AddSpecularColor(light, normal, cur);
        }

        public override Vector3 GetNormalOfPoint(Vertex va, Vertex vb, Vertex vc, Vector3 cur)
        {
            FindBarycentricCoordinates(va.GlobalPosition, vb.GlobalPosition, vc.GlobalPosition, cur, out var u, out var v, out var w);
            return u * va.Normal + v * vb.Normal + w * vc.Normal;
        }

        void FindBarycentricCoordinates(Vector4 A, Vector4 B, Vector4 C, Vector3 P, out float u, out float v, out float w)
        {
            float S = (A.X * (B.Y - C.Y) + B.X * (C.Y - A.Y) + C.X * (A.Y - B.Y)) / 2.0f;
            u = ((B.Y - C.Y) * (P.X - C.X) + (C.X - B.X) * (P.Y - C.Y)) / (2.0f * S);
            v = ((C.Y - A.Y) * (P.X - C.X) + (A.X - C.X) * (P.Y - C.Y)) / (2.0f * S);
            w = 1.0f - u - v;
        }

    }
}
