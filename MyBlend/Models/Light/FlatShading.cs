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
        private Vector3 normal;
        private Light light;
        private int color;
        public FlatShading(Light light, int color)
        {
            this.light = light;
            this.color = color;
        }

        public override int GetColorWithShading(Vertex va, Vertex vb, Vertex vc, Vector3 p)
        {
            return (int)(color * CountLightIntension(va, vb, vc));
        }

        float CountLightIntension(Vertex v1, Vertex v2, Vertex v3)
        {
            var l1 = CalculateNormalDotLight(v1.Normal, light);
            var l2 = CalculateNormalDotLight(v2.Normal, light);
            var l3 = CalculateNormalDotLight(v3.Normal, light);

            return Math.Max(0, (float)(l1 + l2 + l3) / 3);
        }
    }
}
