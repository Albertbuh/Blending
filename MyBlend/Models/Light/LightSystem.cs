using MyBlend.Models.Basic;
using MyBlend.Models.Display;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MyBlend.Models.Light
{
    public static class LightSystem
    {
        public static float CalcBlinnPhongLightIntensity(Vector3 camera, Vector3 light, Vector3 position, Vector3 normal)
        {
            return AddAmbientColor() + AddDiffuseColor(light, normal, position) + AddSpecularColorBlinnPhong(camera, light, normal, position);
        }

        public static float CalcPhongLightIntensity(Vector3 camera, Vector3 light, Vector3 position, Vector3 normal)
        {
            return AddAmbientColor() + AddDiffuseColor(light, normal, position) + AddSpecularColorPhong(camera, light, normal, position);
        }

        public static float CalcCelShading(Vector3 camera, Vector3 light, Vector3 position, Vector3 normal)
        {
            var lightDirection = Vector3.Normalize(position - light);
            var intensity = Math.Max(0, Math.Min(1f, Vector3.Dot(normal, -lightDirection)));
            if (intensity < .3f)
                return intensity;
            else if (intensity < .85f)
                return .7f;
            else
                return 1;
        }

        public static float CalcFlatShading(Vector3 camera, Vector3 light, Vector3 position, Vector3 normal)
        {
            return AddAmbientColor() + AddDiffuseColor(light, normal, position);
        }
        
        static float AddAmbientColor()
        {
            const float ka = 0.035f;
            return ka;
        }

        static float AddDiffuseColor(Vector3 light, Vector3 normal, Vector3 cur)
        {
            const float kd = 0.6f;
            light = cur - light;
            return kd * CalculateNormalDotLight(normal, -light);
        }
        static float AddSpecularColorBlinnPhong(Vector3 camera, Vector3 light, Vector3 normal, Vector3 cur)
        {
            const float ks = 0.25f;
            const float shininess = 32f;
            normal = Vector3.Normalize(normal);
            camera = Vector3.Normalize(camera - cur);
            light = Vector3.Normalize(light - cur);
            var halfwayDir = Vector3.Normalize(light + camera);
            return (float)(Math.Max(0, ks * Math.Pow(Vector3.Dot(normal, halfwayDir), shininess)));
        }

        static float AddSpecularColorPhong(Vector3 camera, Vector3 light, Vector3 normal, Vector3 cur)
        {
            const float ks = 0.2f;
            const float alpha = 8f;
            normal = Vector3.Normalize(normal);
            var dir = Vector3.Normalize(cur - light);
            var R = Vector3.Normalize(dir - 2 * Vector3.Dot(dir, normal) * normal);
            var rv = Vector3.Dot(Vector3.Normalize(camera), R);
            return (float)(ks * Math.Pow(rv, alpha));
        }


        static float CalculateNormalDotLight(Vector3 normal, Vector3 light) => Math.Max(0, Vector3.Dot(Vector3.Normalize(normal), Vector3.Normalize(light)));
    }

}
