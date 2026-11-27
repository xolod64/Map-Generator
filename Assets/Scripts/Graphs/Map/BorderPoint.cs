using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class BorderPoint
{
    public int id;
    public Vector2Int center;
    public float height;

    public List<Border> borders;

    public BorderPoint(int centerX, int centerY)
    {
        center = new Vector2Int(centerX, centerY);

        borders = new List<Border>();
    }
}