using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class TrickleMaker : MakerBase
    {

        private double radius;
        private double side;
        private double thickness;

        public TrickleMaker(double radius, double side, double thickness)
        {
            this.radius = radius;
            this.side = side;
            this.thickness = thickness;

        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Vertices = pnts;
            Faces = faces;



            List<Point> edge = new List<Point>();
            Point p1 = new Point(0, 0);
            Point p2 = new Point(side, 0);
            Point p3 = new Point(0, side);
            double ang;
            for (ang =  180; ang <= 270; ang ++)
            {
                double theta = ang * Math.PI / 180.0;
                double x = p1.X + (radius * Math.Cos(theta));
                double y = p1.Y + (radius * Math.Sin(theta));
                edge.Add(new Point(x,y));
            }

            for (ang = -90; ang <= 45; ang++)
            {
                double theta = ang * Math.PI / 180.0;
                double x = p2.X + (radius * Math.Cos(theta));
                double y = p2.Y + (radius * Math.Sin(theta));
                edge.Add(new Point(x, y));
            }

            for (ang = 45; ang < 180; ang++)
            {
                double theta = ang * Math.PI / 180.0;
                double x = p3.X + (radius * Math.Cos(theta));
                double y = p3.Y + (radius * Math.Sin(theta));
                edge.Add(new Point(x, y));
            }
            
            TriangulatePerimiter(edge, thickness);

            // do the side
            for (int i = 0; i < edge.Count; i ++)
            {
                int j = i + 1;
                if ( j == edge.Count)
                { 
                    j = 0;
                }
                int v0 = AddVertice(new Point3D(edge[i].X, 0, edge[i].Y));
                int v1 = AddVertice(new Point3D(edge[i].X, thickness, edge[i].Y));
                int v2 = AddVertice(new Point3D(edge[j].X, thickness, edge[j].Y));
                int v3 = AddVertice(new Point3D(edge[j].X, 0, edge[j].Y));

                Faces.Add(v0);
                Faces.Add(v1);
                Faces.Add(v2);

                Faces.Add(v0);
                Faces.Add(v2);
                Faces.Add(v3);
            }
        }
    }
}
