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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Barnacle.Dialogs.RenameSelection
{
    /// <summary>
    /// Interaction logic for RenameSelectionDlg.xaml
    /// </summary>
    public partial class RenameSelectionDlg : Window, INotifyPropertyChanged
    {
        private List<Object3D> items;
        private string newName;
        private List<Object3D> selections;

        public RenameSelectionDlg()
        {
            InitializeComponent();
            items = null;
            selections = null;
            newName = "";
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public List<Object3D> Items
        {
            get
            {
                return items;
            }
            set
            {
                items = value;
            }
        }

        public String NewName
        {
            get
            {
                return newName;
            }
            set
            {
                newName = value;
            }
        }

        public List<Object3D> Selections
        {
            get
            {
                return selections;
            }
            set
            {
                selections = value;
            }
        }

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OKClicked(object sender, RoutedEventArgs e)
        {
            if (newName != "" && selections != null && items != null)
            {
                List<String> possibles = new List<String>();
                possibles.Add(newName);
                for (int i = 1; i < selections.Count; i++)
                {
                    possibles.Add(newName + "_" + i.ToString());
                }

                bool exists = false;
                foreach (Object3D ob in items)
                {
                    foreach (string s in possibles)
                    {
                        if (ob.Name == s)
                        {
                            bool isInSelection = false;
                            foreach (Object3D ob2 in selections)
                            {
                                if (ob2 == ob)
                                {
                                    isInSelection = true;
                                }
                            }
                            if (!isInSelection)
                            {
                                exists = true;
                            }
                        }
                    }
                }
                if (exists)
                {
                    MessageBox.Show("Rename not possible due to conflicts");
                }
                else
                {
                    selections[0].Name = newName;
                    for (int i = 1; i < selections.Count; i++)
                    {
                        selections[i].Name = newName + "_" + i.ToString();
                    }
                    DialogResult = true;
                    Close();
                }
            }
        }
    }
}