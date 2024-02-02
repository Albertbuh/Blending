using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.IO;

namespace MyBlend
{

    public class ObjParser
    {
        private readonly ObjEntity entity;
        public ObjParser(ObjEntity entity)
        {
            this.entity = entity;
        }

        public ObjEntity ParseFile(string filepath)
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
            return entity;
        }

        private void AnalizeLine(string line)
        {
            var args = line.Split(' ');
            switch (args[0])
            {
                case "f":
                    entity.Poligons.Add(ReadPoligon(args));
                    break;
                case "v":
                    entity.Positions.Add(ReadCoordinate(args));
                    break;
                case "vt":
                    entity.TexturePositions.Add(ReadTexture(args));
                    break;
                case "vn":
                    entity.Normals.Add(ReadNormal(args));
                    break;
            }
        }

        private Vector3 ReadCoordinate(string[] parameters)
        {
            float.TryParse(parameters[1], out var x);
            float.TryParse(parameters[2], out var y);
            float.TryParse(parameters[3], out var z);
            return new Vector3(x, y, z);
        }

        private Vector3 ReadTexture(string[] parameters)
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

        private Vector3 ReadNormal(string[] parameters)
        {
            float.TryParse(parameters[1], out var i);
            float.TryParse(parameters[2], out var j);
            float.TryParse(parameters[3], out var k);
            return new Vector3(i, j, k);
        }

        private List<Apex> ReadPoligon(string[] parameters)
        {
            var result = new List<Apex>();
            for(int i = 1; i < parameters.Length; i++)
            {
                var indexes = parameters[i].Split('/');
                GetVectorFromCollectionByIndex(indexes[0], entity.Positions, out var v);
                GetVectorFromCollectionByIndex(indexes[1], entity.TexturePositions, out var vt);
                GetVectorFromCollectionByIndex(indexes[2], entity.Normals, out var vn);
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
