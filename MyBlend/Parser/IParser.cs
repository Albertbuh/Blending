using MyBlend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyBlend.Models.Basic;

namespace MyBlend.Parser
{
    public interface IParser
    {
        public Entity Parse(string filepath);
    }
}
