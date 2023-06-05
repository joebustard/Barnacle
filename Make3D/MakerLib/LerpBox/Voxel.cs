using System;
using System.Collections.Generic;
using System.Text;

namespace Lerp.LerpLib
{
    public class Voxel
    {
        public float X;
        public float Y;
        public float Z;
        public float V;

        public Voxel()
        {
        }

        public Voxel(float px, float py, float pz, float v)
        {
            this.X = px;
            this.Y = py;
            this.Z = pz;
            V = v;
        }

        public void Dump()
        {
            System.Diagnostics.Debug.WriteLine($"{X},{Y},{Z}={V}");
        }
    }
}