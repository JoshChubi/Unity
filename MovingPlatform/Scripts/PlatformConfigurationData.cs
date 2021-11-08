using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformConfigurationData
{
    public int Row;
    public int Col;
    public float DeltaSpace;
    public float YRange;
    public Global.Colors ColorSpectrum;

    public PlatformConfigurationData(int row, int col, float deltaSpace, float yRange, Global.Colors colorSpectrum)
    {
        this.Row = row;
        this.Col = col;
        this.DeltaSpace = deltaSpace;
        this.YRange = yRange;
        this.ColorSpectrum = colorSpectrum;
    }

    // Copy Constructor
    public PlatformConfigurationData(PlatformConfigurationData pcd)
    {
        this.Row = pcd.Row;
        this.Col = pcd.Col;
        this.DeltaSpace = pcd.DeltaSpace;
        this.YRange = pcd.YRange;
        this.ColorSpectrum = pcd.ColorSpectrum;
    }

    public string ToString()
    {
        return string.Format("{0} {1} {2} {3} {4}", this.Row, this.Col, this.DeltaSpace, this.YRange, (int)this.ColorSpectrum);
    }
}
