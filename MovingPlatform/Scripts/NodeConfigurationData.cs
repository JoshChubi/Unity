using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeConfigurationData
{
    public int Row;
    public int Col;
    public float Height;

    public NodeConfigurationData(NodeConfigurationData NCD)
    {
        this.Row = NCD.Row;
        this.Col = NCD.Col;
        this.Height = NCD.Height;
    }

    public NodeConfigurationData(int row, int col, float height)
    {
        this.Row = row;
        this.Col = col;
        this.Height = height;
    }

    public string ToString()
    {
        return string.Format("{0} {1} {2}", this.Row, this.Col, this.Height);
    }
}
