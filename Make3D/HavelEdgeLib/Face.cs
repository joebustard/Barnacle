using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalfEdgeLib
{
    public class Face
    {
        public int FirstEdge;

        public Face(int f)
        {
            FirstEdge = f;
        }
    }
}
