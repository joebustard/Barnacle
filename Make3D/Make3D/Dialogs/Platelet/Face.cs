using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    public class Face
    {
        private int[] edges;

        public int[] Edges
        {
            get { return edges; }
        }

        public int Mode { get; set; }

        public Face()
        {
            edges = new int[3];
            Mode = 0;
        }

        public Face(int e0, int e1, int e2)
        {
            edges = new int[3];
            edges[0] = e0;
            edges[1] = e1;
            edges[2] = e2;
            Mode = 0;
        }

        internal Point GetFlatCentroid(Point3DCollection vertices, List<Edge> edges)
        {
            double x = 0;
            double y = 0;
            for ( int i = 0; i < 3; i ++)
            {
                x += vertices[edges[i].P0].X;
                y += vertices[edges[i].P0].Y;
            }
            return new Point(x / 3.0, y / 3.0);
        }

        internal  void MoveForward(Point3DCollection vertices, List<Edge> edges)
        {
            
        }
    }
}