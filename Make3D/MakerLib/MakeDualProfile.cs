using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class DualProfileMaker : MakerBase
    {
        private String frontProfile;
        private String topProfile;
        public DualProfileMaker( string front, string top)
    {
            frontProfile = front;
            topProfile = top;
        }

public void Generate(Point3DCollection pnts, Int32Collection faces)
{
    pnts.Clear();
    faces.Clear();
    Vertices = pnts;
    Faces = faces;
}
}
}
