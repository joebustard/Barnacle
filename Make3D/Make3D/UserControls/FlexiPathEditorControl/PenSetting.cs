using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Barnacle.UserControls
{
    public class PenSetting
    {
        public PenSetting(int v1, SolidColorBrush br, double op)
        {
            this.StrokeThickness = v1;
            this.Brush = br;
            this.Opacity = op;
            DashPattern = new DoubleCollection();
        }

        public double Opacity { get; set; }
        public int StrokeThickness { get; set; }
        public SolidColorBrush Brush { get; set; }
        public DoubleCollection DashPattern { get; set; }
    }
}