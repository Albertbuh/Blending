using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.IO;
using MyBlend.Models;
using MyBlend.Models.Basic;
using MyBlend.Models.Textures;
using System.Security.Policy;

namespace MyBlend.Parser
{

    public class ObjParser : IParser
    {
        private readonly ObjEntity? entity;

        public ObjParser(ObjEntity entity)
        {
            this.entity = entity;
        }

        public Entity? Parse(string filepath)
        {
            try
            {
                Clear();
                using (StreamReader sr = new StreamReader(filepath))
                {
                    string? line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        line = line.Replace('.', ',').Replace("   ", " ").Replace("  ", " ").Trim();
                        AnalizeLine(line);
                    }
                }
                ReadMtlFile(filepath);
            }
            catch(Exception e)
            {
                throw new ParserException(e.ToString());
            }
            return entity;
        }

        private string[] mtlComponents = new string[]
        {
            "map_Kd", "norm", "map_MRAO"
        };

        private Texture[]? ReadMtlFile(string filepath)
        {
            try
            {
                filepath = filepath.Substring(0, filepath.IndexOf(".") + 1) + "mtl";
                var textureTasks = new List<Task>();
                using (var sr = new StreamReader(filepath, Encoding.ASCII))
                {
                    string? line;
                    while((line = sr.ReadLine()) != null)
                    {
                        var componentName = line.Substring(0, line.IndexOf(" "));
                        if(mtlComponents.Contains(componentName))
                        {
                            var path = filepath.Substring(0, filepath.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                            path += line.Substring(line.IndexOf(" ") + 1);
                            textureTasks.Add(Task.Run(() => entity?.Textures.Add(componentName, new Texture(path))));
                            //entity?.Textures.Add(componentName, new Texture(path));
                        }
                    }
                }
                Task.WaitAll(textureTasks.ToArray());
                return entity?.Textures.Values.ToArray();

            }
            catch(Exception e)
            {
                throw new ParserException($"Error occured while read MTL file {e.ToString}", e);
            }
        }


        private void Clear()
        {
            entity?.Clear();
        }
        private void AnalizeLine(string line)
        {
            var args = line.Split(' ');
            switch (args[0])
            {
                case "f":
                    entity!.Faces.Add(ReadPoligon(args));
                    break;
                case "v":
                    entity!.Positions.Add(ReadCoordinate(args));
                    break;
                case "vt":
                    entity!.TexturePositions.Add(ReadTexturePosition(args));
                    break;
                case "vn":
                    entity!.NormalPositions.Add(ReadNormal(args));
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

        private Vector2 ReadTexturePosition(string[] parameters)
        {
            var length = parameters.Length;
            float u = 0, v = 0, w = 0;

            float.TryParse(parameters[1], out u);
            if (length == 3)
                float.TryParse(parameters[2], out v);

            return new Vector2(u, v);
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
                    //because obj file starts indexes from 1
                    indexes[j]--; 
                }
                result[i - 1] = new Face(indexes);
            }
            return result;
        }
        
    }

    
    
}
