using System.Collections.Generic;
using UnityEngine;

public static class MapGenerator
{
    public static Map GenerateMap(int seed, int width, int height, float mountainess)
    {
        Map newMap = new Map(width, height);
        System.Random random = new System.Random(seed);

        newMap.heightmap = GenerateHeightmap(width, height, mountainess, random);
        return newMap;
    }

    public static float[,] GenerateHeightmap(int width, int height, float mountainess, System.Random random)
    {
        float[,] newHeightmap = new float[width, height];

        float step = 0.1763f;

        bool[,] visited = new bool[width, height];
        List<Vector2Int> startPoints = Utils.TwoDimentionalArrayBounds(width, height);
        List<Vector2Int> neighborBuffer = new List<Vector2Int>(4);

        int visitedCount = startPoints.Count;
        int totalCount = width * height;

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

            float visitedRatio = (float)visitedCount / totalCount;
            float currentStep = step * mountainess * visitedRatio * visitedRatio;

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

            if (neighborBuffer.Count < 2) startPoints.SwapBackAndRemove(i);
            if (neighborBuffer.Count > 0)
            {
                neighborBuffer.Shuffle(random);

                Vector2Int neighbor = neighborBuffer[0];
                int nX = neighbor.x;
                int nY = neighbor.y;

                startPoints.Add(neighbor);
                visited[nX, nY] = true;
                newHeightmap[nX, nY] = newHeightmap[x, y] + currentStep;

                visitedCount++;
            }
        }

        return newHeightmap;
    }
}
