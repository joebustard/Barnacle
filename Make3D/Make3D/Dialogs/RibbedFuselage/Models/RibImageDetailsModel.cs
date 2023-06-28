using Barnacle.RibbedFuselage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Barnacle.Dialogs.RibbedFuselage.Models
{
    internal class RibImageDetailsModel : ImageDetailsModel
    {
        public int MarkerPosition
        {
            get => default;
            set
            {
            }
        }

        public override void Load(XmlElement ele)
        {
        }

        public override void Save(XmlElement ele, XmlDocument doc)
        {
            base.Save(ele, doc);
        }
    }
}