using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.IO;

namespace MyBlend
{
    public record Apex(Vector3? position, Vector3? texture, Vector3? normal);

    public class ObjModel
    {
        public List<Vector3> Positions { get; set; }
        public List<Vector3> TexturePositions { get; set; }
        public List<Vector3> Normals { get; set; }
        public List<List<Apex>> Poligons { get; set; }

        public ObjModel()
        {
            Positions = new List<Vector3>();
            TexturePositions = new List<Vector3>();
            Normals = new List<Vector3>();
            Poligons = new List<List<Apex>>();
        }

        public void ParseFile(string filepath)
        {
            using (StreamReader sr = new StreamReader(filepath))
            {
                string? line;
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Replace('.', ',');
                    AnalizeLine(line);
                }
            }
        }

        private void AnalizeLine(string line)
        {
            var args = line.Split(' ');
            switch (args[0])
            {
                case "f":
                    Poligons.Add(CreatePoligon(args));
                    break;
                case "v":
                    Positions.Add(CreateCoordinate(args));
                    break;
                case "vt":
                    TexturePositions.Add(CreateTextureCoordinate(args));
                    break;
                case "vn":
                    Normals.Add(CreateNormal(args));
                    break;
            }
        }

        private Vector3 CreateCoordinate(string[] parameters)
        {
            float.TryParse(parameters[1], out var x);
            float.TryParse(parameters[2], out var y);
            float.TryParse(parameters[3], out var z);
            return new Vector3(x, y, z);
        }

        private Vector3 CreateTextureCoordinate(string[] parameters)
        {
            var length = parameters.Length;
            float u = 0, v = 0, w = 0;

            float.TryParse(parameters[1], out u);
            if (length == 3)
                float.TryParse(parameters[2], out v);
            if (length == 4)
                float.TryParse(parameters[3], out w);

            return new Vector3(u, v, w);
        }

        private Vector3 CreateNormal(string[] parameters)
        {
            float.TryParse(parameters[1], out var i);
            float.TryParse(parameters[2], out var j);
            float.TryParse(parameters[3], out var k);
            return new Vector3(i, j, k);
        }

        private List<Apex> CreatePoligon(string[] parameters)
        {
            var result = new List<Apex>();
            for(int i = 1; i < parameters.Length; i++)
            {
                var indexes = parameters[i].Split('/');
                GetVectorFromCollectionByIndex(indexes[0], Positions, out var v);
                GetVectorFromCollectionByIndex(indexes[1], TexturePositions, out var vt);
                GetVectorFromCollectionByIndex(indexes[2], Normals, out var vn);
                result.Add(new Apex(v, vt, vn));
            }
            return result;
        }

        private void GetVectorFromCollectionByIndex(string ind, IList<Vector3> collection, out Vector3? vector)
        {
            if (int.TryParse(ind, out var vInd))
            {
                //obj file starts indexes from 1
                vector = collection[vInd - 1];
            }
            else
                vector = null;
        }
    }

    
    
}
