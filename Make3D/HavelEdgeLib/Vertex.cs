using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace HalfEdgeLib
{
    public class Vertex
    {

        public float X;
        public float Y;
        public float Z;
        public int OutgoingHalfEdge;

        public Vertex()
        {
            X = 0;
            Y = 0;
            Z = 0;
            OutgoingHalfEdge = -1;
        }
        public Vertex(Point3D p)
        {
            X = (float)p.X;
            Y = (float)p.Y;
            Z = (float)p.Z;
            OutgoingHalfEdge = -1;
        }
        public Vertex(Point3D p, int e)
        {
            X = (float)p.X;
            Y = (float)p.Y;
            Z = (float)p.Z;
            OutgoingHalfEdge = e;
        }
        public Vertex(double x, double y, double z, int e)
        {
            X = (float)x;
            Y = (float)y;
            Z = (float)z;
            OutgoingHalfEdge = e;
        }
    }
}
