using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class Maker<TOOL> : MakerBase
    {
        <FIELDS>

        public Maker<TOOL>(<CONSTRUCTORPARAMS>)
        {
            <FIELDCOPY>
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