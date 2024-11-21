// **************************************************************************
// *   Copyright (c) 2024 Joe Bustard <barnacle3d@gmailcom>                  *
// *                                                                         *
// *   This file is part of the Barnacle 3D application.                     *
// *                                                                         *
// *   This application is free software. You can redistribute it and/or     *
// *   modify it under the terms of the GNU Library General Public           *
// *   License as published by the Free Software Foundation. Either          *
// *   version 2 of the License, or (at your option) any later version.      *
// *                                                                         *
// *   This application is distributed in the hope that it will be useful,   *
// *   but WITHOUT ANY WARRANTY. Without even the implied warranty of        *
// *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
// *   GNU Library General Public License for more details.                  *
// *                                                                         *
// *************************************************************************

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
