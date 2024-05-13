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
        public static readonly RgbColor White = new(255,255,255);
        public static readonly RgbColor Black = new(0, 0, 0);

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
            k = Clamp(k, 0f, 1f);
            return (A << 24) | ((int)(R * k) << 16) | ((int)(G * k) << 8) | (int)(B * k);
        }

        public int ByIntensity(float ri, float gi, float bi)
        {
            ri = Clamp(ri, 0f, 1f);
            gi = Clamp(gi, 0f, 1f);
            bi = Clamp(bi, 0f, 1f);
            return (A << 24) | ((int)(R * ri) << 16) | ((int)(G * gi) << 8) | (int)(B * bi);
        }

        public RgbColor GetColorByIntensity(float k)
            => new RgbColor((byte)(R * k), (byte)(G * k), (byte)(B * k), A);

        public RgbColor UpdateColor(int color)
        {
            this.color |= color;
            return this;
        }

        public static RgbColor CrossColors(RgbColor c1, RgbColor c2)
        {
            return new RgbColor(Clamp(c1.R + c2.R), Clamp(c1.G + c2.G), Clamp(c1.B + c2.B), Clamp(c1.A + c2.A));
        }

        private static float Lerp(float a, float b, float t)
            => a + (b - a) * t;


        public static RgbColor Interpolate(RgbColor c1, RgbColor c2, float s)
        {
            s = Clamp(s, 0, 1);
            return new RgbColor((byte)Lerp(c1.R, c2.R, s), (byte)Lerp(c1.G, c2.G, s), (byte)Lerp(c1.B, c1.B, s));
        }


        private static byte Clamp(int value, float min = 0, float max = 255)
            => (byte)Math.Max(min, Math.Min(value, max));
        private static float Clamp(float value, float min, float max)
            => Math.Max(min, Math.Min(value, max));

    }
}
