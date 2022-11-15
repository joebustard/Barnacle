using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barnacle.Dialogs
{
    public class Face
    {
        private int[] edges;

        public int[] Edges
        {
            get { return edges; }
        }

        public bool Raised { get; set; }

        public Face()
        {
            edges = new int[3];
            Raised = false;
        }

        public Face(int e0, int e1, int e2)
        {
            edges = new int[3];
            edges[0] = e0;
            edges[1] = e1;
            edges[2] = e2;
            Raised = false;
        }
    }
}