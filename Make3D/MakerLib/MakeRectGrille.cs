using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class RectGrilleMaker : MakerBase
    {
        private double edgeThickness;
        private double grilleHeight;
        private double grilleLength;
        private double grilleWidth;
        private double horizontalBars;
        private double horizontalBarThickness;
        private bool makeEdge;
        private double verticalBars;
        private double verticalBarThickness;

        public RectGrilleMaker(double grillLength, double grillHeight, double grillWidth, bool makeEdge, double edgeThickness, double verticalBars, double verticalBarThickness, double horizontalBars, double horizontalBarThickness)
        {
            this.grilleLength = grillLength;
            this.grilleHeight = grillHeight;
            this.grilleWidth = grillWidth;
            this.makeEdge = makeEdge;
            this.edgeThickness = edgeThickness;
            this.verticalBars = verticalBars;
            this.verticalBarThickness = verticalBarThickness;
            this.horizontalBars = horizontalBars;
            this.horizontalBarThickness = horizontalBarThickness;
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

        private void FrameSides(double fl, double hOff, double vOff, double y, double fRight, double fTop)
        {
            double oldy = fl;
            double sy = 0;
            double dy = 0;
            for (int i = 1; i <= horizontalBars; i++)
            {
                y = fl + (i * hOff);
                sy = y - horizontalBarThickness / 2;
                Box(0, sy, 0, fl, horizontalBarThickness, grilleWidth, true, false, false, false);
                Box(fRight, sy, 0, fl, horizontalBarThickness, grilleWidth, false, true, false, false);

                MakeHorizontalCrossbars(fl, vOff, fRight, sy);

                dy = sy - oldy;
                Box(0, oldy, 0, fl, dy, grilleWidth, true, true, false, false);
                Box(fRight, oldy, 0, fl, dy, grilleWidth, true, true, false, false);

                oldy = y + horizontalBarThickness / 2;
            }
            dy = fTop - oldy;
            Box(0, oldy, 0, fl, dy, grilleWidth, true, true, false, false);
            Box(fRight, oldy, 0, fl, dy, grilleWidth, true, true, false, false);
        }

        private void FrameTopBottom(double fl, double hOff, double vOff, double x, double fRight, double fTop)
        {
            double oldx = fl;
            double sx = 0;
            double dx = 0;
            for (int i = 1; i <= verticalBars; i++)
            {
                x = fl + (i * vOff);
                sx = x - verticalBarThickness / 2;
                // add bottom and top box with inner facing hole
                Box(sx, 0, 0, verticalBarThickness, fl, grilleWidth, false, false, false, true);
                Box(sx, fTop, 0, verticalBarThickness, fl, grilleWidth, false, false, true, false);
                dx = sx - oldx;
                // fill in bottom and top gap with box closed on both sides
                Box(oldx, 0, 0, dx, fl, grilleWidth, false, false, true, true);
                Box(oldx, fTop, 0, dx, fl, grilleWidth, false, false, true, true);

                MakeVerticalBars(fl, hOff, fTop, oldx, sx, dx);

                oldx = x + verticalBarThickness / 2;
            }
            dx = fRight - oldx;
            Box(oldx, 0, 0, dx, fl, grilleWidth, false, false, true, true);
            Box(oldx, fTop, 0, dx, fl, grilleWidth, false, false, true, true);
        }

        private void GenerateWithEdge()
        {
            double fl = edgeThickness;

            double innerLength = grilleLength - (2 * fl);
            double innerHeight = grilleHeight - (2 * fl);
            double vOff = (innerLength) / (verticalBars + 1);
            double hOff = (innerHeight) / (horizontalBars + 1);

            // basic corners
            double x = 0;
            double y = 0;
            double fRight = grilleLength - fl;
            double fTop = grilleHeight - fl;
            MakeFrameCorners(fl, fRight, fTop);
            // left right frames
            FrameSides(fl, hOff, vOff, y, fRight, fTop);

            // top and bottom of frame
            FrameTopBottom(fl, hOff, vOff, x, fRight, fTop);
        }

        private void GenerateWithoutEdge()
        {
            double x;
            double y;

            double vOff = (grilleLength) / (verticalBars + 1);
            double hOff = (grilleHeight) / (horizontalBars + 1);
            double oldy = 0;
            double sx = 0;
            double sy = 0;
            double dy = 0;
            for (int i = 1; i <= horizontalBars; i++)
            {
                y = (i * hOff);
                sy = y - horizontalBarThickness / 2;

                double oldx = 0;

                double dx = 0;
                bool closeLeft = false;

                for (int j = 1; j <= verticalBars; j++)
                {
                    x = (j * vOff);
                    if (j == 1)
                    {
                        closeLeft = true;
                    }
                    else
                    {
                        closeLeft = false;
                    }

                    sx = x - verticalBarThickness / 2;
                    Box(sx, sy, 0, verticalBarThickness, horizontalBarThickness, grilleWidth, false, false, false, false);

                    dx = sx - oldx;
                    Box(oldx, sy, 0, dx, horizontalBarThickness, grilleWidth, closeLeft, false, true, true);

                    oldx = x + verticalBarThickness / 2;
                }
                dx = grilleLength - oldx;
                Box(oldx, sy, 0, dx, horizontalBarThickness, grilleWidth, false, true, true, true);

                dy = sy - oldy;

                oldy = y + horizontalBarThickness / 2;
            }

            // part 2
            for (int i = 1; i <= verticalBars; i++)
            {
                x = (i * vOff);
                sx = x - horizontalBarThickness / 2;

                oldy = 0;

                bool closeBottom = false;

                for (int j = 1; j <= horizontalBars; j++)
                {
                    y = (j * hOff);
                    if (j == 1)
                    {
                        closeBottom = true;
                    }
                    else
                    {
                        closeBottom = false;
                    }

                    sy = y - horizontalBarThickness / 2;

                    dy = sy - oldy;
                    Box(sx, oldy, 0, verticalBarThickness, dy, grilleWidth, true, true, false, closeBottom);

                    oldy = y + horizontalBarThickness / 2;
                }
                dy = grilleHeight - oldy;
                Box(sx, oldy, 0, verticalBarThickness, dy, grilleWidth, true, true, true, false);
            }
        }

        private void MakeFrameCorners(double fl, double fRight, double fTop)
        {
            Box(0, 0, 0, fl, fl, grilleWidth, true, false, false, true);
            Box(fRight, 0, 0, fl, fl, grilleWidth, false, true, false, true);
            Box(0, fTop, 0, fl, fl, grilleWidth, true, false, true, false);
            Box(fRight, fTop, 0, fl, fl, grilleWidth, false, true, true, false);
        }

        private void MakeHorizontalCrossbars(double fl, double vOff, double fRight, double sy)
        {
            double oldx = fl;
            double sx = 0;
            double dx = 0;
            double x;
            for (int j = 1; j <= verticalBars; j++)
            {
                x = fl + (j * vOff);
                sx = x - verticalBarThickness / 2;
                Box(sx, sy, 0, verticalBarThickness, horizontalBarThickness, grilleWidth, false, false, false, false);

                dx = sx - oldx;
                Box(oldx, sy, 0, dx, horizontalBarThickness, grilleWidth, false, false, true, true);

                oldx = x + verticalBarThickness / 2;
            }
            dx = fRight - oldx;
            Box(oldx, sy, 0, dx, horizontalBarThickness, grilleWidth, false, false, true, true);
        }

        private void MakeVerticalBars(double fl, double hOff, double fTop, double oldx, double sx, double dx)
        {
            double oldy = fl;
            double sy = 0;
            double dy = 0;
            double y;
            for (int j = 1; j <= horizontalBars; j++)
            {
                y = fl + (j * hOff);
                sy = y - horizontalBarThickness / 2;

                dy = sy - oldy;
                Box(sx, oldy, 0, verticalBarThickness, dy, grilleWidth, true, true, false, false);
                oldy = y + horizontalBarThickness / 2;
            }
            dy = fTop - oldy;

            Box(sx, oldy, 0, verticalBarThickness, dy, grilleWidth, true, true, false, false);
        }
    }
}