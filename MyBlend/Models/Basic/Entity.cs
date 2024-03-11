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
            result.Add(CountPositionInWorld(position, worldModel));
        }
        return result;
    }

    public List<Vector3> GetNormalsInWorldModel(Matrix4x4 worldModel)
    {
        var result = new List<Vector3>();
        foreach(var normal in Normals)
        {
            result.Add(Vector3.Transform(normal, worldModel));
        }
        return result;
    }


    private Vector4 CountPositionInWorld(Vector4 v, Matrix4x4 m)
    {
        var result = Vector4.Transform(v, m);
        if (result.W <= 0)
            return Vector4.Zero;
        return new Vector4(result.X / result.W, result.Y / result.W, result.Z / result.W, result.W/result.W);
    }
}

