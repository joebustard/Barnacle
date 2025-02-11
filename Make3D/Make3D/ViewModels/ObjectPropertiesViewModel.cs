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
using Barnacle.UserControls;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace Barnacle.ViewModels
{
    internal class ObjectPropertiesViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private List<AvailableColour> availableColours;
        private bool canScale;

        private bool controlsEnabled;
        private String description;
        private bool editingActive;
        private bool exportable;
        private bool lockAspectRatio;
        private AvailableColour objectColour;

        private String objectMetrics;
        private string objectName;

        private string objectType;
        private double percentScale;

        private DispatcherTimer rescaleTimer;
        private double rotationX;
        private double rotationY;
        private double rotationZ;

        private Object3D selectedObject;

        public ObjectPropertiesViewModel()
        {
            selectedObject = null;
            RotateXCommand = new RelayCommand(OnRotateX);
            RotateYCommand = new RelayCommand(OnRotateY);
            RotateZCommand = new RelayCommand(OnRotateZ);
            SetRotationCommand = new RelayCommand(OnSetRotation);
            ScaleByPercentCommand = new RelayCommand(OnScaleByPercent);
            MoveToFloorCommand = new RelayCommand(OnMoveToFloor);
            MoveToCentreCommand = new RelayCommand(OnMoveToCentre);
            MoveToZeroCommand = new RelayCommand(OnMoveToZero);
            NudgeCommand = new RelayCommand(OnNudge);
            NotificationManager.Subscribe("ObjectProperties", "ObjectSelected", OnObjectSelected);
            NotificationManager.Subscribe("ObjectProperties", "ScaleUpdated", OnScaleUpdated);
            NotificationManager.Subscribe("ObjectProperties", "PositionUpdated", OnPositionUpdated);
            NotificationManager.Subscribe("ObjectProperties", "SuspendEditing", SuspendEditing);
            NotificationManager.Subscribe("ObjectProperties", "ScaleByPercent", OnScaleByPercent);

            editingActive = true;
            rotationX = 90;
            rotationY = 90;
            rotationZ = 90;
            PercentScale = 1;
            CanScale = false;
            LockAspectRatio = false;
            SetAvailableColours();
        }

        public List<AvailableColour> AvailableColours
        {
            get
            {
                return availableColours;
            }
            set
            {
                if (availableColours != value)
                {
                    availableColours = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool CanScale
        {
            get
            {
                return canScale && controlsEnabled;
            }
            set
            {
                if (value != canScale)
                {
                    canScale = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool ControlsEnabled
        {
            get
            {
                return (controlsEnabled && editingActive);
            }
            set
            {
                if (controlsEnabled != value)
                {
                    controlsEnabled = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("CanScale");
                }
            }
        }

        public String Description
        {
            get
            {
                return description;
            }
            set
            {
                if (description != value)
                {
                    description = value;
                    if (selectedObject != null)
                    {
                        CheckPoint();
                        selectedObject.Description = value;
                        Document.Dirty = true;
                    }
                    NotifyPropertyChanged();
                }
            }
        }

        public bool EditingActive
        {
            get
            {
                return editingActive;
            }
            set
            {
                if (editingActive != value)
                {
                    editingActive = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("CanScale");
                    NotifyPropertyChanged("ControlsEnabled");
                }
            }
        }

        public bool Exportable
        {
            get
            {
                return exportable;
            }
            set
            {
                if (exportable != value)
                {
                    exportable = value;
                    if (selectedObject != null)
                    {
                        CheckPoint();
                        selectedObject.Exportable = exportable;
                        Document.Dirty = true;
                    }
                    NotifyPropertyChanged();
                }
            }
        }

        public bool LockAspectRatio
        {
            get
            {
                return lockAspectRatio;
            }
            set
            {
                if (lockAspectRatio != value)
                {
                    lockAspectRatio = value;
                    if (selectedObject != null)
                    {
                        CheckPoint();
                        selectedObject.LockAspectRatio = lockAspectRatio;
                        Document.Dirty = true;
                    }
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand MoveToCentreCommand
        {
            get; set;
        }

        public ICommand MoveToFloorCommand
        {
            get; set;
        }

        public ICommand MoveToZeroCommand
        {
            get; set;
        }

        public ICommand NudgeCommand
        {
            get; set;
        }

        public AvailableColour ObjectColour
        {
            get
            {
                return objectColour;
            }
            set
            {
                if (objectColour != value)
                {
                    objectColour = value;
                    NotifyPropertyChanged();
                    if (selectedObject != null)
                    {
                        CheckPoint();
                        System.Drawing.Color tmp = System.Drawing.Color.FromName(objectColour.Name);
                        selectedObject.Color = System.Windows.Media.Color.FromArgb(tmp.A, tmp.R, tmp.G, tmp.B);
                        Document.Dirty = true;
                    }
                }
            }
        }

        public String ObjectMetrics
        {
            get
            {
                return objectMetrics;
            }

            set
            {
                if (objectMetrics != value)
                {
                    objectMetrics = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String ObjectName
        {
            get
            {
                return objectName;
            }
            set
            {
                if (objectName != value)
                {
                    objectName = value;
                    NotifyPropertyChanged();
                    if (selectedObject != null)
                    {
                        if (selectedObject.Name != value)
                        {
                            CheckPoint();
                            selectedObject.Name = value;
                            Document.Dirty = true;
                            NotificationManager.Notify("ObjectNamesChanged", null);
                        }
                    }
                }
            }
        }

        public String ObjectType
        {
            get
            {
                return objectType;
            }
            set
            {
                if (objectType != value)
                {
                    objectType = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double PercentScale
        {
            get
            {
                return percentScale;
            }
            set
            {
                if (percentScale != value)
                {
                    percentScale = value;

                    NotifyPropertyChanged();
                }
            }
        }

        public String PositionX
        {
            get
            {
                if (selectedObject != null)
                {
                    return selectedObject.Position.X.ToString("F3");
                }
                else
                {
                    return "";
                }
            }

            set
            {
                if (selectedObject != null)
                {
                    double v = GetDouble(value);
                    if (selectedObject.Position.X != v)
                    {
                        CheckPoint();
                        Point3D p = selectedObject.Position;
                        Point3D p2 = new Point3D(v, p.Y, p.Z);
                        selectedObject.Position = p2;
                        NotifyPropertyChanged();
                        NotificationManager.Notify("ScaleRefresh", selectedObject);
                        NotificationManager.Notify("RefreshAdorners", null);
                        Document.Dirty = true;
                    }
                }
            }
        }

        public String PositionY
        {
            get
            {
                if (selectedObject != null)
                {
                    return selectedObject.Position.Y.ToString("F3");
                }
                else
                {
                    return "";
                }
            }

            set
            {
                if (selectedObject != null)
                {
                    double v = GetDouble(value);
                    if (selectedObject.Position.Y != v)
                    {
                        CheckPoint();
                        Point3D p = selectedObject.Position;
                        Point3D p2 = new Point3D(p.X, v, p.Z);
                        selectedObject.Position = p2;
                        NotifyPropertyChanged();
                        NotificationManager.Notify("ScaleRefresh", selectedObject);
                        NotificationManager.Notify("RefreshAdorners", null);
                        Document.Dirty = true;
                    }
                }
            }
        }

        public String PositionZ
        {
            get
            {
                if (selectedObject != null)
                {
                    return selectedObject.Position.Z.ToString("F3");
                }
                else
                {
                    return "";
                }
            }

            set
            {
                if (selectedObject != null)
                {
                    double v = GetDouble(value);
                    if (selectedObject.Position.Z != v)
                    {
                        CheckPoint();
                        Point3D p = selectedObject.Position;
                        Point3D p2 = new Point3D(p.X, p.Y, v);
                        selectedObject.Position = p2;
                        NotifyPropertyChanged();
                        NotificationManager.Notify("ScaleRefresh", selectedObject);
                        NotificationManager.Notify("RefreshAdorners", null);
                        Document.Dirty = true;
                    }
                }
            }
        }

        public ICommand RotateXCommand
        {
            get; set;
        }

        public ICommand RotateYCommand
        {
            get; set;
        }

        public ICommand RotateZCommand
        {
            get; set;
        }

        public double RotationX
        {
            get
            {
                return rotationX;
            }

            set
            {
                if (rotationX != value)
                {
                    rotationX = value;
                    NotifyPropertyChanged();
                    NotificationManager.Notify("ObjectXRotationChange", rotationX);
                }
            }
        }

        public double RotationY
        {
            get
            {
                return rotationY;
            }

            set
            {
                if (rotationY != value)
                {
                    rotationY = value;
                    NotifyPropertyChanged();
                    NotificationManager.Notify("ObjectYRotationChange", rotationY);
                }
            }
        }

        public double RotationZ
        {
            get
            {
                return rotationZ;
            }

            set
            {
                if (rotationZ != value)
                {
                    rotationZ = value;
                    NotifyPropertyChanged();
                    NotificationManager.Notify("ObjectZRotationChange", rotationZ);
                }
            }
        }

        public ICommand ScaleByPercentCommand
        {
            get; set;
        }

        public string ScaleX
        {
            get
            {
                if (selectedObject != null)
                {
                    return selectedObject.Scale.X.ToString("F3");
                }
                else
                {
                    return "";
                }
            }

            set
            {
                if (selectedObject != null)
                {
                    double v = GetDouble(value);
                    Scale3D p = selectedObject.Scale;
                    if (p.X != v && p.X != 0 && v != 0)
                    {
                        CheckPoint();
                        double d = v / p.X;
                        if (lockAspectRatio)
                        {
                            RescaleSelectedObject(d, d, d);
                        }
                        else
                        {
                            RescaleSelectedObject(d, 1, 1);
                        }
                    }
                }
            }
        }

        public String ScaleY
        {
            get
            {
                if (selectedObject != null)
                {
                    return selectedObject.Scale.Y.ToString("F3");
                }
                else
                {
                    return "";
                }
            }

            set
            {
                if (selectedObject != null)
                {
                    double v = GetDouble(value);
                    Scale3D p = selectedObject.Scale;
                    if (p.Y != v && p.Y != 0 && v != 0)
                    {
                        CheckPoint();
                        double d = v / p.Y;
                        if (lockAspectRatio)
                        {
                            RescaleSelectedObject(d, d, d);
                        }
                        else
                        {
                            RescaleSelectedObject(1.0, d, 1.0);
                        }
                    }
                }
            }
        }

        public String ScaleZ
        {
            get
            {
                if (selectedObject != null)
                {
                    return selectedObject.Scale.Z.ToString("F3");
                }
                else
                {
                    return "";
                }
            }

            set
            {
                if (selectedObject != null)
                {
                    double v = GetDouble(value);
                    Scale3D p = selectedObject.Scale;
                    if (p.Z != v && p.Z != 0 && v != 0)
                    {
                        CheckPoint();
                        double d = v / p.Z;
                        if (lockAspectRatio)
                        {
                            RescaleSelectedObject(d, d, d);
                        }
                        else
                        {
                            RescaleSelectedObject(1.0, 1.0, d);
                        }
                    }
                }
            }
        }

        public ICommand SetRotationCommand
        {
            get; set;
        }

        private AvailableColour FindAvailableColour(Color color)
        {
            AvailableColour res = null;
            foreach (AvailableColour cl in AvailableColours)
            {
                if (cl.Colour.A == color.A &&
                cl.Colour.R == color.R &&
                cl.Colour.G == color.G &&
                cl.Colour.B == color.B)
                {
                    res = cl;
                    break;
                }
            }
            return res;
        }

        private double GetDouble(string value)
        {
            double res = 0.0;
            try
            {
                res = Convert.ToDouble(value);
            }
            catch (Exception ex)
            {
                LoggerLib.Logger.LogLine(ex.Message);
            }
            return res;
        }

        private void OnMoveToCentre(object obj)
        {
            if (selectedObject != null)
            {
                NotificationManager.Notify("MoveObjectToCentre", selectedObject);
                NotificationManager.Notify("RefreshAdorners", null);
                NotifyPropertyChanged("PositionX");
                NotifyPropertyChanged("PositionY");
                NotifyPropertyChanged("PositionZ");
            }
        }

        private void OnMoveToFloor(object obj)
        {
            if (selectedObject != null)
            {
                NotificationManager.Notify("MoveObjectToFloor", selectedObject);
                NotificationManager.Notify("RefreshAdorners", null);
                NotifyPropertyChanged("PositionX");
                NotifyPropertyChanged("PositionY");
                NotifyPropertyChanged("PositionZ");
            }
        }

        private void OnMoveToZero(object obj)
        {
            if (selectedObject.Position.X != 0.0 || selectedObject.Position.Y != 0.0 || selectedObject.Position.Z != 0.0)
            {
                CheckPoint();

                Point3D p2 = new Point3D(0, 0, 0);
                selectedObject.Position = p2;
                NotifyPropertyChanged();
                NotificationManager.Notify("ScaleRefresh", selectedObject);
                NotificationManager.Notify("RefreshAdorners", null);
                Document.Dirty = true;
                NotifyPropertyChanged("PositionX");
                NotifyPropertyChanged("PositionY");
                NotifyPropertyChanged("PositionZ");
            }
        }

        private void OnNudge(object obj)
        {
            Point3D d = new Point3D(0, 0, 0);
            string s = obj as string;
            if (s != null)
            {
                switch (s)
                {
                    case "X+":
                        {
                            d.X = 1;
                        }
                        break;

                    case "X-":
                        {
                            d.X = -1;
                        }
                        break;

                    case "Y+":
                        {
                            d.Y = 1;
                        }
                        break;

                    case "Y-":
                        {
                            d.Y = -1;
                        }
                        break;

                    case "Z+":
                        {
                            d.Z = 1;
                        }
                        break;

                    case "Z-":
                        {
                            d.Z = -1;
                        }
                        break;
                }
                CheckPoint();
                Point3D p = selectedObject.Position;
                Point3D p2 = new Point3D(p.X + d.X, p.Y + d.Y, p.Z + d.Z);
                selectedObject.Position = p2;
                NotifyPropertyChanged("PositionX");
                NotifyPropertyChanged("PositionY");
                NotifyPropertyChanged("PositionZ");
                NotificationManager.Notify("ScaleRefresh", selectedObject);
                NotificationManager.Notify("RefreshAdorners", null);
                Document.Dirty = true;
            }
        }

        private void OnObjectSelected(object param)
        {
            selectedObject = param as Object3D;
            if (selectedObject == null)
            {
                //objectColour = new SolidColorBrush(Colors.Transparent);
                objectColour = null;
                objectName = "";
                exportable = false;
                description = "";
                ControlsEnabled = false;
                ObjectType = "";
                LockAspectRatio = false;
                ObjectMetrics = "";
            }
            else
            {
                objectColour = FindAvailableColour(selectedObject.Color);
                objectName = selectedObject.Name;
                CanScale = selectedObject.IsSizable();
                exportable = selectedObject.Exportable;
                description = selectedObject.Description;
                lockAspectRatio = selectedObject.LockAspectRatio;
                ControlsEnabled = true;
                ObjectMetrics = $"v:{selectedObject.RelativeObjectVertices.Count} f:{selectedObject.TotalFaces}";
                if (selectedObject is Group3D)
                {
                    ObjectType = "Group";
                }
                else
                {
                    if (selectedObject.EditorParameters.ToolName != "")
                    {
                        ObjectType = selectedObject.EditorParameters.ToolName;
                    }
                    else
                    {
                        ObjectType = "Primitive";
                    }
                }
            }
            NotifyPropertyChanged("PositionX");
            NotifyPropertyChanged("PositionY");
            NotifyPropertyChanged("PositionZ");

            NotifyPropertyChanged("ScaleX");
            NotifyPropertyChanged("ScaleY");
            NotifyPropertyChanged("ScaleZ");

            NotifyPropertyChanged("RotationX");
            NotifyPropertyChanged("RotationY");
            NotifyPropertyChanged("RotationZ");
            NotifyPropertyChanged("ObjectColour");
            NotifyPropertyChanged("ObjectName");
            NotifyPropertyChanged("Exportable");
            NotifyPropertyChanged("LockAspectRatio");
            NotifyPropertyChanged("Description");
            NotifyPropertyChanged("ControlsEnabled");
        }

        private void OnPositionUpdated(object param)
        {
            Object3D o = param as Object3D;
            if (o != null && o == selectedObject)
            {
                NotifyPropertyChanged("PositionX");
                NotifyPropertyChanged("PositionY");
                NotifyPropertyChanged("PositionZ");

                NotifyPropertyChanged("ScaleX");
                NotifyPropertyChanged("ScaleY");
                NotifyPropertyChanged("ScaleZ");
            }
        }

        private void OnRotateX(object obj)
        {
            Point3D p2;
            if (obj.ToString() == "+")
            {
                p2 = new Point3D(rotationX, 0, 0);
            }
            else
            {
                p2 = new Point3D(-rotationX, 0, 0);
            }
            RotateSelected(p2);
        }

        private void OnRotateY(object obj)
        {
            Point3D p2;
            if (obj.ToString() == "+")
            {
                p2 = new Point3D(0, rotationY, 0);
            }
            else
            {
                p2 = new Point3D(0, -rotationY, 0);
            }
            RotateSelected(p2);
        }

        private void OnRotateZ(object obj)
        {
            Point3D p2;
            if (obj.ToString() == "+")
            {
                p2 = new Point3D(0, 0, rotationZ);
            }
            else
            {
                p2 = new Point3D(0, 0, -rotationZ);
            }
            RotateSelected(p2);
        }

        private void OnScaleByPercent(object obj)
        {
            if (selectedObject != null)
            {
                double v = 100.0;
                bool valid = false;
                string param = obj.ToString();
                if (param == "+")
                {
                    v += percentScale;
                    if (v > 0)
                    {
                        v = ScaleBy(v);
                        valid = true;
                    }
                }
                else if (param == "-")
                {
                    v -= percentScale;
                    if (v > 0)
                    {
                        v = ScaleBy(v);
                        valid = true;
                    }
                }
                else if (param == "to")
                {
                    v = percentScale;
                    if (v > 0)
                    {
                        v = ScaleBy(v);
                        valid = true;
                    }
                }

                if (!valid)
                {
                    MessageBox.Show("Scale invalid. Would create a 0 sized object. Operation ignored", "Warning");
                }
            }
        }

        private void OnScaleUpdated(object param)
        {
            NotifyPropertyChanged("ScaleX");
            NotifyPropertyChanged("ScaleY");
            NotifyPropertyChanged("ScaleZ");
        }

        private void OnSetRotation(object obj)
        {
            string v = obj.ToString();
            double d = Convert.ToDouble(v);
            rotationX = d;
            rotationY = d;
            rotationZ = d;
            NotifyPropertyChanged("RotationX");
            NotifyPropertyChanged("RotationY");
            NotifyPropertyChanged("RotationZ");
            NotificationManager.Notify("ObjectXRotationChange", rotationX);
            NotificationManager.Notify("ObjectYRotationChange", rotationY);
            NotificationManager.Notify("ObjectZRotationChange", rotationZ);
        }

        private void RescaleSelectedObject(double d1, double d2, double d3)
        {
            selectedObject.ScaleMesh(d1, d2, d3);
            selectedObject.CalcScale(false);
            selectedObject.Remesh();
            OnScaleUpdated(null);
            NotificationManager.Notify("ScaleRefresh", selectedObject);
            NotificationManager.Notify("RefreshAdorners", null);
            Document.Dirty = true;
        }

        private void RotateSelected(Point3D p2)
        {
            if (selectedObject != null)
            {
                CheckPoint();
                selectedObject.Rotate(p2);
                selectedObject.Remesh();

                NotifyPropertyChanged();
                NotificationManager.Notify("ScaleRefresh", selectedObject);
                NotificationManager.Notify("RefreshAdorners", null);
                Document.Dirty = true;
            }
        }

        private double ScaleBy(double v)
        {
            v = v / 100.0;
            CheckPoint();

            selectedObject.ScaleMesh(v, v, v);
            selectedObject.Remesh();
            selectedObject.CalcScale(false);
            NotifyPropertyChanged();

            NotificationManager.Notify("ScaleRefresh", selectedObject);
            NotificationManager.Notify("RefreshAdorners", null);
            OnScaleUpdated(null);
            Document.Dirty = true;
            return v;
        }

        private void SetAvailableColours()
        {
            string[] ignore =
            {
        "AliceBlue",
        "Azure",
        "Beige",
        "Cornsilk",
        "Ivory",
        "GhostWhite",
        "LavenderBlush",
        "LightYellow",
        "Linen",
        "MintCream",
        "OldLace",
        "SeaShell",
        "Snow",
        "WhiteSmoke",
        "Transparent"
        };
            List<AvailableColour> cls = new List<AvailableColour>();
            Type colors = typeof(System.Drawing.Color);
            PropertyInfo[] colorInfo = colors.GetProperties(BindingFlags.Public |
                BindingFlags.Static);
            foreach (PropertyInfo info in colorInfo)
            {
                var result = Array.Find(ignore, element => element == info.Name);
                if (result == null || result == String.Empty)
                {
                    cls.Add(new AvailableColour(info.Name));
                }
            }
            AvailableColours = cls;
        }

        private void SuspendEditing(object param)
        {
            bool b = Convert.ToBoolean(param);
            EditingActive = !b;
        }
    }
}