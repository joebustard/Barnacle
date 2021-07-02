using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Make3D.Dialogs.Figure
{
    public class FigureModel
    {
        public FigureModel(string name, string figureModelName, double ls, double ws, double hs)
        {
            this.BoneName = name;
            this.FigureModelName = figureModelName;
            LScale = ls;
            WScale = ws;
            HScale = hs;
        }

        public String BoneName { get; set; }
        public String FigureModelName { get; set; }
        public double HScale { get; set; }
        public double LScale { get; set; }
        public double WScale { get; set; }
    }
}