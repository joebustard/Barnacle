using Barnacle.LineLib;
using PolygonTriangulationLib;
using System;
using System.Collections.Generic;

namespace Barnacle.Dialogs
{
    public class LinkPart
    {
        private FlexiPath flexipath;
        private List<System.Drawing.PointF> profile;
        private List<Triangle> triangles;

        public LinkPart()
        {
            PathText = "";
            profile = null;
            triangles = null;
        }

        public String Name { get; set; }
        public String PathText { get; set; }

        public List<System.Drawing.PointF> Profile
        {
            get
            {
                if (profile == null)
                {
                    if (PathText != "")
                    {
                        FlexiPath fp = new FlexiPath();
                        if (fp.FromTextPath(PathText))
                        {
                            profile = fp.DisplayPointsF();
                            GenerateTriangles();
                        }
                    }
                }
                return profile;
            }
        }

        public List<Triangle> Triangles
        {
            get { return triangles; }
        }

        public double W { get; set; }

        public double X { get; set; }

        public double Y { get; set; }

        public double Z { get; set; }

        private void GenerateTriangles()
        {
            TriangulationPolygon ply = new TriangulationPolygon();
            // ply.Points = pf.ToArray();
            ply.Points = profile.ToArray();
            triangles = ply.Triangulate();
        }
    }
}