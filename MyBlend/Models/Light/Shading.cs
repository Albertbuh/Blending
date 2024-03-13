using MyBlend.Models.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MyBlend.Models.Light
{
    public abstract class Shading
    {
        protected float CalculateNormalDotLight(Vector3 normal, Light light) => Math.Max(0, Vector3.Dot(Vector3.Normalize(normal), Vector3.Normalize(-light.Position)));

        public abstract int GetColorWithShading(Vertex va, Vertex vb, Vertex vc, Vector3 p);
    }
}
