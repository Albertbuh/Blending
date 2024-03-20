using MyBlend.Models.Basic;
using MyBlend.Models.Light;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBlend.Graphics
{
    public struct ScanLineData
    {
        public int Y;
        public RgbColor Color;
        public readonly IEnumerable<Light>? Lights;

        public ScanLineData(IEnumerable<Light>? lights)
        {
            Lights = lights;
        }
    }
}
