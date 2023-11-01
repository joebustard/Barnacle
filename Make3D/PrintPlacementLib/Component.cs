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
    public class Component
    {
        public Object3D Shape { get; set; }
        public Point LowBound { get; set; }
        public Point HighBound { get; set; }
        public Point3D Position { get; set; }
        public Point3D OriginalPosition { get; set; }
        public BedMap Map { get; set; }

        internal void SetMap()
        {
        }
    }
}