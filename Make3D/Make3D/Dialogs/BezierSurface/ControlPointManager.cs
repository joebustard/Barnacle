using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace Make3D.Dialogs.BezierSurface
{
    class ControlPointManager
    {
        const double XGap = 5;
        const double ZGap = 5;
        ControlPoint[,] allcontrolPoints;
        Patch[,] patches;
        public ControlPointManager()
        {

            patchRows = 4;
            patchColumns = 4;


            ResetControlPoints();
        }
        private int patchRows;
        public int PatchRows
        {
            get { return patchRows; }
            set
            {
                if (value != patchRows)
                {
                    patchRows = value;
                    ResetControlPoints();
                }
            }
        }

        private void ResetControlPoints()
        {
            allcontrolPoints = new ControlPoint[patchRows * 4, patchColumns * 4];

            patches = new Patch[patchRows, patchColumns];
            double xo = (patchColumns * 2) * XGap;
            double zo = (patchRows * 2) * ZGap;
            for (int r = 0; r < patchRows; r++)
            {
                for (int c = 0; c < patchColumns; c++)
                {
                    patches[r, c] = new Patch();
                    for (int r1 = 0; r1 < 4; r1++)
                    {
                        for (int C1 = 0; C1 < 4; C1++)
                        {
                            int row = (r * 4) + r1;
                            int col = (c * 4) + C1;
                            double x = col * XGap-xo;
                            double z = row * ZGap-zo;
                            allcontrolPoints[row, col] = new ControlPoint(x, 20, z);

                        }

                    }

                }

            }

        }

        private int patchColumns;
        public int PatchColumns
        {
            get { return patchColumns; }
            set
            {
                if (value != patchColumns)
                {
                    patchColumns = value;
                    ResetControlPoints();
                }
            }
        }

        internal bool CheckHit(GeometryModel3D hitModel, bool shift, ref int selRow, ref int selColumn)
        {
            bool hit = false;
            selRow = -1;
            selColumn = -1;
            for ( int r =0; r<allcontrolPoints.GetLength(0); r ++)
            {
                for (int c = 0; c < allcontrolPoints.GetLength(0); c++)
                {
                    allcontrolPoints[r, c].Selected = false;
                    hit = allcontrolPoints[r, c].CheckHit(hitModel, false);
                    if ( hit)
                    {
                        selRow = r;
                        selColumn = c;
                    }
                }

            }
            return hit;
        }

        public List<GeometryModel3D> Models 
        { 
        get
        {
                List<GeometryModel3D> res = new List<GeometryModel3D>();
                for ( int r =0; r < allcontrolPoints.GetLength(0); r ++)
                {
                    for (int c = 0; c < allcontrolPoints.GetLength(1); c++)
                    {
                        res.Add(allcontrolPoints[r, c].Model);
                    }

                }
                return res;
            }
        }

   
        internal void MovePoint(int r, int c, Point3D positionChange)
        {
            if ( r>=0 && r < allcontrolPoints.GetLength(0))
            {
                if (c >= 0 && c < allcontrolPoints.GetLength(1))
                {
                    allcontrolPoints[r, c].MovePosition(positionChange);
                }
            }
        }
    }
}
