using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MyBlend.Models.Basic
{
    public struct RgbColor
    {
        public const byte FullIntensity = byte.MaxValue; 
        public readonly byte R;
        public readonly byte G;
        public readonly byte B;
        public readonly byte A;
        private int color;
        public int Color => color;
        public RgbColor(byte r, byte g, byte b, byte a = 0)
        {
            R = r;
            G = g;
            B = b;
            A = a;
            color = (a << 24) | (r << 16) | (g << 8) | b;
        }

        public int ByIntensity(float k)
        {
            return (A << 24) | ((int)(R * k) << 16) | ((int)(G * k) << 8) | (int)(B * k);
        }
    }
}
