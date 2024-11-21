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
using System.Collections.Generic;

namespace Barnacle.Models
{
    internal class ObjectClipboard
    {
        public static List<Object3D> Items = new List<Object3D>();

        public static void Add(Object3D obj)
        {
            Object3D cl = obj.Clone();
            Items.Add(cl);
        }

        public static void Clear()
        {
            Items.Clear();
        }

        public static bool HasItems()
        {
            return Items.Count > 0;
        }
    }
}