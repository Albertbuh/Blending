using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MyBlend.Models.Basic;

public class ObjEntity : Entity
{
    public ObjEntity()
        : base(new List<Vector4>(), new List<Vector3>(), new List<Vector3>(), new List<Face[]>())
    {
    }
}
