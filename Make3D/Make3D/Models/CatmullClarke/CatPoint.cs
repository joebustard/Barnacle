using System.Windows.Media.Media3D;

namespace Make3D.Models.CatmullClarke
{
    internal class CatPoint
    {
    public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public CatPoint(Point3D p)
        {
            X = p.X;
            Y = p.Y;
            Z = p.Z;
        }
    }
}