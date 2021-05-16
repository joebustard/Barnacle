using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Make3D.Models.LoopSmoothing
{
    internal class LoopPoint
    {
    public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }



        public LoopCoord UpdatedPosition { get; set; }

        public Int32Collection Edges { get; set; }
        public LoopPoint(Point3D p)
        {
            X = p.X;
            Y = p.Y;
            Z = p.Z;
            Edges = new Int32Collection();

        }


        public LoopPoint(LoopCoord cc)
        {
            X = cc.X;
            Y = cc.Y;
            Z =cc.Z;
            Edges = new Int32Collection();
        }


        public LoopPoint(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
            Edges = new Int32Collection();

        }
    }
}