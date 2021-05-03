using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Make3D.Models.CatmullClarke
{
    class CatmullSmoother
    {
        List<CatPoint> catPoints;
        public CatmullSmoother()
        {
            catPoints = new List<CatPoint>();
        }

        internal void Smooth(ref Point3DCollection p3col, ref Int32Collection icol)
        {
            foreach( Point3D p in p3col)
            {

                CatPoint cp = new CatPoint(p);
                catPoints.Add(cp);
            }
        }
    }
}
