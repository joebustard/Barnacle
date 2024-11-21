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

using Barnacle.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace Barnacle.Models
{
    public static class WindowExtensions
    {
        public static void SaveSizeAndLocation(this Window w, bool dialog = false)
        {
            try
            {
                var s = "<W>";
                s += GetNode("Top", w.WindowState == WindowState.Maximized ? w.RestoreBounds.Top : w.Top);
                s += GetNode("Left", w.WindowState == WindowState.Maximized ? w.RestoreBounds.Left : w.Left);
                s += GetNode("Height", w.WindowState == WindowState.Maximized ? w.RestoreBounds.Height : w.Height);
                s += GetNode("Width", w.WindowState == WindowState.Maximized ? w.RestoreBounds.Width : w.Width);
                s += GetNode("WindowState", w.WindowState);
                s += "</W>";

                if ( dialog )
                {
                    Settings.Default.DialogXml = s;
                }
                else
                {
                    Settings.Default.WindowXml = s;
                }
                
                Settings.Default.Save();
                
            }
            catch (Exception)
            {
            }
        }

        public static void RestoreSizeAndLocation(this Window w, bool dialog =false)
        {
            try
            {
                XDocument xd;
                if (dialog)
                {
                    xd = XDocument.Parse(Settings.Default.DialogXml);
                }
                else
                {
                    xd = XDocument.Parse(Settings.Default.WindowXml);
                }
                w.WindowState = (WindowState)Enum.Parse(typeof(WindowState), xd.Descendants("WindowState").FirstOrDefault().Value);
                w.Top = Convert.ToDouble(xd.Descendants("Top").FirstOrDefault().Value);
                w.Left = Convert.ToDouble(xd.Descendants("Left").FirstOrDefault().Value);
                w.Height = Convert.ToDouble(xd.Descendants("Height").FirstOrDefault().Value);
                w.Width = Convert.ToDouble(xd.Descendants("Width").FirstOrDefault().Value);
            }
            catch (Exception)
            {
            }
        }

        private static string GetNode(string name, object value)
        {
            return string.Format("<{0}>{1}</{0}>", name, value);
        }
    }
}
