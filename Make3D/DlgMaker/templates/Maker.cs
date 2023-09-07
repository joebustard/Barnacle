using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class TOOLNAMEMaker : MakerBase
    {
        //FIELDS

        public TOOLNAMEMaker( /*CONSTRUCTORPARAMS*/ )
        {
            //FIELDCOPY
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