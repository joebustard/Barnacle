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
using FileUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Barnacle.ViewModels
{
    internal partial class EditorViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private void BackCamera()
        {
            ResetSelection();
            camera.HomeBack();
            SetCameraDistance();
            NotifyPropertyChanged("CameraPos");
            CameraScrollDelta = new Point3D(-1, 1, 0);
            LookToCenter();
            zoomPercent = 100;
        }

        private void BottomCamera()
        {
            ResetSelection();
            camera.HomeBottom();
            SetCameraDistance();
            NotifyPropertyChanged("CameraPos");
            CameraScrollDelta = new Point3D(-1, 0, -1);
            LookToCenter();
            zoomPercent = 100;
        }

        private void HomeCamera()
        {
            ResetSelection();
            camera.HomeFront();
            SetCameraDistance();
            NotifyPropertyChanged("CameraPos");
            CameraScrollDelta = new Point3D(1, 1, 0);
            LookToCenter();
            zoomPercent = 100;
        }

        private void LeftCamera()
        {
            ResetSelection();
            camera.HomeLeft();
            // true because we are looking from the side
            SetCameraDistance(true);
            NotifyPropertyChanged("CameraPos");
            CameraScrollDelta = new Point3D(0, 1, 1);
            LookToCenter();
            zoomPercent = 100;
        }

        private void LoadCamera()
        {
            string dataPath = PathManager.CommonAppDataFolder() + "\\" + cameraRecordFile;
            camera.Read(dataPath);
            LookToCenter();
            ReportCameraPosition();
            NotifyPropertyChanged("CameraPos");
        }

        private void LookAtObject()
        {
            if (selectedItems.Count == 1)
            {
                Object3D sel = selectedItems[0];
                CameraLookObject = sel.Position;
                camera.LookAt(CameraLookObject);
                LookToObject();
                NotifyPropertyChanged("CameraPos");
                NotifyPropertyChanged("LookDirection");
            }
        }

        private void LookToCenter()
        {
            lookDirection.X = -camera.CameraPos.X;
            lookDirection.Y = -camera.CameraPos.Y;
            lookDirection.Z = -camera.CameraPos.Z;
            lookDirection.Normalize();
            NotifyPropertyChanged("LookDirection");
        }

        private void LookToObject()
        {
            Vector3D v = new Vector3D(CameraLookObject.X - camera.CameraPos.X,
                  CameraLookObject.Y - camera.CameraPos.Y,
                  CameraLookObject.Z - camera.CameraPos.Z);
            v.Normalize();
            LookDirection = v;
            NotifyPropertyChanged("LookDirection");
        }

        private void MoveCameraDelta(Point lastMouse, Point newPos)
        {
            double dx = newPos.X - lastMouse.X;
            double dy = newPos.Y - lastMouse.Y;
            double dz = newPos.X - lastMouse.X;

            camera.Move(dx, dy);
            ReportCameraPosition();
            NotifyPropertyChanged("CameraPos");
        }

        private void OnCameraCommand(object param)
        {
            string p = param.ToString();
            switch (p)
            {
                case "CameraHome":
                    {
                        HomeCamera();
                        cameraMode = CameraModes.CameraMoveLookCenter;
                    }
                    break;

                case "CameraBack":
                    {
                        BackCamera();
                        cameraMode = CameraModes.CameraMoveLookCenter;
                    }
                    break;

                case "CameraLeft":
                    {
                        LeftCamera();
                        cameraMode = CameraModes.CameraMoveLookCenter;
                    }
                    break;

                case "CameraRight":
                    {
                        RightCamera();
                        cameraMode = CameraModes.CameraMoveLookCenter;
                    }
                    break;

                case "CameraTop":
                    {
                        TopCamera();
                        cameraMode = CameraModes.CameraMoveLookCenter;
                    }
                    break;

                case "CameraBottom":
                    {
                        BottomCamera();
                        cameraMode = CameraModes.CameraMoveLookCenter;
                    }
                    break;

                case "CameraLookCenter":
                    {
                        LookToCenter();
                        ReportCameraPosition();
                        cameraMode = CameraModes.CameraMoveLookCenter;
                    }
                    break;

                case "CameraMove":
                    {
                        cameraMode = CameraModes.CameraMove;
                    }
                    break;

                case "CameraMoveLookCenter":
                    {
                        cameraMode = CameraModes.CameraMoveLookCenter;
                        camera.LookAt(new Point3D(0, 0, 0));
                    }
                    break;

                case "CameraLookObject":
                    {
                        cameraMode = CameraModes.CameraMoveLookObject;
                        LookAtObject();
                    }
                    break;

                default:
                    break;
            }
            ReportCameraPosition();
        }

        private void ReportCameraPosition()
        {
            String s = $"Camera ({camera.CameraPos.X:F2},{camera.CameraPos.Y:F2},{camera.CameraPos.Z:F2}) => ({lookDirection.X:F2},{lookDirection.Y:F2},{lookDirection.Z:F2}) Zoom {zoomPercent:F1}%";
            NotificationManager.Notify("SetStatusText1", s);
            NotifyPropertyChanged("ModelItems");
            ReportStatistics();
        }

        private void RightCamera()
        {
            ResetSelection();
            camera.HomeRight();
            // true because we are looking from the side
            SetCameraDistance(true);
            NotifyPropertyChanged("CameraPos");
            CameraScrollDelta = new Point3D(0, 1, -1);
            LookToCenter();
            zoomPercent = 100;
        }

        private void RotateCamera(double dt, double dp)
        {
            camera.RotateDegrees(dt, dp);
            LookToCenter();
            ReportCameraPosition();
            NotifyPropertyChanged("CameraPos");
        }

        private void SaveCamera()
        {
            string dataPath = PathManager.CommonAppDataFolder();
            dataPath += "\\" + cameraRecordFile;
            camera.Save(dataPath);
        }

        private void SetCameraDistance(bool sideView = false)
        {
            double w = allBounds.Upper.X;
            double h = allBounds.Upper.Y;
            double d = allBounds.Upper.Z;

            if (Math.Abs(allBounds.Lower.X) > w)
            {
                w = Math.Abs(allBounds.Lower.X);
            }

            if (Math.Abs(allBounds.Lower.Y) > h)
            {
                h = Math.Abs(allBounds.Lower.Y);
            }

            if (Math.Abs(allBounds.Lower.Z) > d)
            {
                d = Math.Abs(allBounds.Lower.Z);
            }
            if (sideView)
            {
                camera.DistanceToFit(d, h, w * 1.5);
            }
            else
            {
                camera.DistanceToFit(w, h, d * 1.5);
            }
        }

        private void TopCamera()
        {
            ResetSelection();
            camera.HomeTop();
            NotifyPropertyChanged("CameraPos");
            CameraScrollDelta = new Point3D(1, 0, 1);
            LookToCenter();
            zoomPercent = 100;
        }

        private void UpdateLookAt()
        {
            if (cameraMode == CameraModes.CameraMoveLookObject)
            {
                if (selectedItems.Count == 1)
                {
                    LookAtObject();
                }
            }
        }

        private void Zoom(double v)
        {
            camera.Zoom(v);
            zoomPercent += v;
            ReportCameraPosition();
            NotifyPropertyChanged("CameraPos");
        }

        private void ZoomIn(object param)
        {
            Zoom(1);
        }

        private void ZoomOut(object param)
        {
            if (zoomPercent > -50)
            {
                Zoom(-1);
            }
        }

        private void ZoomReset(object param)
        {
            double diff = 100 - zoomPercent;
            Zoom(diff);
            zoomPercent = 100;
        }
    }
}