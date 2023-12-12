using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
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
            meshColour = Colors.Brown;
        }
        private GeometryModel3D model;
        public GeometryModel3D HitModel
        {
            get { return model; }
        }
        public new GeometryModel3D GetModel()
        {
            model = base.GetModel();
            return model;
        }
    }
}
