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

using Barnacle.Dialogs;
using Barnacle.Models;
using Barnacle.Object3DLib;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Barnacle.ViewModels
{
    internal partial class EditorViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private void OnCircularPaste(object param)
        {
            CircularPasteDlg dlg = new CircularPasteDlg();
            dlg.Owner = Application.Current.MainWindow;
            if (dlg.ShowDialog() == true)
            {
                if (ObjectClipboard.HasItems())
                {
                    CheckPoint();
                    RecalculateAllBounds();
                    if (selectedObjectAdorner != null)
                    {
                        selectedObjectAdorner.Clear();
                    }
                    double radius = Convert.ToDouble(dlg.RadiusBox.Text);
                    double altRadius = Convert.ToDouble(dlg.AltBox.Text);
                    if (altRadius == 0)
                    {
                        altRadius = radius;
                    }
                    if (radius > 0 || altRadius > 0)
                    {
                        double cx = allBounds.Upper.X;
                        if (radius > altRadius)
                        {
                            cx += radius;
                        }
                        else
                        {
                            cx += altRadius;
                        }
                        double cy = 0;
                        double cz = 0;
                        if (Project.SharedProjectSettings.PlaceNewAtMarker && floorMarker != null)
                        {
                            cx = floorMarker.Position.X;
                            cy = floorMarker.Position.Y;
                            cz = floorMarker.Position.Z;
                        }
                        int repeats = Convert.ToInt16(dlg.RepeatsBox.Text);
                        if (repeats > 0)
                        {
                            double rx;
                            double ry;
                            double rz;

                            double dTheta = (Math.PI * 2) / repeats;
                            double theta = 0;
                            bool alt = false;
                            double x;
                            double y;

                            while (theta < (Math.PI * 2))
                            {
                                rx = 0;
                                ry = 0;
                                rz = 0;
                                if (!alt)
                                {
                                    x = radius * Math.Cos(theta);
                                    y = radius * Math.Sin(theta);
                                }
                                else
                                {
                                    x = altRadius * Math.Cos(theta);
                                    y = altRadius * Math.Sin(theta);
                                }
                                alt = !alt;
                                foreach (Object3D cl in ObjectClipboard.Items)
                                {
                                    Object3D o = cl.Clone();
                                    o.Name = Document.NextName;

                                    if (o is Group3D)
                                    {
                                        (o as Group3D).Init();
                                    }

                                    if (dlg.DirectionX.IsChecked == true)
                                    {
                                        o.Position = new Point3D(cx + o.AbsoluteBounds.Width / 2 + x, cy, cz + o.AbsoluteBounds.Depth / 2 + y);
                                        ry = -theta;
                                        if (dlg.FaceIn.IsChecked == true)
                                        {
                                            ry += Math.PI;
                                        }
                                    }

                                    if (dlg.DirectionY.IsChecked == true)
                                    {
                                        o.Position = new Point3D(cx, cy + o.AbsoluteBounds.Height / 2 + x, cz + o.AbsoluteBounds.Depth / 2 + y);
                                        // rx = (Math.PI / 2) + theta;
                                        rx = theta;
                                        if (dlg.FaceIn.IsChecked == true)
                                        {
                                            rx += Math.PI;
                                        }
                                    }

                                    if (dlg.DirectionZ.IsChecked == true)
                                    {
                                        o.Position = new Point3D(cx + o.AbsoluteBounds.Width / 2 + x, cy + o.AbsoluteBounds.Height / 2 + y, cz);
                                        // rz = theta;
                                        //rz = (Math.PI / 2) + theta;
                                        rz = 3 * Math.PI / 2 + theta;
                                        if (dlg.FaceIn.IsChecked == true)
                                        {
                                            rz += Math.PI;
                                        }
                                    }

                                    o.CalcScale(false);

                                    if (!dlg.FaceNone.IsChecked == true)
                                    {
                                        o.RotateRad(new Point3D(rx, ry, rz));
                                    }

                                    o.Remesh();

                                    if (dlg.DirectionX.IsChecked == true)
                                    {
                                        o.MoveToFloor();
                                    }
                                    allBounds.Add(o.AbsoluteBounds);
                                    GeometryModel3D gm = GetMesh(o);
                                    Document.Content.Add(o);
                                    Document.Dirty = true;

                                    selectedItems.Add(o);
                                    if (selectedObjectAdorner == null)
                                    {
                                        MakeSizeAdorner();
                                    }
                                    selectedObjectAdorner.AdornObject(o);
                                }

                                theta += dTheta;
                            }
                        }
                        document.Dirty = true;
                        RegenerateDisplayList();
                        UpdateSelectionDisplay();
                    }
                }
            }
        }

        private void OnCloneInPlace(object param)
        {
            if (selectedObjectAdorner != null && selectedObjectAdorner.SelectedObjects.Count == 1)
            {
                Object3D ob = selectedObjectAdorner.SelectedObjects[0];
                if (ob != null)
                {
                    CheckPoint();
                    Object3D o = ob.Clone();
                    o.Name = Document.DuplicateName(o.Name);

                    o.Remesh();
                    // o.MoveToFloor();
                    o.CalcScale(false);
                    allBounds += o.AbsoluteBounds;
                    GeometryModel3D gm = GetMesh(o);
                    Document.Content.Add(o);
                    Document.Dirty = true;

                    selectedObjectAdorner.Clear();
                    selectedObjectAdorner.AdornObject(o);
                    RegenerateDisplayList();
                    NotificationManager.Notify("ObjectNamesChanged", null);
                    NotificationManager.Notify("ObjectSelected", o);
                    PassOnGroupStatus(o);
                }
            }
            else
            {
                MessageBox.Show("Must have a single object selected", "Error");
            }
        }

        private void OnCopy(object param)
        {
            if (selectedObjectAdorner != null)
            {
                ObjectClipboard.Clear();
                GC.Collect();
                foreach (Object3D o in selectedObjectAdorner.SelectedObjects)
                {
                    ObjectClipboard.Add(o);
                }
            }
        }

        private void OnCut(object param)
        {
            if (selectedObjectAdorner != null)
            {
                CheckPoint();
                ObjectClipboard.Clear();
                foreach (Object3D o in selectedObjectAdorner.SelectedObjects)
                {
                    ObjectClipboard.Add(o);
                    Document.DeleteObject(o);
                }
                document.Dirty = true;
                selectedObjectAdorner.Clear();
                RegenerateDisplayList();
                NotificationManager.Notify("ObjectNamesChanged", null);
            }
        }

        private void OnMultiPaste(object param)
        {
            MultiPasteConfig cfg = param as MultiPasteConfig;
            if (cfg != null)
            {
                if (ObjectClipboard.HasItems())
                {
                    CheckPoint();
                    RecalculateAllBounds();
                    if (selectedObjectAdorner == null)
                    {
                        MakeSizeAdorner();
                    }

                    selectedObjectAdorner.Clear();
                    double cx = allBounds.Upper.X;

                    double cy = allBounds.Upper.Y;
                    double cz = allBounds.Upper.Z;
                    if (Project.SharedProjectSettings.PlaceNewAtMarker && floorMarker != null)
                    {
                        cx = floorMarker.Position.X;
                        cy = floorMarker.Position.Y;
                        cz = floorMarker.Position.Z;
                    }
                    double altOff = 0;
                    for (int i = 0; i < cfg.Repeats; i++)
                    {
                        if (i % 2 == 0)
                        {
                            altOff = 0;
                        }
                        else
                        {
                            altOff = cfg.AlternatingOffset;
                        }
                        foreach (Object3D cl in ObjectClipboard.Items)
                        {
                            Object3D o = cl.Clone();
                            o.Name = Document.NextName;

                            if (o is Group3D)
                            {
                                (o as Group3D).Init();
                            }

                            if (cfg.Direction == "X")
                            {
                                o.Position = new Point3D(cx + o.AbsoluteBounds.Width / 2, cy, cz + altOff);
                                cx += o.AbsoluteBounds.Width + cfg.Spacing;
                            }
                            if (cfg.Direction == "Y")
                            {
                                o.Position = new Point3D(cx + altOff, cy + o.AbsoluteBounds.Height / 2, cz);
                                cy += o.AbsoluteBounds.Height + cfg.Spacing;
                            }
                            if (cfg.Direction == "Z")
                            {
                                o.Position = new Point3D(cx + altOff, cy, cz + o.AbsoluteBounds.Depth / 2);
                                cz += o.AbsoluteBounds.Depth + cfg.Spacing;
                            }
                            o.CalcScale(false);

                            o.Remesh();
                            if (cfg.Direction == "X" || cfg.Direction == "Z")
                            {
                                o.MoveToFloor();
                            }

                            allBounds.Add(o.AbsoluteBounds);
                            GeometryModel3D gm = GetMesh(o);
                            Document.Content.Add(o);
                            Document.Dirty = true;

                            selectedItems.Add(o);
                            selectedObjectAdorner.AdornObject(o);
                        }
                    }
                    UpdateSelectionDisplay();
                    RegenerateDisplayList();
                }
            }
        }

        private void OnPaste(object param)
        {
            if (ObjectClipboard.HasItems())
            {
                CheckPoint();
                RecalculateAllBounds();

                if (selectedObjectAdorner != null && selectedObjectAdorner.SelectedObjects.Count == 1)
                {
                    Object3D old = selectedObjectAdorner.SelectedObjects[0];
                    MessageBoxResult res = MessageBox.Show("Do you want to replace " + old.Name + "?", "Info", MessageBoxButton.YesNo);
                    if (res == MessageBoxResult.Yes)
                    {
                        Document.Content.Remove(old);
                        PasteAt(old.Position);
                    }
                    else
                    {
                        OrdinaryPaste();
                    }
                }
                else
                {
                    OrdinaryPaste();
                }
            }
        }

        private void OnPasteAt(object param)
        {
            if (ObjectClipboard.HasItems() && floorMarker != null)
            {
                PasteAt(floorMarker.Position);
            }
        }

        private void OrdinaryPaste()
        {
            selectedObjectAdorner?.Clear();

            if (Project.SharedProjectSettings.PlaceNewAtMarker && floorMarker != null)
            {
                PasteAt(floorMarker.Position);
            }
            else
            {
                foreach (Object3D cl in ObjectClipboard.Items)
                {
                    Object3D o = cl.Clone();
                    if (Document.ContainsName(o.Name))
                    {
                        o.Name = Document.DuplicateName(o.Name);
                    }

                    double cx = allBounds.Upper.X + o.AbsoluteBounds.Width / 2;

                    o.Position = new Point3D(cx, 0, 0);
                    o.Remesh();
                    o.MoveToFloor();
                    o.CalcScale(false);
                    allBounds += o.AbsoluteBounds;
                    GeometryModel3D gm = GetMesh(o);
                    Document.Content.Add(o);
                    Document.Dirty = true;
                }
            }
            RegenerateDisplayList();
            NotificationManager.Notify("ObjectNamesChanged", null);
        }

        private void PasteAt(Point3D targetPoint)
        {
            CheckPoint();
            RecalculateAllBounds();
            selectedObjectAdorner?.Clear();
            Point collectionCentre = new Point(0, 0);
            // if we have more than one object being pasted at the marker we want to keep the
            // relative positions the same as the original but treat the marker as there new centre.
            // If there is only one object, it should just be positioned directly at the marker
            if (ObjectClipboard.Items.Count > 1)
            {
                foreach (Object3D cl in ObjectClipboard.Items)
                {
                    collectionCentre.X = collectionCentre.X + cl.Position.X;
                    collectionCentre.Y = collectionCentre.Y + cl.Position.Z;
                }
                collectionCentre.X /= ObjectClipboard.Items.Count;
                collectionCentre.Y /= ObjectClipboard.Items.Count;
            }
            foreach (Object3D cl in ObjectClipboard.Items)
            {
                Object3D o = cl.Clone();
                if (Document.ContainsName(o.Name))
                {
                    o.Name = Document.DuplicateName(o.Name);
                }
                if (o is Group3D)
                {
                    // (o as Group3D).Init();
                }

                if (ObjectClipboard.Items.Count > 1)
                {
                    double offsetX = o.Position.X - collectionCentre.X;
                    double offsetZ = o.Position.Z - collectionCentre.Y;

                    o.Position = new Point3D(targetPoint.X + offsetX, targetPoint.Y, targetPoint.Z + offsetZ);
                }
                else
                {
                    o.Position = new Point3D(targetPoint.X, targetPoint.Y, targetPoint.Z);
                }
                o.Remesh();
                o.MoveToFloor();
                o.CalcScale(false);
                allBounds += o.AbsoluteBounds;
                GeometryModel3D gm = GetMesh(o);
                Document.Content.Add(o);
                Document.Dirty = true;
            }

            RegenerateDisplayList();
            NotificationManager.Notify("ObjectNamesChanged", null);
        }
    }
}