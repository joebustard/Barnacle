using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Make3D.LineLib
{
    public class PathSegment
    {
        public virtual PathPoint End()
        {
            return null;
        }

        public virtual PathPoint Start()
        {
            return null;
        }
    }
}