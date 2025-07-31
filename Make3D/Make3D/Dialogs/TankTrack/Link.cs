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

using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;

namespace Barnacle.Dialogs
{
    public class Link
    {
        public Link()
        {
            Name = "";

            SourceModel = new Object3D();
        }

        public String Name
        {
            get; set;
        }

        public Object3D ScaledModel
        {
            get; set;
        }

        public Object3D SourceModel
        {
            get; set;
        }

        internal void SetLinkSize(double linkLength, double linkHeight, double linkWidth)
        {
            if (SourceModel != null)
            {
                ScaledModel = SourceModel.Clone();
                // source model is 1x1x1
                ScaledModel.ScaleMesh(linkLength, linkHeight, linkWidth);
            }
        }
    }
}