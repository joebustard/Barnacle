using Make3D.Dialogs.WireFrame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace Make3D.Dialogs.BezierSurface
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

        public ControlPoint[,] AllcontrolPoints { get { return allcontrolPoints; } }

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

        internal bool CheckHit(GeometryModel3D hitModel, bool shift, ref int selRow, ref int selColumn)
        {
            bool hit = false;
            selRow = -1;
            selColumn = -1;
            for (int r = 0; r < allcontrolPoints.GetLength(0); r++)
            {
                for (int c = 0; c < allcontrolPoints.GetLength(0); c++)
                {
                    allcontrolPoints[r, c].Selected = false;
                }
            }

            for (int r = 0; r < allcontrolPoints.GetLength(0) && hit == false; r++)
            {
                for (int c = 0; c < allcontrolPoints.GetLength(0) && hit == false; c++)
                {
                    hit = allcontrolPoints[r, c].CheckHit(hitModel, false);
                    if (hit)
                    {
                        selRow = r;
                        selColumn = c;
                    }
                }
            }
            return hit;
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

        private void GenerateWireFrames()
        {
            wireFrames = new List<WireFrameSegment>();
            for (int r = 0; r < patchRows - 1; r++)
            {
                for (int c = 0; c < patchColumns - 1; c++)
                {
                    WireFrameSegment seg = new WireFrameSegment(allcontrolPoints[r, c].Position, allcontrolPoints[r, c + 1].Position, 0.1);
                    wireFrames.Add(seg);
                    seg = new WireFrameSegment(allcontrolPoints[r, c].Position, allcontrolPoints[r + 1, c].Position, 0.1);
                    wireFrames.Add(seg);
                }
                WireFrameSegment seg2 = new WireFrameSegment(allcontrolPoints[r, patchColumns - 1].Position, allcontrolPoints[r + 1, patchColumns - 1].Position, 0.1);
                wireFrames.Add(seg2);
            }
            for (int c = 0; c < patchColumns - 1; c++)
            {
                WireFrameSegment seg2 = new WireFrameSegment(allcontrolPoints[patchRows - 1, c].Position, allcontrolPoints[patchRows - 1, c + 1].Position, 0.1);
                wireFrames.Add(seg2);
            }
        }
    }
}