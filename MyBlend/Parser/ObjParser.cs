using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.IO;
using MyBlend.Models;
using MyBlend.Models.Basic;

namespace MyBlend.Parser
{

    public class ObjParser : IParser
    {
        private readonly ObjEntity? entity;

        public ObjParser(ObjEntity entity)
        {
            this.entity = entity;
        }

        public Entity Parse(string filepath)
        {
            try
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
            catch(Exception e)
            {
                throw new ParserException(e.ToString());
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
                    entity.Textures.Add(ReadTexture(args));
                    break;
                case "vn":
                    entity.Normals.Add(ReadNormal(args));
                    break;
            }
        }

        private Vector4 ReadCoordinate(string[] parameters)
        {
            float.TryParse(parameters[1], out var x);
            float.TryParse(parameters[2], out var y);
            float.TryParse(parameters[3], out var z);
            float w = 1f;
            if (parameters.Length > 4)
                float.TryParse(parameters[4], out w);
            return new Vector4(x, y, z, w);
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

        private Face[] ReadPoligon(string[] parameters)
        {
            var length = parameters.Length;
            var result = new Face[length - 1];
            for(int i = 1; i < length; i++)
            {
                var strIndexes = parameters[i].Split('/');
                var indexes = new int[strIndexes.Length];
                for(int j = 0; j < strIndexes.Length; j++)
                {
                    Int32.TryParse(strIndexes[j], out indexes[j]);
                    indexes[j]--; //because obj file starts indexes from 1
                }
                result[i - 1] = new Face(indexes);
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
