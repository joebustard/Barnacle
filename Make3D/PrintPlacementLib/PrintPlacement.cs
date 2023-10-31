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
        public double BedWidth { get { return bedWidth; } };
        private double bedHeight;
        public double BedHeight { get { return bedHeight; } };
        public List<Component> components;
        private BedMap overallPlacement;

        public PrintPlacement()
        {
            Clearance = 3;
            SetBedSize(200, 200);
            components = new List<Component>();
        }

        public int Clearance { get; set; }

        public void SetBedSize(double width, double height)
        {
            bedWidth = width;
            bedHeight = height;
            if (Clearance > 0)
            {
                int rows = (int)(bedHeight / Clearance);
                int cols = (int)(bedWidth / Clearance);
                overallPlacement = new BedMap(rows, cols);
            }
        }

        public void Arrange()
        {
        }

        public void AddComponent(Object3D ob, Point point1, Point point2)
        {
            Component comp = new Component();
            comp.Shape = ob;
            comp.LowBound = point1;
            comp.LowBound = point2;
            comp.OriginalPosition = ob.Position;
            components.Add(comp);
        }

        public class Component
        {
            public Object3D Shape { get; set; }
            public Point LowBound { get; set; }
            public Point HighBound { get; set; }
            public Point3D Position { get; set; }
            public Point3D OriginalPosition { get; set; }
        }
    }
}