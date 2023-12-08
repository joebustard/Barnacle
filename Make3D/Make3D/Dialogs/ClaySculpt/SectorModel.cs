using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    public class SectorModel : BaseModellerDialog
    {
        public bool Dirty { get; set; }
        public SectorModel()
        {
            Dirty = true;
            ClearShape();
        }
    }
}
