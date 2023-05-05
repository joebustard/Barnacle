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
        public Point[] points;
        public float[] values;
        public DistanceCell2D[] SubCells;

        public const int TopLeft = 0;
        public const int TopRight = 1;
        public const int BottomLeft = 2;
        public const int BottomRight = 3;
        public const int Centre = 4;

        public DistanceCell2D()
        {
            points = new System.Windows.Point[5];
            values = new float[5];
            SubCells = null;
        }

        public void SetPoint(int loc, Point p, float val)
        {
            int l = (int)loc;
            points[l] = new Point(p.X, p.Y);
            values[l] = val;
        }

        public void SetCentre(int v)
        {
            points[4] = new Point(points[0].X + (points[3].X - points[0].X) / 2,
                                  points[0].Y + (points[3].Y - points[0].Y) / 2);
            values[4] = v;
        }

        /// <summary>
        /// splitValues 0, top, 1, right, 2, bottom, 3 left
        /// </summary>
        /// <param name="splitValues"></param>
        public void CreateSubCells(float[] splitValues)
        {
            SubCells = new DistanceCell2D[4];

            SubCells[TopLeft] = new DistanceCell2D();
            SubCells[TopLeft].SetPoint(TopLeft, points[TopLeft], values[TopLeft]);
            SubCells[TopLeft].SetPoint(TopRight, new Point(points[Centre].X, points[TopLeft].Y), splitValues[0]);
            SubCells[TopLeft].SetPoint(BottomRight, points[Centre], values[Centre]);
            SubCells[TopLeft].SetPoint(BottomLeft, new Point(points[TopLeft].X, points[Centre].Y), splitValues[3]);

            SubCells[TopRight] = new DistanceCell2D();
            SubCells[TopRight].SetPoint(TopLeft, new Point(points[Centre].X, points[TopLeft].Y), splitValues[0]);
            SubCells[TopRight].SetPoint(TopRight, points[TopRight], values[TopRight]);
            SubCells[TopRight].SetPoint(BottomRight, new Point(points[TopRight].X, points[Centre].Y), splitValues[1]);
            SubCells[TopRight].SetPoint(BottomLeft, points[Centre], values[Centre]);

            SubCells[BottomLeft] = new DistanceCell2D();
            SubCells[BottomLeft].SetPoint(TopLeft, new Point(points[BottomLeft].X, points[Centre].Y), splitValues[3]);
            SubCells[BottomLeft].SetPoint(TopRight, new Point(points[Centre].X, points[Centre].Y), values[Centre]);
            SubCells[BottomLeft].SetPoint(BottomRight, new Point(points[Centre].X, points[BottomLeft].Y), splitValues[2]);
            SubCells[BottomLeft].SetPoint(BottomLeft, new Point(points[BottomLeft].X, points[BottomLeft].Y), values[BottomLeft]);

            SubCells[BottomRight] = new DistanceCell2D();
            SubCells[BottomRight].SetPoint(TopLeft, new Point(points[Centre].X, points[Centre].Y), values[Centre]);
            SubCells[BottomRight].SetPoint(TopRight, new Point(points[TopRight].X, points[Centre].Y), splitValues[1]);
            SubCells[BottomRight].SetPoint(BottomRight, new Point(points[BottomRight].X, points[BottomRight].Y), values[BottomRight]);
            SubCells[BottomRight].SetPoint(BottomLeft, new Point(points[Centre].X, points[BottomRight].Y), splitValues[2]);
        }

        public void Dump(string indent = "")
        {
            Debug($"{indent}=====");
            Debug($"{indent}Tl {points[TopLeft].X},{points[TopLeft].Y}={values[TopLeft]}  TR {points[TopRight].X},{points[TopRight].Y}={values[TopRight]} ");
            Debug($"{indent}Ce {points[Centre].X},{points[Centre].Y}={values[Centre]} ");
            Debug($"{indent}BL {points[BottomLeft].X},{points[BottomLeft].Y}={values[BottomLeft]}  BR {points[BottomRight].X},{points[BottomRight].Y}={values[BottomRight]} ");
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

        internal void CreateSubCells(int cellId, float[] testVals)
        {
            if (SubCells != null)
            {
                SubCells[cellId].CreateSubCells(testVals);
            }
        }
    }
}