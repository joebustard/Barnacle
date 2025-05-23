﻿// **************************************************************************
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
using Barnacle.Dialogs.RibbedFuselage;
using System.Windows;

namespace Barnacle.Models
{
    public class ToolFactory
    {
        public static BaseModellerDialog MakeTool(string name)
        {
            BaseModellerDialog res = null;
            switch (name)
            {
                case "Barrel":
                    res = new BarrelDlg();
                    break;

                case "BezierFuselage":
                    res = new FuselageLoftDialog();
                    break;

                case "BezierRing":
                    res = new BezierRingDlg();
                    break;

                case "BezierSurface":
                    res = new BezierSurfaceDlg();
                    break;

                case "Box":
                    res = new BoxDlg();
                    break;

                case "Bicorn":
                    res = new BicornDlg();
                    break;

                case "BrickTower":
                    res = new BrickTowerDlg();
                    break;

                case "BrickWall":
                    res = new BrickWallDlg();
                    break;

                case "ConstructionStrip":
                    res = new ConstructionStripDlg();
                    break;

                case "CrossGrille":
                    res = new CrossGrilleDlg();
                    break;

                case "CanvasWing":
                    res = new CanvasWingDlg();
                    break;

                case "ClaySculpt":
                    res = new ClaySculptDlg();
                    // res = new MeshEditorDlg();
                    break;

                case "CurvedFunnel":
                    res = new CurvedFunnelDlg();
                    break;

                case "Dual":
                    res = new DualProfileDlg();
                    break;

                case "ImagePlaque":
                    res = new ImagePlaqueDlg();
                    break;

                case "Morphable":
                    res = new MorphableModelDlg();
                    break;

                case "ObliqueEndCylinder":
                    res = new ObliqueEndCylinderDlg();
                    break;

                case "ParabolicDish":
                    res = new ParabolicDishDlg();
                    break;

                case "PathLoft":
                    res = new PathLoftDlg();
                    break;

                case "Parallelogram":
                    res = new PGramDlg();
                    break;

                case "Pie":
                    res = new PieDlg();
                    break;

                case "Pill":
                    res = new PillDlg();
                    break;

                case "Platelet":
                    res = new Platelet();
                    break;

                case "PlankWall":
                    res = new PlankWallDlg();
                    break;

                case "ProfileFuselage":
                    // res = new ProfileFuselageDlg();
                    res = new RibbedFuselageDlg();
                    break;

                case "Propeller":
                    res = new Propeller();
                    break;

                case "Pulley":
                    res = new PulleyDlg();
                    break;

                case "RailWheel":
                    res = new RailWheelDlg();
                    break;

                case "RectGrille":
                    res = new RectGrilleDlg();
                    break;

                case "RoundGrille":
                    res = new RoundGrilleDlg();
                    break;

                case "RoofRidge":
                    res = new RoofRidgeDlg();
                    break;

                case "Reuleaux":
                    res = new ReuleauxDlg();
                    break;

                case "SdfModel":
                    res = new SdfModelDlg();
                    break;

                case "Tray":
                    res = new TrayDlg();
                    break;

                case "ShapedBrickWall":
                    res = new ShapedBrickWallDlg();
                    break;

                case "ShapedTiledRoof":
                    res = new ShapedTiledRoofDlg();
                    break;

                case "ShapedWing":
                    res = new ShapedWingDlg();
                    break;

                case "Spring":
                    res = new SpringDlg();
                    break;

                case "SpurGear":
                    res = new SpurGearDialog();
                    break;

                case "Stadium":
                    res = new StadiumDialog();
                    break;

                case "Star":
                    res = new Star();
                    break;

                case "SquaredStadium":
                    res = new SquaredStadiumDlg();
                    break;

                case "Squirkle":
                    res = new SquirkleDlg();
                    break;

                case "StoneWall":
                    res = new StoneWallDlg();
                    break;

                case "Symbol":
                    res = new SymbolDlg();
                    break;

                case "Text":
                    res = new TextDlg();
                    break;

                case "TexturedTube":
                    res = new TexturedTubeDlg();
                    break;

                case "TiledRoof":
                    res = new TiledRoofDlg();
                    break;

                case "Trickle":
                    res = new TrickleDlg();
                    break;

                case "TwoShape":
                    res = new ShapeLoftDialog();
                    break;

                case "Torus":
                    res = new TorusDialog();
                    break;

                case "TankTrack":
                    //res = new TrackDialog();
                    res = new TankDialog2();
                    break;

                case "Thread":
                    res = new ThreadDlg();
                    break;

                case "Trapezoid":
                    res = new TrapezoidDlg();
                    break;

                case "Tube":
                    res = new TubeDlg();
                    break;

                case "TurboFan":
                    res = new TurboFan();
                    break;

                case "VaseLoft":
                    res = new VaseLoftDlg();
                    break;

                case "WagonWheel":
                    res = new WagonWheelDlg();
                    break;

                case "Wheel":
                    res = new Wheel();
                    break;

                case "Wing":
                    res = new WingDlg();
                    break;
            }
            if (res != null)
            {
                res.Owner = Application.Current.MainWindow;
                res.RestoreSizeAndLocation(true);
            }
            return res;
        }
    }
}