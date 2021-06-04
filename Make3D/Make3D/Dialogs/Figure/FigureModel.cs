using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Make3D.Dialogs.Figure
{
    public class FigureModel
    {
        public FigureModel(string name, string figureModelName)
        {
            this.BoneName = name;
            this.FigureModelName = figureModelName;
        }

        public String BoneName { get; set; }
        public String FigureModelName { get; set; }
    }
}