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
        private Light light;
        public FlatShading(Light light)
        {
            this.light = light;
        }

        public override float GetColorIntensity(Vertex va, Vertex vb, Vertex vc, Vector3 p)
        {
            return CountLightIntension(va, vb, vc, p);
        }

        float CountLightIntension(Vertex v1, Vertex v2, Vertex v3, Vector3 p)
        {
            var dir = - (p - light.Position);
            var l1 = CalculateNormalDotLight(v1.Normal, dir);
            var l2 = CalculateNormalDotLight(v2.Normal, dir);
            var l3 = CalculateNormalDotLight(v3.Normal, dir);
            return Math.Max(0, (float)(l1 + l2 + l3)/3);
        }
    }
}
