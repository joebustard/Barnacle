﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sdflib
{
    public interface Isdf
    {
        void GetDimensions(ref int x, ref int y, ref int z);

        bool SetDimension(int x, int y, int z);

        void Clear();

        void Set(int x, int y, int z, double v);

        double Get(int x, int y, int z);

        // union another sdf into this one at the offset
        void Union(Isdf sdf, int x, int y, int z);

        // difference another sdf into this one at the offset
        void Difference(Isdf sdf, int x, int y, int z);
    }
}