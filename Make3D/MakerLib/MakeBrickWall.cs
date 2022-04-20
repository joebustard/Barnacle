using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class BrickWallMaker : MakerBase
    {
        
    private double wallLength ;
    private double wallHeight ;
    private double wallWidth ;
    private double largeBrickLength ;
    private double smallBrickLength ;
    private double brickHeight ;
    private double mortarGap ;

        public BrickWallMaker(double wallLength, double wallHeight, double wallWidth, double largeBrickLength, double smallBrickLength, double brickHeight, double mortarGap)
    {
                    this.wallLength = wallLength ;
        this.wallHeight = wallHeight ;
        this.wallWidth = wallWidth ;
        this.largeBrickLength = largeBrickLength ;
        this.smallBrickLength = smallBrickLength ;
        this.brickHeight = brickHeight ;
        this.mortarGap = mortarGap ;

        }

public void Generate(Point3DCollection pnts, Int32Collection faces)
{
    pnts.Clear();
    faces.Clear();
    Vertices = pnts;
    Faces = faces;
}
}
}
