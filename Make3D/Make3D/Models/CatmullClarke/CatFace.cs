using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Make3D.Models.CatmullClarke
{
    class CatFace
    {
        public int P1 { get; set; }
        public int P2 { get; set; }
        public int P3 { get; set; }

        public int E1 { get; set; }
        public int E2 { get; set; }
        public int E3 { get; set; }


        public CatPoint FacePoint { get; set; }
    }
}
