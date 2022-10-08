using System;

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

        public static float Sphere(double x, double y, double z)
        {
            return Sphere((float)x, (float)y, (float)z);
        }
    }
}