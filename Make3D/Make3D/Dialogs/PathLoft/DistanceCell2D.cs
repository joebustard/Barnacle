using asdflibrary;
using Barnacle.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Barnacle.Dialogs
{
    internal class DistanceCell2D
    {
        public static List<System.Drawing.PointF> AllPoints;

        public delegate float CalculateDistance(float x, float y);

        public int[] points;
        public float[] values;
        public DistanceCell2D[] SubCells;

        public const int TopLeft = 0;
        public const int TopRight = 1;
        public const int BottomLeft = 2;
        public const int BottomRight = 3;
        public const int Centre = 4;
        public static CalculateDistance OnCalculateDistance = null;
        private static CubeMarcher cubeMarcher;
        private static GridCell gc = new GridCell();

        public DistanceCell2D()
        {
            points = new int[5];
            values = new float[5];
            SubCells = null;
        }

        public void InitialisePoints()
        {
            AllPoints = new List<System.Drawing.PointF>();
            cubeMarcher = new CubeMarcher();
            gc = new GridCell();
        }

        public void SetPoint(int loc, float x, float y, float v)
        {
            int l = (int)loc;
            AllPoints.Add(new System.Drawing.PointF(x, y));
            points[l] = AllPoints.Count - 1;
            values[l] = v;
        }

        public void CalcPoint(int loc, float x, float y)
        {
            int l = (int)loc;
            AllPoints.Add(new System.Drawing.PointF(x, y));
            points[l] = AllPoints.Count - 1;
            values[l] = OnCalculateDistance(x, y);
        }

        public void SetPoint(int loc, int pointIndex, float v)
        {
            int l = (int)loc;
            points[l] = pointIndex;
            values[l] = v;
        }

        public void SetCentre()
        {
            AllPoints.Add(new System.Drawing.PointF(AllPoints[points[0]].X + (AllPoints[points[3]].X - AllPoints[points[0]].X) / 2,
                                     AllPoints[points[0]].Y + (AllPoints[points[3]].Y - AllPoints[points[0]].Y) / 2));
            points[4] = AllPoints.Count - 1;
            if (OnCalculateDistance != null)
            {
                values[4] = OnCalculateDistance(AllPoints[points[4]].X, AllPoints[points[4]].Y);
            }
        }

        /// <summary>
        /// splitValues 0, top, 1, right, 2, bottom, 3 left
        /// </summary>

        public void CreateSubCells()
        {
            SubCells = new DistanceCell2D[4];

            SubCells[TopLeft] = new DistanceCell2D();
            SubCells[TopLeft].SetPoint(TopLeft, points[TopLeft], values[TopLeft]);
            SubCells[TopLeft].CalcPoint(TopRight, AllPoints[points[Centre]].X, AllPoints[points[TopLeft]].Y);
            SubCells[TopLeft].SetPoint(BottomRight, points[Centre], values[Centre]);
            SubCells[TopLeft].CalcPoint(BottomLeft, AllPoints[points[TopLeft]].X, AllPoints[points[Centre]].Y);
            SubCells[TopLeft].SetCentre();

            SubCells[TopRight] = new DistanceCell2D();
            SubCells[TopRight].CalcPoint(TopLeft, AllPoints[points[Centre]].X, AllPoints[points[TopLeft]].Y);
            SubCells[TopRight].SetPoint(TopRight, points[TopRight], values[TopRight]);
            SubCells[TopRight].CalcPoint(BottomRight, AllPoints[points[TopRight]].X, AllPoints[points[Centre]].Y);
            SubCells[TopRight].SetPoint(BottomLeft, points[Centre], values[Centre]);
            SubCells[TopRight].SetCentre();

            SubCells[BottomLeft] = new DistanceCell2D();
            SubCells[BottomLeft].CalcPoint(TopLeft, AllPoints[points[BottomLeft]].X, AllPoints[points[Centre]].Y);
            SubCells[BottomLeft].SetPoint(TopRight, points[Centre], values[Centre]);
            SubCells[BottomLeft].CalcPoint(BottomRight, AllPoints[points[Centre]].X, AllPoints[points[BottomLeft]].Y);
            SubCells[BottomLeft].SetPoint(BottomLeft, points[BottomLeft], values[BottomLeft]);
            SubCells[BottomLeft].SetCentre();

            SubCells[BottomRight] = new DistanceCell2D();
            SubCells[BottomRight].SetPoint(TopLeft, points[Centre], values[Centre]);
            SubCells[BottomRight].CalcPoint(TopRight, AllPoints[points[TopRight]].X, AllPoints[points[Centre]].Y);
            SubCells[BottomRight].SetPoint(BottomRight, points[BottomRight], values[BottomRight]);
            SubCells[BottomRight].CalcPoint(BottomLeft, AllPoints[points[Centre]].X, AllPoints[points[BottomRight]].Y);
            SubCells[BottomRight].SetCentre();
        }

        public void Dump(string indent = "")
        {
            Debug($"{indent}=====");
            Debug($"{indent}Tl {AllPoints[points[TopLeft]].X},{AllPoints[points[TopLeft]].Y}={values[TopLeft]}  TR {AllPoints[points[TopRight]].X},{AllPoints[points[TopRight]].Y}={values[TopRight]} ");
            Debug($"{indent}Ce {AllPoints[points[Centre]].X},{AllPoints[points[Centre]].Y}={values[Centre]} ");
            Debug($"{indent}BL {AllPoints[points[BottomLeft]].X},{AllPoints[points[BottomLeft]].Y}={values[BottomLeft]}  BR {AllPoints[points[BottomRight]].X},{AllPoints[points[BottomRight]].Y}={values[BottomRight]} ");
            if (SubCells != null)
            {
                SubCells[TopLeft].Dump(indent + " ");
                SubCells[TopRight].Dump(indent + " ");
                SubCells[BottomLeft].Dump(indent + " ");
                SubCells[BottomRight].Dump(indent + " ");
            }
        }

        public void Debug(string txt, [CallerMemberName] string caller = "")
        {
            System.Diagnostics.Debug.WriteLine($" {caller}: {txt}");
        }

        internal void CreateSubCells(int cellId)
        {
            if (SubCells != null)
            {
                SubCells[cellId].CreateSubCells();
            }
        }

        internal double Size()
        {
            /*
                System.Drawing.PointF p1 = AllPoints[points[TopLeft]];
                System.Drawing.PointF p2 = AllPoints[points[BottomRight]];

                double res = Math.Sqrt( (p1.X - p2.X) * (p1.X - p2.X) +
                                        (p1.Y - p2.Y) * (p1.Y - p2.Y));

                //Debug($"p1 {p1.X},{p1.Y} p2 {p2.X},{p2.Y} res {res}");
                return res;
                */
            return AllPoints[points[BottomRight]].X - AllPoints[points[TopLeft]].X;
        }

        internal void AdjustValues(float th)
        {
            values[0] -= th;
            values[1] -= th;
            values[2] -= th;
            values[3] -= th;
            values[4] -= th;
            if (SubCells != null && SubCells.GetLength(0) == 4)
            {
                SubCells[0].AdjustValues(th);
                SubCells[1].AdjustValues(th);
                SubCells[2].AdjustValues(th);
                SubCells[3].AdjustValues(th);
            }
        }

        public void GenerateWalls(List<Triangle> triangles)
        {
            if (SubCells != null && SubCells.GetLength(0) > 0)
            {
                SubCells[0].GenerateWalls(triangles);
                SubCells[1].GenerateWalls(triangles);
                SubCells[2].GenerateWalls(triangles);
                SubCells[3].GenerateWalls(triangles);
            }
            else
            {
                System.Drawing.PointF p1 = AllPoints[points[TopLeft]];
                System.Drawing.PointF p2 = AllPoints[points[BottomRight]];
                gc.p[0] = new XYZ(p1.X, 0, p1.Y);
                gc.p[1] = new XYZ(p2.X, 0, p1.Y);
                gc.p[2] = new XYZ(p2.X, 0, p2.Y);
                gc.p[3] = new XYZ(p1.X, 0, p2.Y);

                gc.val[0] = values[TopLeft];
                gc.val[1] = values[TopRight];
                gc.val[2] = values[BottomRight];
                gc.val[3] = values[BottomLeft];

                gc.p[4] = new XYZ(p1.X, 1, p1.Y);
                gc.p[5] = new XYZ(p2.X, 1, p1.Y);
                gc.p[6] = new XYZ(p2.X, 1, p2.Y);
                gc.p[7] = new XYZ(p1.X, 1, p2.Y);

                gc.val[4] = values[TopLeft];
                gc.val[5] = values[TopRight];
                gc.val[6] = values[BottomRight];
                gc.val[7] = values[BottomLeft];
                cubeMarcher.Polygonise(gc, 0.0, triangles);
                /*
                gc.p[0] = new XYZ(p1.X, -0.1, p1.Y);
                gc.p[1] = new XYZ(p2.X, -0.1, p1.Y);
                gc.p[2] = new XYZ(p2.X, -0.1, p2.Y);
                gc.p[3] = new XYZ(p1.X, -0.1, p2.Y);

                gc.val[0] = values[TopLeft] + 0.1;
                gc.val[1] = values[TopRight] + 0.1;
                gc.val[2] = values[BottomRight] + 0.1;
                gc.val[3] = values[BottomLeft] + 0.1;

                gc.p[4] = new XYZ(p1.X, 0.1, p1.Y);
                gc.p[5] = new XYZ(p2.X, 0.1, p1.Y);
                gc.p[6] = new XYZ(p2.X, 0.1, p2.Y);
                gc.p[7] = new XYZ(p1.X, 0.1, p2.Y);

                gc.val[4] = values[TopLeft] + 0.1;
                gc.val[5] = values[TopRight] + 0.1;
                gc.val[6] = values[BottomRight] + 0.1;
                gc.val[7] = values[BottomLeft] + 0.1;
                cubeMarcher.Polygonise(gc, 0.0, triangles);
                */
            }
        }

        internal bool InterceptedBy(Point point1, Point point2)
        {
            bool intercept = false;
            // if both points are to left of box, no intercept
            if (point1.X < AllPoints[points[TopLeft]].X && point2.X < AllPoints[points[TopLeft]].X)
            {
                intercept = false;
            }
            else
            if (point1.X > AllPoints[points[TopRight]].X && point2.X > AllPoints[points[TopRight]].X)
            {
                // both to right of box
                intercept = false;
            }
            else
            if (point1.Y > AllPoints[points[TopLeft]].Y && point2.Y > AllPoints[points[TopLeft]].Y)
            {
                intercept = false;
            }
            else
            if (point1.Y < AllPoints[points[BottomLeft]].Y && point2.Y < AllPoints[points[BottomLeft]].Y)
            {
                intercept = false;
            }
            else
            {
                System.Drawing.PointF p1 = new System.Drawing.PointF((float)point1.X, (float)point1.Y);
                System.Drawing.PointF p2 = new System.Drawing.PointF((float)point2.X, (float)point2.Y);
                if (LineUtils.DoSegmentsIntersect(AllPoints[points[TopLeft]], AllPoints[points[TopRight]], p1, p2))
                {
                    intercept = true;
                }
                else
                     if (LineUtils.DoSegmentsIntersect(AllPoints[points[BottomLeft]], AllPoints[points[BottomRight]], p1, p2))
                {
                    intercept = true;
                }
                else
                     if (LineUtils.DoSegmentsIntersect(AllPoints[points[TopLeft]], AllPoints[points[BottomLeft]], p1, p2))
                {
                    intercept = true;
                }
                else
                     if (LineUtils.DoSegmentsIntersect(AllPoints[points[TopRight]], AllPoints[points[BottomRight]], p1, p2))
                {
                    intercept = true;
                }
            }
            return intercept;
        }

        internal bool Contains(double x, double y)
        {
            if ((float)x >= AllPoints[points[TopLeft]].X)
            {
                if ((float)x <= AllPoints[points[TopRight]].X)
                {
                    if ((float)y <= AllPoints[points[TopLeft]].Y)
                    {
                        if ((float)y >= AllPoints[points[BottomRight]].Y)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}