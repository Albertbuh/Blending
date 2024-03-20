using MyBlend.Models.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace MyBlend.Models.Light
{
    public struct Light
    {
        public Vector3 Position;
        public RgbColor Color;

        public Shading? Shader;

        public Light(Vector3 position, RgbColor color)
        {
            Position = position;
            Color = color;
        }

        public Light(Vector3 position, byte color)
            : this(position, new RgbColor(color, color, color)) { }


        
        public float CalculateNormalDotLight(Vector3 normal) => Math.Max(0, Vector3.Dot(Vector3.Normalize(normal), Vector3.Normalize(Position)));
    }
}
