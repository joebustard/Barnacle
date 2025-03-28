using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs.SdfModel.Operations
{
    internal class SdfStep
    {
        public SdfOperation Operation
        {
            get;
            set;
        }

        public SdfPrimitive Primitive
        {
            get;
            set;
        }

        public override string ToString()
        {
            string s = Primitive.ToParameters();
            if (Operation != null)
            {
                s += "," + Operation.ToParameters();
            }
            else
            {
                s += ",null";
            }
            return s;
        }

        internal void AdjustBounds(Bounds3D bounds)
        {
            Primitive?.AdjustBounds(bounds);
        }

        internal double DoOperation(Point3D xYZ, double res)
        {
            double v = GetSdfValue(xYZ);
            if (Operation != null)
            {
                res = Operation.Apply(v, res);
            }
            return res;
        }

        internal double GetSdfValue(Point3D xyZ)
        {
            if (Primitive != null)
            {
                return Primitive.GetSdfValue(xyZ);
            }
            else
            {
                return double.MaxValue;
            }
        }
    }
}