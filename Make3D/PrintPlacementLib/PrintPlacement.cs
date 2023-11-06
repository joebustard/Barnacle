using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using static PrintPlacementLib.BedSpiralor;

namespace PrintPlacementLib
{
    public class PrintPlacement
    {
        private double bedWidth;
        public double BedWidth { get { return bedWidth; } }
        private double bedHeight;
        public double BedHeight { get { return bedHeight; } }
        public List<Component> components;
        private BedMap overallPlacement;
        private int bedRows;
        private int bedCols;
        public List<Component> Results;

        public PrintPlacement()
        {
            Clearance = 3;
            SetBedSize(200, 200);
            components = new List<Component>();
            Results = new List<Component>(); ;
        }

        public double Clearance { get; set; }
        private double workingCLearance;

        public void SetBedSize(double width, double height)
        {
            bedWidth = width;
            bedHeight = height;
            if (Clearance > 0)
            {
                workingCLearance = Clearance / 2.0;
                bedRows = (int)(bedHeight / workingCLearance);
                bedCols = (int)(bedWidth / workingCLearance);
                overallPlacement = new BedMap(bedRows, bedCols);
            }
        }

        private void Log(string s)
        {
            System.Diagnostics.Debug.WriteLine(s);
        }

        public void Arrange()
        {
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

            //Place each one
            foreach (Component cm in components)
            {
                Log($"SetTarget {bedWidth / 2}, {bedHeight / 2}");
                cm.SetTarget(bedWidth / 2, bedHeight / 2);
                double bestScore = double.MaxValue;
                BedSpiralor spiro = new BedSpiralor(bedRows, bedCols);
                bool done = false;
                MapPoint bestPos = new MapPoint(0, 0);
                while (spiro.numberVisited < spiro.MaxVisited && !done)
                {
                    MapPoint mapPoint = spiro.GetNextPos();
                    //       Log($"spiro mapPoint r {mapPoint.Row} c {mapPoint.Column}");
                    double testScore = cm.Score(mapPoint.Row, mapPoint.Column, bedRows, bedCols);
                    //       Log($"Test Score {testScore}");
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
                            Log($"Test Score {testScore}");
                            Log("^^^^ BETTER ^^^^^^");
                        }
                    }
                }
                cm.Position = new Point3D((bestPos.Column * workingCLearance) - (bedWidth / 2), 0, (bestPos.Row * workingCLearance) - (bedHeight / 2));
                Log($"Position {cm.Position.X},{cm.Position.Y},{cm.Position.Z}");
                Results.Add(cm);
                cm.ExpandMap();
                // cm.ExpandMap();
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
                            }
                        }
                    }
                }
                //    overallPlacement.Dump();
            }
        }

        public void AddComponent(Object3D ob, Point3D point1, Point3D point2)
        {
            Component comp = new Component();
            comp.Shape = ob;
            comp.LowBound = point1;
            comp.HighBound = point2;
            comp.Clearance = workingCLearance;

            comp.OriginalPosition = ob.Position;
            components.Add(comp);
        }
    }
}