using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBlend.Models.Basic;

public struct Face
{
    public int vIndex { get; set; }
    public int tIndex { get; set; }
    public int nIndex { get; set; }

    public Face(params int[] indexes)
    {
        if (indexes.Length > 0)
            vIndex = indexes[0];
        if (indexes.Length > 1)
            tIndex = indexes[1];
        if (indexes.Length > 2)
            nIndex = indexes[2];
    }
}
