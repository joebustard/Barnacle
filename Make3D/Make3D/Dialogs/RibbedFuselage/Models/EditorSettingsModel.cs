using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Barnacle.RibbedFuselage.Models
{
    internal class EditorSettingsModel
    {
        public EditorSettingsModel()
        {
            throw new System.NotImplementedException();
        }

        public bool ShowGrid
        {
            get => default;
            set
            {
            }
        }

        public int Pen
        {
            get => default;
            set
            {
            }
        }

        public void Load(XmlElement ele)
        {
        }

        public void Save(XmlElement ele, XmlDocument doc)
        {
        }
    }
}