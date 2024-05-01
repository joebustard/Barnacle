using Barnacle.Dialogs.WireFrame;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs.BezierSurface
{
    internal class ControlPointManager
    {
        private const double XGap = 5;
        private const double ZGap = 5;
        private ControlPoint[,] allcontrolPoints;
        private int patchColumns;
        private int patchRows;
        private List<WireFrameSegment> wireFrames;

        public ControlPointManager()
        {
            patchRows = 13;
            patchColumns = 13;
            ResetControlPoints();
        }

        public void SetDimensions(int rows, int cols)
        {
            patchRows = rows;
            patchColumns = cols;
            ResetControlPoints();
        }

        public ControlPoint[,] AllcontrolPoints
        { get { return allcontrolPoints; } }

        public List<GeometryModel3D> Models
        {
            get
            {
                List<GeometryModel3D> res = new List<GeometryModel3D>();
                foreach (WireFrameSegment wf in wireFrames)
                {
                    res.Add(wf.Model);
                }
                for (int r = 0; r < allcontrolPoints.GetLength(0); r++)
                {
                    for (int c = 0; c < allcontrolPoints.GetLength(1); c++)
                    {
                        res.Add(allcontrolPoints[r, c].Model);
                    }
                }

                return res;
            }
        }

        public int PatchColumns
        {
            get
            {
                return patchColumns;
            }

            set
            {
                if (value != patchColumns)
                {
                    patchColumns = value;
                    ResetControlPoints();
                }
            }
        }

        public int PatchRows
        {
            get
            {
                return patchRows;
            }

            set
            {
                if (value != patchRows)
                {
                    patchRows = value;
                    ResetControlPoints();
                }
            }
        }

        public void GenerateWireFrames()
        {
            wireFrames = new List<WireFrameSegment>();
            for (int r = 0; r < patchRows - 1; r++)
            {
                for (int c = 0; c < patchColumns - 1; c++)
                {
                    WireFrameSegment seg = new WireFrameSegment(allcontrolPoints[r, c].Position, allcontrolPoints[r, c + 1].Position, 0.1, Colors.Black);
                    wireFrames.Add(seg);
                    seg = new WireFrameSegment(allcontrolPoints[r, c].Position, allcontrolPoints[r + 1, c].Position, 0.1, Colors.Black);
                    wireFrames.Add(seg);
                }
                WireFrameSegment seg2 = new WireFrameSegment(allcontrolPoints[r, patchColumns - 1].Position, allcontrolPoints[r + 1, patchColumns - 1].Position, 0.1, Colors.Black);
                wireFrames.Add(seg2);
            }
            for (int c = 0; c < patchColumns - 1; c++)
            {
                WireFrameSegment seg2 = new WireFrameSegment(allcontrolPoints[patchRows - 1, c].Position, allcontrolPoints[patchRows - 1, c + 1].Position, 0.1, Colors.Black);
                wireFrames.Add(seg2);
            }
        }

        public void ResetControlPoints()
        {
            allcontrolPoints = new ControlPoint[patchRows, patchColumns];

            double xo = (patchColumns / 2) * XGap;
            double zo = (patchRows / 2) * ZGap;
            for (int r = 0; r < patchRows; r++)
            {
                for (int c = 0; c < patchColumns; c++)
                {
                    double x = c * XGap - xo;
                    double z = r * ZGap - zo;
                    allcontrolPoints[r, c] = new ControlPoint(x, 20, z);
                }
            }
            GenerateWireFrames();
        }

        public void ResetControlPointsHalfTube()
        {
            allcontrolPoints = new ControlPoint[patchRows, patchColumns];

            double xo = (patchColumns / 2) * XGap;
            double zo = (patchRows / 2) * ZGap;
            for (int r = 0; r < patchRows; r++)
            {
                for (int c = 0; c < patchColumns; c++)
                {
                    double x = c * XGap - xo;
                    double z = r * ZGap - zo;

                    double y = 20;
                    if (c == 0 || c == patchColumns - 1)
                    {
                        y = 0;
                    }
                    if (c == 1 || c == patchColumns - 2)
                    {
                        y = 3;
                    }
                    if (c == 2 || c == patchColumns - 3)
                    {
                        y = 6;
                    }
                    if (c == 3 || c == patchColumns - 4)
                    {
                        y = 10;
                    }
                    allcontrolPoints[r, c] = new ControlPoint(x, y, z);
                }
            }
            GenerateWireFrames();
        }

        public void ResetControlPointsBow()
        {
            allcontrolPoints = new ControlPoint[patchRows, patchColumns];

            double centerX = ((double)patchColumns / 2.0);
            double centerZ = ((double)patchRows / 2.0);
            int offset = 0;
            while (offset < centerX)
            {
                for (int r = offset; r < patchRows - offset; r++)
                {
                    double dist = Math.Abs(centerZ - r) * ZGap;
                    for (int c = offset; c < patchColumns - offset; c++)
                    {
                        double angle = Math.Atan2((centerZ - r), (centerX - c));
                        double x = (dist * Math.Cos(angle)) + centerX;
                        double z = (dist * Math.Sin(angle)) + centerZ;
                        allcontrolPoints[r, c] = new ControlPoint(x, 20, z);
                    }
                }
                offset++;
            }
            GenerateWireFrames();
        }

        public void ResetControlPointsCircle()
        {
            allcontrolPoints = new ControlPoint[patchRows, patchColumns];

            double centerX = ((double)patchColumns / 2.0);
            double centerZ = ((double)patchRows / 2.0);
            int offset = 0;
            while (offset <= centerX)
            {
                double dist = Math.Abs(centerX - offset) * XGap;
                for (int r = offset; r < patchRows - offset; r++)
                {
                    for (int c = offset; c < patchColumns - offset; c++)
                    {
                        double angle = Math.Atan2((centerZ - r), (centerX - c));
                        double x = (dist * Math.Cos(angle)) + centerX;
                        double z = (dist * Math.Sin(angle)) + centerZ;
                        allcontrolPoints[r, c] = new ControlPoint(x, 20, z);
                    }
                }
                offset++;
            }

            GenerateWireFrames();
        }

        internal void FloorPoints()
        {
            for (int r = 0; r < patchRows; r++)
            {
                for (int c = 0; c < patchColumns; c++)
                {
                    if (allcontrolPoints[r, c].Selected)
                    {
                        allcontrolPoints[r, c].Position = new Point3D(
                        allcontrolPoints[r, c].Position.X,
                        0.0,
                        allcontrolPoints[r, c].Position.Z
                        );
                    }

                    allcontrolPoints[r, c].MoveControlMarker();
                }
            }
            GenerateWireFrames();
        }

        public override string ToString()
        {
            string res = "";
            for (int r = 0; r < patchRows; r++)
            {
                for (int c = 0; c < patchColumns; c++)
                {
                    res += allcontrolPoints[r, c].Position.X + " ";
                    res += allcontrolPoints[r, c].Position.Y + " ";
                    res += allcontrolPoints[r, c].Position.Z + " ";
                }
            }
            return res;
        }

        internal bool CheckHit(GeometryModel3D hitModel, bool shift, ref bool alreadySelected, ref int selectedRow, ref int selectedCol)
        {
            bool hit = false;
            alreadySelected = false;
            selectedCol = -1;
            selectedRow = -1;
            for (int r = 0; r < patchRows && hit == false; r++)
            {

                for (int c = 0; c < patchColumns && hit == false; c++)
                {
                    hit = allcontrolPoints[r, c].CheckHit(hitModel);
                    if (hit)
                    {
                        alreadySelected = allcontrolPoints[r, c].Selected;

                        allcontrolPoints[r, c].Selected = true;
                        selectedCol = c;
                        selectedRow = r;
                    }
                }
            }
            return hit;
        }


        public void DeselectAll()
        {
            for (int r = 0; r < allcontrolPoints.GetLength(0); r++)
            {
                for (int c = 0; c < allcontrolPoints.GetLength(0); c++)
                {
                    allcontrolPoints[r, c].Selected = false;
                }
            }
        }

        internal void MovePoint(int r, int c, Point3D positionChange)
        {
            if (r >= 0 && r < allcontrolPoints.GetLength(0))
            {
                if (c >= 0 && c < allcontrolPoints.GetLength(1))
                {
                    allcontrolPoints[r, c].MovePosition(positionChange);
                    GenerateWireFrames();
                }
            }
        }

        internal void MovePointWithoutWireframes(int r, int c, Point3D positionChange)
        {
            if (r >= 0 && r < allcontrolPoints.GetLength(0))
            {
                if (c >= 0 && c < allcontrolPoints.GetLength(1))
                {
                    allcontrolPoints[r, c].MovePosition(positionChange);
                }
            }
        }

        internal void UpDiagPoints(int sr, int sc, int er, int ec, int delta)
        {
            if (sr < er)
            {
                for (int r = sr; r < er; r++)
                {
                    for (int c = sc; c < ec; c++)
                    {
                        double diff = c - r;
                        double v1 = Math.Abs(diff) * delta;
                        Point3D mv = new Point3D(0, v1, 0);
                        if (r >= 0 && r < PatchRows && c >= 0 && c < PatchColumns)
                        {
                            allcontrolPoints[r, c].MovePosition(mv);
                        }
                    }
                }
            }
            else
            {
                for (int r = sr; r >= er; r--)
                {
                    for (int c = sc; c < ec; c++)
                    {
                        double diff = (patchColumns - c) - r;
                        double v1 = Math.Abs(diff) * delta;
                        Point3D mv = new Point3D(0, v1, 0);
                        if (r >= 0 && r < PatchRows && c >= 0 && c < PatchColumns)
                        {
                            allcontrolPoints[r, c].MovePosition(mv);
                        }
                    }
                }
            }
        }

        internal void UpXPoints(int v)
        {
            int mid = patchRows / 2;

            for (int r = 0; r < patchRows; r++)
            {
                int diff = r - mid;
                double v1 = Math.Abs(diff) * v;
                Point3D mv = new Point3D(0, v1, 0);
                for (int c = 0; c < patchColumns; c++)
                {
                    allcontrolPoints[r, c].MovePosition(mv);
                }
            }
        }

        internal void UpZPoints(double v)
        {
            int mid = patchColumns / 2;

            for (int c = 0; c < patchColumns; c++)
            {
                int diff = c - mid;
                double v1 = Math.Abs(diff) * v;
                Point3D mv = new Point3D(0, v1, 0);
                for (int r = 0; r < patchRows; r++)
                {
                    allcontrolPoints[r, c].MovePosition(mv);
                }
            }
        }

        internal void Select(int r, int c)
        {
            if (r >= 0 &&
                r < patchRows &&
                c >= 0 &&
                c < patchColumns)
            {
                allcontrolPoints[r, c].Selected = true;
            }
        }

        internal void SetPointPos(int r, int c, double x, double y, double z)
        {
            if (r >= 0 &&
                 r < patchRows &&
                 c >= 0 &&
                 c < patchColumns)
            {
                allcontrolPoints[r, c] = new ControlPoint(x, y, z);
            }
        }

        internal void MoveSelectedPoints(Point3D positionChange)
        {
            for (int r = 0; r < patchRows; r++)
            {
                for (int c = 0; c < patchColumns; c++)
                {
                    if (allcontrolPoints[r, c].Selected)
                    {
                        MovePointWithoutWireframes(r, c, positionChange);
                    }
                }
            }
            GenerateWireFrames();
        }
    }
}