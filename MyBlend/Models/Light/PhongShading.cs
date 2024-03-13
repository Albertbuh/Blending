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
        private int color;

        public PhongShading(Light light, Screen screen, int color)
        {
            this.light = light;
            this.color = color;
            this.screen = screen;
        }

        public override int GetColorWithShading(Vertex va, Vertex vb, Vertex vc, Vector3 p)
        {
            return GetColorByPhong(va, vb, vc, p);
        }

        int AddAmbientColor()
        {
            const int ia = 255;
            const float ka = 0.2f;
            var clr = (int)(ka * ia);
            return clr;
        }
        int AddDiffuseColor(Vector3 normal, Light light)
        {
            const float kd = 0.4f;
            const int id = 255;
            var clr = (int)(kd * CalculateNormalDotLight(normal, light) * id);
            return clr;
        }

        int AddSpecularColor(Vector3 normal, Light light)
        {
            const float ks = 0.4f;
            const int iS = 100;
            const float alpha = 0.1f;
            var R = light.Position - 2 * Vector3.Dot(light.Position, normal) * normal;
            var rv = Math.Max(0, Vector3.Dot(screen.Camera!.Eye, R));
            var clr = (int)(ks * Math.Pow(rv, alpha) * iS);
            return clr;
        }

        int GetColorByPhong(Vertex va, Vertex vb, Vertex vc, Vector3 cur)
        {
            CalculateBarycentricCoordinates(va.WorldPosition, vb.WorldPosition, vc.WorldPosition, cur, out var u, out var v, out var w);
            var normal = u * va.Normal + v * vb.Normal + w * vc.Normal;
            return AddAmbientColor() + AddDiffuseColor(normal, light) + AddSpecularColor(normal, light);
        }

        void CalculateBarycentricCoordinates(Vector3 A, Vector3 B, Vector3 C, Vector3 P, out float u, out float v, out float w)
        {
            Vector3 v0 = B - A;
            Vector3 v1 = C - A;
            Vector3 v2 = P - A;

            float d00 = Vector3.Dot(v0, v0);
            float d01 = Vector3.Dot(v0, v1);
            float d11 = Vector3.Dot(v1, v1);
            float d20 = Vector3.Dot(v2, v0);
            float d21 = Vector3.Dot(v2, v1);

            float denom = d00 * d11 - d01 * d01;

            v = (d11 * d20 - d01 * d21) / denom;
            w = (d00 * d21 - d01 * d20) / denom;
            u = 1 - v - w;
        }

    }
}
