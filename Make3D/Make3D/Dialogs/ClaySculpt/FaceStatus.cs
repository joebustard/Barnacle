using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barnacle.Dialogs.ClaySculpt
{
    public class FaceStatus
    {
        public int VerticesInTool { get; set; }
        public int[] FaceVertices { get; set; }

        public FaceStatus()
        {
            VerticesInTool = 0;
            FaceVertices = new int[3];
        }
    }
}