using System.Collections.Generic;
using UnityEngine;

public class Region
{
    public int id;
    public List<Vector2Int> cellsCoords;

    //graph properties
    public Vector2Int center;
    public List<Road> roads;
    public bool isPadding;

    //landscape properties
    public bool isSea;
    public Color color;
    public float height;

    public Region(int centerX, int centerY, int sizeInCells)
    {
        cellsCoords = new List<Vector2Int>(sizeInCells * sizeInCells);

        center = new Vector2Int(centerX, centerY);
        roads = new List<Road>();

        isPadding = true;
    }
}
