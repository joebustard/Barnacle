﻿using Make3D.Dialogs;

namespace Make3D.Models
{
    public class ToolFactory
    {
        public static BaseModellerDialog MakeTool(string name)
        {
            BaseModellerDialog res = null;
            switch (name)
            {
                case "Wheel":
                    res = new Wheel();
                    break;

                case "Filet":
                    res = new FiletDlg();
                    break;

                case "Scribble":
                    res = new Scribble();
                    break;

                case "LinearLoft":
                    res = new LinearLoftDialog();
                    break;

                case "TwoShape":
                    res = new ShapeLoftDialog();
                    break;

                case "BezierRing":
                    res = new BezierRingDlg();
                    break;

                case "Platelet":
                    res = new PlateletDlg();
                    break;

                case "Torus":
                    res = new TorusDialog();
                    break;

                case "SpurGear":
                    res = new SpurGearDialog();
                    break;

                case "Stadium":
                    res = new StadiumDialog();
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

                case "BezierFuselage":
                    res = new FuselageLoftDialog();
                    break;

                case "ProfileFuselage":
                    res = new ProfileFuselageDlg();
                    break;

                case "Wing":
                    res = new WingDlg();
                    break;

                case "Propeller":
                    res = new Propeller();
                    break;

                case "TurboFan":
                    res = new TurboFan();
                    break;
            }
            return res;
        }
    }
}