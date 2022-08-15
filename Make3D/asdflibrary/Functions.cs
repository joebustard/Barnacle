using System;
using System.Collections.Generic;
using System.Text;

namespace asdflibrary
{
    public class Functions
    {
     public static float SphereRadius { get; set; }
        public static float Sphere(float x, float y, float z)
        {
            float v = (x * x) + (y * y) + (z * z);
            float d = (float)Math.Sqrt(v) - SphereRadius;
            return d;
        }
    }
}
