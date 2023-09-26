using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class RoundGrilleMaker : MakerBase
    {
        private double edgeThickness;

        //  private double grilleHeight;
        private double grilleRadius;

        private double grilleWidth;
        private double horizontalBars;
        private double horizontalBarThickness;
        private bool makeEdge;
        private double verticalBars;
        private double verticalBarThickness;
        private double innerRadius;
        private double cx = 0.0;
        private double cy = 0.0;
        private double cz = 0.0;
        private double innerDiameter;

        public struct PolarPoint
        {
            public double theta;
            public double r;
            public double X;
            public double y;
            public bool vertical;
            public bool barId;
        }

        private List<PolarPoint> spokePoints;

        private void SortSpokesByTheta()
        {
            if (spokePoints != null && spokePoints.Count > 1)
            {
                bool swapped = true;
                while (swapped)
                {
                    swapped = false;
                    for (int i = 0; i < spokePoints.Count; i++)
                    {
                        if (spokePoints[i].theta > spokePoints[i + 1].theta)
                        {
                            swapped = true;
                            PolarPoint tmp = spokePoints[i];
                            spokePoints[i] = spokePoints[i + 1];
                            spokePoints[i + 1] = tmp;
                        }
                    }
                }
            }
        }

        private void MakeVerticalBarPolarCoords()
        {
            if (verticalBars > 0)
            {
                double vspacing = innerDiameter / (verticalBars + 1);
                for (int i = 0; i < verticalBars; i++)
                {
                    // work out where vertical bar crosses the x axies.
                    double vx = (cx - innerRadius) + (i * vspacing);
                    // distance of this from the centre
                    double vdx = Math.Abs(cx - vx);
                    double vdx1 = vdx - (verticalBarThickness / 2.0);
                    double vdx2 = vdx + (verticalBarThickness / 2.0);
                    double h1 = Math.Sqrt((innerRadius * innerRadius) - (vdx1 * vdx1));
                    double h2 = Math.Sqrt((innerRadius * innerRadius) - (vdx2 * vdx2));

                    // so we should have
                }
            }
        }

        public RoundGrilleMaker(double grilleRadius, double grillWidth, bool makeEdge, double edgeThickness, double verticalBars, double verticalBarThickness, double horizontalBars, double horizontalBarThickness)
        {
            this.grilleRadius = grilleRadius;
            //  this.grilleHeight = grillHeight;
            this.grilleWidth = grillWidth;
            this.makeEdge = makeEdge;
            this.edgeThickness = edgeThickness;
            this.verticalBars = verticalBars;
            this.verticalBarThickness = verticalBarThickness;
            this.horizontalBars = horizontalBars;
            this.horizontalBarThickness = horizontalBarThickness;
            this.innerRadius = grilleRadius - edgeThickness;
            this.innerDiameter = 2.0 * this.innerRadius;
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;

            if (makeEdge)
            {
                GenerateWithEdge();
            }
            else
            {
                GenerateWithoutEdge();
            }
        }

        private void Box(double x, double y, double z, double l, double h, double w, bool left, bool right, bool top, bool bottom)
        {
            // back
            int v0 = AddVertice(x, y, z);
            int v1 = AddVertice(x, y + h, z);
            int v2 = AddVertice(x + l, y + h, z);
            int v3 = AddVertice(x + l, y, z);
            AddFace(v0, v1, v2);
            AddFace(v0, v2, v3);

            // front
            int v4 = AddVertice(x, y, z + w);
            int v5 = AddVertice(x, y + h, z + w);
            int v6 = AddVertice(x + l, y + h, z + w);
            int v7 = AddVertice(x + l, y, z + w);
            AddFace(v4, v6, v5);
            AddFace(v4, v7, v6);
            if (left)
            {
                AddFace(v0, v5, v1);
                AddFace(v0, v4, v5);
            }

            if (right)
            {
                AddFace(v3, v2, v6);
                AddFace(v3, v6, v7);
            }

            if (top)
            {
                AddFace(v1, v6, v2);
                AddFace(v1, v5, v6);
            }

            if (bottom)
            {
                AddFace(v0, v3, v7);
                AddFace(v0, v7, v4);
            }
        }

        private void GenerateWithEdge()
        {
        }

        private void GenerateWithoutEdge()
        {
        }
    }
}