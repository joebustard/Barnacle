using Make3D.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Make3D.Dialogs.Figure
{
    internal class BoneDisplayRecord
    {
        public BoneDisplayRecord()
        {
            ModelName = "bone";
        }

        public Point3D MarkerPosition { get; set; }
        public string ModelName { get; internal set; }
        public string Name { get; internal set; }

        // Absolute, Midpoint of the associated bone (i.e. after all rotations etc)
        public Point3D Position { get; set; }

        // accumlated roations of all parents etc
        public Point3D Rotation { get; set; }

        public Scale3D Scale { get; set; }
    }
}