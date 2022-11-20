using Barnacle.Dialogs;
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
                case "Dual":
                    res = new DualProfileDlg();
                    break;

                case "BrickWall":
                    res = new BrickWallDlg();
                    break;

                case "ShapedBrickWall":
                    res = new ShapedBrickWallDlg();
                    break;

                case "WagonWheel":
                    res = new WagonWheelDlg();
                    break;

                case "Reuleaux":
                    res = new ReuleauxDlg();
                    break;

                case "RailWheel":
                    res = new RailWheelDlg();
                    break;

                case "Wheel":
                    res = new Wheel();
                    break;

                case "SquaredStadium":
                    res = new SquaredStadiumDlg();
                    break;

                case "Filet":
                    res = new FiletDlg();
                    break;

                case "Text":
                    res = new TextDlg();
                    break;

                case "Squirkle":
                    res = new SquirkleDlg();
                    break;

                case "StoneWall":
                    res = new StoneWallDlg();
                    break;

                case "PlankWall":
                    res = new PlankWallDlg();
                    break;

                case "Trickle":
                    res = new TrickleDlg();
                    break;

                case "TiledRoof":
                    res = new TiledRoofDlg();
                    break;

                case "ShapedTiledRoof":
                    res = new ShapedTiledRoofDlg();
                    break;

                case "Scribble":
                    res = new ScribbleDlg();
                    break;

                case "TwoShape":
                    res = new ShapeLoftDialog();
                    break;

                case "BezierRing":
                    res = new BezierRingDlg();
                    break;

                case "Platelet":
                    res = new Platelet();
                    break;

                case "Parallelogram":
                    res = new PGramDlg();
                    break;

                case "ParabolicDish":
                    res = new ParabolicDishDlg();
                    break;

                case "Torus":
                    res = new TorusDialog();
                    break;

                case "Bicorn":
                    res = new BicornDlg();
                    break;

                case "SpurGear":
                    res = new SpurGearDialog();
                    break;

                case "Stadium":
                    res = new StadiumDialog();
                    break;

                case "Thread":
                    res = new ThreadDlg();
                    break;

                case "Tube":
                    res = new TubeDlg();
                    break;

                case "Star":
                    res = new Star();
                    break;

                case "TankTrack":
                    res = new TrackDialog();
                    break;

                case "Trapezoid":
                    res = new TrapezoidDlg();
                    break;

                case "BezierFuselage":
                    res = new FuselageLoftDialog();
                    break;

                case "ProfileFuselage":
                    res = new ProfileFuselageDlg();
                    break;

                case "Wing":
                    res = new WingDlg();
                    break;

                case "CanvasWing":
                    res = new CanvasWingDlg();
                    break;

                case "Propeller":
                    res = new Propeller();
                    break;

                case "TurboFan":
                    res = new TurboFan();
                    break;

                case "Figure":
                    res = new FigureDlg();
                    break;

                case "BezierSurface":
                    res = new BezierSurfaceDlg();
                    break;

                case "Pulley":
                    res = new PulleyDlg();
                    break;

                case "VaseLoft":
                    res = new VaseLoftDlg();
                    break;

                case "CurvedFunnel":
                    res = new CurvedFunnelDlg();
                    break;
            }
            if (res != null)
            {
                res.Owner = Application.Current.MainWindow;
            }
            return res;
        }
    }
}