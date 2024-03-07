using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Barnacle.UserControls
{
    public class LengthLabel
    {
        public string Content
        {
            set
            {
                if (textBox != null)
                {
                    textBox.Text = value;
                }
            }
        }
        public Point pos;
        public Point offset = new Point(30,30);
        public Point Position
        {
            set
            {
                if (textBox != null)
                {
                    pos = value;
                   Canvas.SetLeft(textBox, pos.X + offset.X);
                    Canvas.SetTop(textBox, pos.Y + offset.Y);
                }
            }
        }
        private TextBox textBox;
        public TextBox TextBox { get { return textBox; } }

        public LengthLabel()
        {
            textBox = new TextBox();
            textBox.FontSize = 16;
            textBox.AcceptsReturn = false;
            textBox.IsReadOnly = true;
        }
    }
}
