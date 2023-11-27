using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using static PrintPlacementLib.BedSpiralor;

namespace PrintPlacementLib
{
    public class PrintPlacement
    {
        public List<Component> components;
        public List<Component> Results;
        private int bedCols;
        private double bedHeight;
        private int bedRows;
        private double bedWidth;
        private BedMap overallPlacement;
        private double workingClearance;

        public PrintPlacement()
        {
            Clearance = 2;
            SetBedSize(200, 200);
            components = new List<Component>();
            Results = new List<Component>(); ;
        }

        public double BedHeight { get { return bedHeight; } }
        public double BedWidth { get { return bedWidth; } }
        private double clearance;
        public double Clearance
        {
            get { return clearance; }

            set
            {
                clearance = value;
                if (components != null)
                {
                    foreach (Component cm in components)
                    {
                        cm.Clearance = clearance/2;
                    }
                }
            }
        }

        public void AddComponent(Object3D ob, Point3D point1, Point3D point2)
        {
            Component comp = new Component();
            comp.Shape = ob;
            comp.LowBound = point1;
            comp.HighBound = point2;
            comp.Clearance = workingClearance;

            comp.OriginalPosition = ob.Position;
            components.Add(comp);
        }

        public void Arrange()
        {
            DateTime st = DateTime.Now;
            // clear the floor map
            for (int r = 0; r < bedRows; r++)
            {
                for (int c = 0; c < bedCols; c++)
                {
                    overallPlacement.Set(r, c, false);
                }
            }

            // create the bed maps
            foreach (Component cm in components)
            {

                cm.SetMap();
            }

            // sort so we have the densist object first
            components = components.OrderByDescending(x => x.Density).ToList();
            Bounds3D bedBounds = new Bounds3D();
            bedBounds.Lower = new Point3D(bedCols / 2, 0, bedRows / 2);
            bedBounds.Upper = new Point3D(bedCols / 2, 0, bedRows / 2);
            //Place each one
            foreach (Component cm in components)
            {
                cm.SetTarget(bedWidth / 2, bedHeight / 2);
                double bestScore = double.MaxValue;
                BedSpiralor spiro = new BedSpiralor(bedRows, bedCols);

                bool done = false;
                MapPoint bestPos = new MapPoint(0, 0);
                while (spiro.numberVisited < spiro.MaxVisited && !done)
                {
                    MapPoint mapPoint = spiro.GetNextPos();

                    double testScore = cm.Score(mapPoint.Row, mapPoint.Column, bedRows, bedCols, bedBounds);

                    if (testScore < bestScore)
                    {
                        // test if we can actually
                        bool add = true;
                        for (int r = 0; r < cm.Rows && add; r++)
                        {
                            for (int c = 0; c < cm.Columns && add; c++)
                            {
                                if (cm.Map.Get(r, c))
                                {
                                    int tr = (r - cm.Rows / 2) + mapPoint.Row;
                                    int tc = (c - cm.Columns / 2) + mapPoint.Column;
                                    if (tr >= 0 && tr < bedRows && tc >= 0 && tc < bedCols)
                                    {
                                        if (overallPlacement.Get(tr, tc) == true)
                                        {
                                            add = false;
                                        }
                                    }
                                }
                            }
                        }
                        if (add)
                        {
                            bestPos = mapPoint;
                            bestScore = testScore;
                        }
                    }
                }
                cm.Position = new Point3D((bestPos.Column * workingClearance) - (bedWidth / 2), 0, (bestPos.Row * workingClearance) - (bedHeight / 2));
                Results.Add(cm);
                cm.ExpandMap();

                // mark the bed map with the object map to block that area out
                for (int r = 0; r < cm.Rows; r++)
                {
                    for (int c = 0; c < cm.Columns; c++)
                    {
                        if (cm.Map.Get(r, c))
                        {
                            int tr = (r - cm.Rows / 2) + bestPos.Row;
                            int tc = (c - cm.Columns / 2) + bestPos.Column;
                            if (tr >= 0 && tr < bedRows && tc >= 0 && tc < bedCols)
                            {
                                overallPlacement.Set(tr, tc, true);
                                bedBounds.Adjust(new Point3D(tc, 0, tr));
                            }
                        }
                    }
                }
             //   overallPlacement.Dump("Updated Placement");
            }
            DateTime et = DateTime.Now;
            TimeSpan ts = et - st;
            System.Diagnostics.Debug.WriteLine($"Arrange took {ts.TotalSeconds} seconds");
        }

        public void SetBedSize(double width, double height)
        {
            bedWidth = width;
            bedHeight = height;
            if (Clearance > 0)
            {
                workingClearance = Clearance / 2.0;
                bedRows = (int)(bedHeight / workingClearance);
                bedCols = (int)(bedWidth / workingClearance);
                overallPlacement = new BedMap(bedRows, bedCols);
            }
        }

        private void Log(string s)
        {
            System.Diagnostics.Debug.WriteLine(s);
        }
    }
}