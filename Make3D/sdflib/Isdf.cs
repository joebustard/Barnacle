using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sdflib
{
    public interface Isdf
    {
        void Clear(double d);

        double Get(int r, int c, int p);

        double GetAt(double x, double y, double z);

        void GetDimensions(ref int r, ref int c, ref int p);

        // union another sdf into this one at the offset
        void Perform(Isdf sdf, int r, int c, int p, Sdf.OpType op, double strength);

        void Set(int r, int c, int p, double v);

        bool SetDimension(int r, int c, int p);
    }
}