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

using Barnacle.Models;
using Barnacle.Object3DLib;
using Object3DLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using static MakerLib.TextureUtils.TextureManager;

namespace Barnacle.ViewModels
{
    internal partial class EditorViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private void DropItems(string dir)
        {
            CheckPoint();
            Object3D baseOb = selectedObjectAdorner.SelectedObjects[0];
            ObjectDropper dropper = new ObjectDropper();

            for (int i = 1; i < selectedItems.Count; i++)
            {
                Object3D ob = selectedItems[i];
                dropper.DropItems(dir, baseOb, ob);
            }
        }
    }
}