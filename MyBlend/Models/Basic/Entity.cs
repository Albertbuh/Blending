using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MyBlend.Models.Basic;

public class Entity
{
    public List<Vector4> Positions { get; set; }
    public List<Vector3> Textures { get; set; }
    public List<Vector3> Normals { get; set; }
    public List<Face[]> Poligons { get; set; }

    public Entity() { }
    public Entity(List<Vector4> positions, List<Vector3> textures, List<Vector3> normals, List<Face[]> poligons)
    {
        Positions = positions;
        Textures = textures;
        Normals = normals;
        Poligons = poligons;
    }

    public void Clear()
    {
        Positions.Clear();
        Textures.Clear();
        Normals.Clear();
        Poligons.Clear();
    }

    public List<Vector4> GetPositionsInWorldModel(Matrix4x4 worldModel)
    {
        var result = new List<Vector4>();
        foreach (var position in Positions)
        {
            result.Add(MultiplyVectorByMatrix(position, worldModel));
        }
        return result;
    }

    private Vector4 MultiplyVectorByMatrix(Vector4 v, Matrix4x4 m)
    {
        return new Vector4(
            v.X * m.M11 + v.Y * m.M21 + v.Z * m.M31 + v.W * m.M41,
            v.X * m.M12 + v.Y * m.M22 + v.Z * m.M32 + v.W * m.M42,
            v.X * m.M13 + v.Y * m.M23 + v.Z * m.M33 + v.W * m.M43,
            v.X * m.M14 + v.Y * m.M24 + v.Z * m.M34 + v.W * m.M44
            );
    }
}
