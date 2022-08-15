using System;
using System.Collections.Generic;
using System.Text;

namespace asdflib
{
    class Functions
    {

    public static float Sphere( float x, float y, float z, float radius)
    {
            float v = (x * x) + (y * y) + (z * z);
            float d = (float)Math.Sqrt(v) - radius;
            return d;
    }
    }
}
