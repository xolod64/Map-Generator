using System;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static List<Vector2Int> TwoDimentionalArrayBounds(int xSize, int ySize)
    {
        List<Vector2Int> coords = new List<Vector2Int>(xSize * 2 + ySize * 2 - 4);

        for (int x = 0; x < xSize - 1; x++)
        {
            int y = 0;
            coords.Add(new Vector2Int(x, y));
        }

        for (int y = 0; y < ySize - 1; y++)
        {
            int x = xSize - 1;
            coords.Add(new Vector2Int(x, y));
        }

        for (int x = xSize - 1; x > 0; x--)
        {
            int y = ySize - 1;
            coords.Add(new Vector2Int(x, y));
        }

        for (int y = ySize - 1; y > 0; y--)
        {
            int x = 0;
            coords.Add(new Vector2Int(x, y));
        }

        return coords;
    }

    public static Vector2Int[] ortNeighborsCoords = { new(0, 1), new(1, 0), new(0, -1), new(-1, 0) };

    public static float DegToRad(float deg)
    {
        return deg * (float)Math.PI / 180f;
    }

    public static void Shuffle<T>(this List<T> list, System.Random random)
    {
        int count = list.Count;
        
        for (int i = count - 1; i > 0; i--)
        {
            int randomIndex = random.Next(0, i + 1);

            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}