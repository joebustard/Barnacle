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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for NewUserTemplate.xaml
    /// </summary>
    public partial class NewUserTemplate : Window
    {
        private bool clearExport;
        private bool includeFileContents;
        private String templateDescription;
        private String templateName;

        public NewUserTemplate()
        {
            InitializeComponent();
            includeFileContents = true;
            clearExport = true;
            DataContext = this;
        }

        public bool ClearExport
        {
            get
            {
                return clearExport;
            }

            set
            {
                clearExport = value;
            }
        }

        public bool IncludeFileContents
        {
            get
            {
                return includeFileContents;
            }

            set
            {
                includeFileContents = value;
            }
        }

        public String TemplateDescription
        {
            get
            {
                return templateDescription;
            }
            set
            {
                templateDescription = value;
            }
        }

        public String TemplateName
        {
            get
            {
                return templateName;
            }
            set
            {
                templateName = value;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}