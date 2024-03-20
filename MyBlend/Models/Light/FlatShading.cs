using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using MyBlend.Models.Basic;

namespace MyBlend.Models.Light
{
    public class FlatShading : Shading
    {
        public FlatShading()
        {
        }

        public override float GetColorIntensity(Vector3 light, Vertex va, Vertex vb, Vertex vc, Vector3 p)
        {
            return CountLightIntension(light, va, vb, vc, p);
        }

        float CountLightIntension(Vector3 light, Vertex v1, Vertex v2, Vertex v3, Vector3 p)
        {
            var l1 = CalculateNormalDotLight(v1.Normal, light);
            var l2 = CalculateNormalDotLight(v2.Normal, light);
            var l3 = CalculateNormalDotLight(v3.Normal, light);
            return Math.Max(0, (float)(l1 + l2 + l3) / 3);
        }
    }
}
