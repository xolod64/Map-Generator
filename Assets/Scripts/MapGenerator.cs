using System.Collections.Generic;
using UnityEngine;

public static class MapGenerator
{
    public static void GenerateHeightmap(float[,] heightmap, float mountainess, System.Random random)
    {
        float step = 0.1763f;

        int width = heightmap.GetLength(0);
        int height = heightmap.GetLength(1);

        bool[,] visited = new bool[width, height];
        List<Vector2Int> startPoints = Utils.TwoDimentionalArrayBounds(width, height);
        List<Vector2Int> neighborBuffer = new List<Vector2Int>(4);

        foreach (Vector2Int coord in startPoints)
        {
            int x = coord.x;
            int y = coord.y;

            visited[x, y] = true;
        }

        while (startPoints.Count > 0)
        {
            int i = random.Next(0, startPoints.Count);
            neighborBuffer.Clear();

            Vector2Int currentPoint = startPoints[i];
            int x = currentPoint.x;
            int y = currentPoint.y;

            foreach (Vector2Int neighbor in Utils.ortNeighborsCoords)
            {
                int nX = x + neighbor.x;
                int nY = y + neighbor.y;

                if (nX < 0 || nX > width - 1 || nY < 0 || nY > height - 1) continue;

                if (!visited[nX, nY]) neighborBuffer.Add(new Vector2Int(nX, nY));
            }

            
        }
    }
}
