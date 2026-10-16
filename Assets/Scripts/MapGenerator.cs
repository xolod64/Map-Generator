using System.Collections.Generic;
using UnityEngine;

public static class MapGenerator
{
    public static float[,] WaterfrontHeightmap(int width, int height, float mountainess, int[,] priority, System.Random random)
    {
        float[,] newHeightmap = new float[width, height];

        float step = 0.1763f;

        bool[,] visited = new bool[width, height];
        // List<Vector2Int> startPoints = Utils.TwoDimentionalArrayBounds(width, height);
        PriorityQueue<Vector2Int> startPoints = new PriorityQueue<Vector2Int>();
        List<Vector2Int> neighborBuffer = new List<Vector2Int>(4);

        int visitedCount = startPoints.Count;
        int totalCount = width * height;

        foreach (Vector2Int coord in Utils.TwoDimentionalArrayBounds(width, height))
        {
            int x = coord.x;
            int y = coord.y;

            startPoints.Enqueue(coord, priority[x, y]);
            visited[x, y] = true;
            visitedCount++;
        }

        while (startPoints.Count > 0)
        {
            // int i = random.Next(0, startPoints.Count);
            neighborBuffer.Clear();

            float visitedRatio = (float)visitedCount / totalCount;
            float currentStep = step * mountainess * visitedRatio * visitedRatio;

            // Vector2Int currentPoint = startPoints[i];
            Vector2Int currentPoint = startPoints.Dequeue();
            int x = currentPoint.x;
            int y = currentPoint.y;

            foreach (Vector2Int neighbor in Utils.ortNeighborsCoords)
            {
                int nX = x + neighbor.x;
                int nY = y + neighbor.y;

                if (nX < 0 || nX > width - 1 || nY < 0 || nY > height - 1) continue;

                if (!visited[nX, nY]) neighborBuffer.Add(new Vector2Int(nX, nY));
            }

            // if (neighborBuffer.Count < 2) startPoints.SwapBackAndRemove(i);
            if (neighborBuffer.Count > 1) startPoints.Enqueue(currentPoint, priority[x, y]);
            if (neighborBuffer.Count > 0)
            {
                neighborBuffer.Shuffle(random);

                Vector2Int neighbor = neighborBuffer[0];
                int nX = neighbor.x;
                int nY = neighbor.y;

                // startPoints.Add(neighbor);
                startPoints.Enqueue(neighbor, priority[nX, nY]);
                visited[nX, nY] = true;
                newHeightmap[nX, nY] = newHeightmap[x, y] + currentStep;

                visitedCount++;
            }
        }

        return newHeightmap;
    }

    public static float[,] PerlinNoiseHeightmap(int width, int height, int octaves, float persistence, float roughness, float scale, System.Random random)
    {
        float[,] newHeightmap = new float[width, height];

        float offsetX = (float)random.NextDouble() * 10000f;
        float offsetY = (float)random.NextDouble() * 10000f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float sampleX = x + offsetX;
                float sampleY = y + offsetY;
                newHeightmap[x, y] = GetFractalNoise(sampleX, sampleY, octaves, persistence, roughness, scale);
            }
        }

        return newHeightmap;

        float GetFractalNoise(float x, float y, int octaves, float persistence, float roughness, float scale)
        {
            float total = 0;
            float frequency = scale;
            float amplitude = 1;
            float maxVal = 0;

            for (int i = 0; i < octaves; i++)
            {
                total += Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude;

                maxVal += amplitude;
                amplitude *= persistence;
                frequency *= roughness;
            }

            return total / maxVal;
        }
    }
}
