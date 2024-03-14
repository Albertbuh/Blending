using MyBlend.Models.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MyBlend.Models.Light
{
    public struct Light
    {
        public Vector3 Position;
        public RgbColor Color;

        public Light(Vector3 position, RgbColor color)
        {
            Position = position;
            Color = color;
        }

        public Light(Vector3 position, byte color)
            : this(position, new RgbColor(color, color, color)) { }
    }
}
