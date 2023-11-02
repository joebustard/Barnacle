using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

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

        public int Clearance { get; set; }

        public void SetBedSize(double width, double height)
        {
            bedWidth = width;
            bedHeight = height;
            if (Clearance > 0)
            {
                bedRows = (int)(bedHeight / Clearance);
                bedCols = (int)(bedWidth / Clearance);
                overallPlacement = new BedMap(bedRows, bedCols);
            }
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

            foreach (Component cm in components)
            {
                cm.SetMap();
            }
        }

        public void AddComponent(Object3D ob, Point3D point1, Point3D point2)
        {
            Component comp = new Component();
            comp.Shape = ob;
            comp.LowBound = point1;
            comp.HighBound = point2;
            comp.Clearance = Clearance;

            comp.OriginalPosition = ob.Position;
            components.Add(comp);
        }
    }
}