using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MyBlend
{
    public class ObjEntity
    {
        public List<Vector3> Positions { get; set; }
        public List<Vector3> TexturePositions { get; set; }
        public List<Vector3> Normals { get; set; }
        public List<List<Apex>> Poligons { get; set; }

        public ObjEntity()
        {
            Positions = new List<Vector3>();
            TexturePositions = new List<Vector3>();
            Normals = new List<Vector3>();
            Poligons = new List<List<Apex>>();
        }
    }
}
