using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using MyBlend.Models.Textures;

namespace MyBlend.Models.Basic;

public class ObjEntity : Entity
{
    public Dictionary<string, Texture> Textures;
    public ObjEntity()
        : base(new List<Vector4>(), new List<Vector2>(), new List<Vector3>(), new List<Face[]>())
    {
        Textures = new();
    }

    public override void Clear()
    {
        base.Clear();
        Textures.Clear();
    }
}
