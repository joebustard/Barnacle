﻿/**************************************************************************
*   Copyright (c) 2024 Joe Bustard <barnacle3d@gmailcom>                  *
*                                                                         *
*   This file is part of the Barnacle 3D application.                     *
*                                                                         *
*   This application is free software; you can redistribute it and/or     *
*   modify it under the terms of the GNU Library General Public           *
*   License as published by the Free Software Foundation; either          *
*   version 2 of the License, or (at your option) any later version.      *
*                                                                         *
*   This application is distributed in the hope that it will be useful,   *
*   but WITHOUT ANY WARRANTY; without even the implied warranty of        *
*   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
*   GNU Library General Public License for more details.                  *
*                                                                         *
**************************************************************************/

using Barnacle.Dialogs.RibbedFuselage.Models;
using Barnacle.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Barnacle.Dialogs.RibbedFuselage.Views
{
    /// <summary>
    /// Interaction logic for RibView.xaml
    /// </summary>
    public partial class RibView : UserControl
    {
        private RibImageDetailsModel vm;

        public RibView()
        {
            InitializeComponent();
            FlexiControl.OnFlexiImageChanged = ImageChanged;
            FlexiControl.OnFlexiPathTextChanged = PathChanged;
            FlexiControl.ToolName = "Ribs";
            FlexiControl.HasPresets = true;
        }

        private void PathChanged(string pathText)
        {
            if (vm != null)
            {
                vm.FlexiPathText = pathText;
            }
        }

        private void ImageChanged(string imagePath)
        {
            if (vm != null)
            {
                vm.ImageFilePath = imagePath;
                if (!String.IsNullOrEmpty(imagePath))
                {
                    vm.DisplayFileName = System.IO.Path.GetFileName(imagePath);
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            vm = this.DataContext as RibImageDetailsModel;
            RibView rv = sender as RibView;
            FlexiPathEditorControl fc = rv.FlexiControl;

            if (!String.IsNullOrEmpty(vm.ImageFilePath))
            {
                fc.LoadImage(vm.ImageFilePath);
            }

            fc.SetPath(vm.FlexiPathText);
        }
    }
}