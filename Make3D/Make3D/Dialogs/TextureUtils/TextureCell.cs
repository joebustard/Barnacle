using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barnacle.Dialogs.TextureUtils
{
    public class TextureCell
    {
        public byte Width;
        // size of side walls to add
        // 0 means no wall, 255 means all the way back
        public byte NorthWall;
        public byte SouthWall;
        public byte EastWall;
        public byte WestWall;

        public TextureCell(byte r)
        {
            Width = r;
            NorthWall = 0;
            SouthWall = 0;
            EastWall = 0;
            WestWall = 0;
        }
    }
}
