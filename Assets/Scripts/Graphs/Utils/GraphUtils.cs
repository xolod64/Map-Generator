using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GraphUtils
{
    //regions----------------------------------------------------------
    public static List<Region> GetGraphBorder(List<Region> initialRegions, HashSet<Region> initialRegionsHash)
    {
        List<Region> graphBorder = new List<Region>();

        HashSet<Region> checkedRegions = new HashSet<Region>();
        Queue<Region> frontier = new Queue<Region>();

        foreach (Region currentRegion in initialRegions)
        {
            if (IsRegionOnGraphBorder(currentRegion, initialRegionsHash))
            {
                frontier.Enqueue(currentRegion);
                checkedRegions.Add(currentRegion);

                break;
            }
        }

        while (frontier.Count > 0)
        {
            Region currentRegion = frontier.Dequeue();

            foreach (Road currentEdge in currentRegion.roads)
            {
                Region neighborRegion = currentEdge.GetNeighbor(currentRegion);

                if (!checkedRegions.Contains(neighborRegion) && IsRegionOnGraphBorder(neighborRegion, initialRegionsHash))
                {
                    frontier.Enqueue(neighborRegion);
                    checkedRegions.Add(neighborRegion);
                }
            }

            graphBorder.Add(currentRegion);
        }

        return graphBorder;
    }

    public static bool IsRegionOnGraphBorder(Region currentRegion, HashSet<Region> initialRegionsHash)
    {
        foreach (Road currentRoad in currentRegion.roads)
        {
            Region neighborRegion = currentRoad.GetNeighbor(currentRegion);

            if (neighborRegion.isPadding || !initialRegionsHash.Contains(neighborRegion)) return true;
        }

        return false;
    }

    public static List<Region> GetRandomWalkPath(Region startRegion, HashSet<Region> endRegions, HashSet<Region> initialRegionsHash, System.Random random)
    {
        List<Region> path = new List<Region>();

        bool pathDone = false;

        List<Region> availableRegions = new List<Region>();

        while (!pathDone)
        {
            availableRegions.Clear();

            foreach (Road currentRoad in startRegion.roads)
            {
                if (currentRoad.border.isDiagonal) continue;
                
                Region candidate = currentRoad.GetNeighbor(startRegion);

                if (initialRegionsHash.Contains(candidate) && !endRegions.Contains(candidate)) availableRegions.Add(candidate);
            }

            if (availableRegions.Count == 0)
            {
                pathDone = true;
            }
            else
            {
                Region nextRegion = availableRegions[random.Next(0, availableRegions.Count)];

                path.Add(nextRegion);
                endRegions.Add(nextRegion);
                startRegion = nextRegion;
            }
        }

        return path;
    }

    public static bool IsRegionIsolated(Region currentRegion, HashSet<Region> insulatingRegions)
    {
        foreach (Road currentRoad in currentRegion.roads)
        {
            if (!insulatingRegions.Contains(currentRoad.GetNeighbor(currentRegion))) return false;
        }

        return true;
    }

    public static List<Region> GetNEquidistantRegions(int count, RegionsGraph initialGraph, System.Random random)
    {
        List<Region> equidistantRegions = new List<Region>(count);
        HashSet<int> usedRegions = new HashSet<int>(count);

        int[] distances = new int[initialGraph.regions.Count];
        Array.Fill(distances, int.MaxValue);

        Region current = initialGraph.regions[random.Next(0, initialGraph.regions.Count)];
        equidistantRegions.Add(current);
        usedRegions.Add(current.id);
        count--;

        while (count > 0)
        {
            current = CalculateDistanceAndReturnFarest(current);
            equidistantRegions.Add(current);
            usedRegions.Add(current.id);
            count--;
        }

        return equidistantRegions;

        Region CalculateDistanceAndReturnFarest(Region startRegion)
        {
            Queue<Region> queue = new Queue<Region>();
            HashSet<Region> checkedRegions = new HashSet<Region>();

            queue.Enqueue(startRegion);
            checkedRegions.Add(startRegion);
            distances[startRegion.id] = 0;

            while (queue.Count > 0)
            {
                Region dequeued = queue.Dequeue();

                foreach (Road road in dequeued.roads)
                {
                    Region neighbor = road.GetNeighbor(dequeued);

                    if (!checkedRegions.Contains(neighbor))
                    {
                        checkedRegions.Add(neighbor);

                        if (distances[neighbor.id] > distances[dequeued.id])
                        {
                            queue.Enqueue(neighbor);
                            distances[neighbor.id] = distances[dequeued.id] + 1;
                        }
                    }
                }
            }

            int farestIndex = startRegion.id;

            for (int i = 0; i < initialGraph.regions.Count; i++) if (distances[i] > distances[farestIndex] && !usedRegions.Contains(i)) farestIndex = i;

            return initialGraph.regions[farestIndex];
        }
    }

    public static List<Border> GetAllVisitedBordersFromSeaPathes(Map map, RegionsGraph initialGraph)
    {
        List<Border> bordersOnPath = new List<Border>();
        HashSet<Region> checkedRegions = new HashSet<Region>();
        PriorityQueue<Region> queue = new PriorityQueue<Region>();

        foreach (Region region in initialGraph.regions)
        {
            foreach (Road road in region.roads)
            {
                Region neighbor = road.GetNeighbor(region);

                if (neighbor.isSea)
                {
                    queue.Enqueue(region, (int)(map.regionsTectonicForces[region.id] * 100));
                    break;
                }
            }
        }

        while (queue.Count > 0)
        {
            Region region = queue.Dequeue();

            foreach (Road road in region.roads)
            {
                if (road.border.isDiagonal) continue;

                Region neighbor = road.GetNeighbor(region);

                if (!checkedRegions.Contains(neighbor) && initialGraph.regionsHash.Contains(neighbor))
                {
                    queue.Enqueue(neighbor, (int)(map.regionsTectonicForces[neighbor.id] * 100));
                    checkedRegions.Add(neighbor);

                    bordersOnPath.Add(road.border);

                    break;
                }
            }

            foreach (Road road in region.roads)
            {
                if (road.border.isDiagonal) continue;

                Region neighbor = road.GetNeighbor(region);

                if (!checkedRegions.Contains(neighbor) && initialGraph.regionsHash.Contains(neighbor))
                {
                    queue.Enqueue(region, (int)(map.regionsTectonicForces[region.id] * 100));
                    break;
                }
            }
        }

        return bordersOnPath;
    }

    //clusters
    public static (List<RegionsGraph>, List<BordersGraph>) GetIsolatedClusters(RegionsGraph initialGraph, bool isTrueIsulating, Func<Region, bool> condition)
    {
        List<RegionsGraph> clusters = new List<RegionsGraph>();
        List<BordersGraph> clustersBorders = new List<BordersGraph>();

        HashSet<Region> candidates = new HashSet<Region>(initialGraph.regions);
        HashSet<Region> checkedRegions = new HashSet<Region>();

        foreach (Region currentRegion in candidates.ToArray())
        {
            if (isTrueIsulating ? condition(currentRegion) : !condition(currentRegion)) candidates.Remove(currentRegion);
        }

        foreach (Region candidate in candidates)
        {
            if (checkedRegions.Contains(candidate)) continue;
            List<Region> newClusterRegions = new List<Region>();
            Queue<Region> queue = new Queue<Region>();
            queue.Enqueue(candidate);
            checkedRegions.Add(candidate);

            while (queue.Count > 0)
            {
                Region currentRegion = queue.Dequeue();

                newClusterRegions.Add(currentRegion);

                foreach (Road currentRoad in currentRegion.roads)
                {
                    Region neighborRegion = currentRoad.GetNeighbor(currentRegion);

                    if (!checkedRegions.Contains(neighborRegion) && candidates.Contains(neighborRegion))
                    {
                        queue.Enqueue(neighborRegion);
                        checkedRegions.Add(neighborRegion);
                    }
                }
            }

            RegionsGraph newCluster = new RegionsGraph(newClusterRegions);
            clusters.Add(newCluster);

            BordersGraph newClusterBorders = RegionsGraphToBordersGraph(newCluster);
            clustersBorders.Add(newClusterBorders);
        }

        return (clusters, clustersBorders);
    }

    public static (List<RegionsGraph>, List<BordersGraph>) GetRandomDividedClusters(RegionsGraph initialGraph, List<Region> seeds, System.Random random)
    {
        List<RegionsGraph> clusters = new List<RegionsGraph>();
        List<BordersGraph> clustersBorders = new List<BordersGraph>();

        List<int> avaibleSeeds = new List<int>();
        HashSet<Region> candidates = new HashSet<Region>(initialGraph.regions);
        Dictionary<int, List<Region>> belonging = new Dictionary<int, List<Region>>();
        Dictionary<int, List<Region>> frontiers = new Dictionary<int, List<Region>>();

        foreach (Region seed in seeds)
        {
            avaibleSeeds.Add(seed.id);
            frontiers.Add(seed.id, new List<Region>());
            belonging.Add(seed.id, new List<Region>());

            frontiers[seed.id].Add(seed);
            candidates.Remove(seed);
        }

        while (avaibleSeeds.Count > 0)
        {
            int randomIndex = random.Next(0, avaibleSeeds.Count);
            int currentClusterId = avaibleSeeds[randomIndex];

            if (frontiers[currentClusterId].Count == 0)
            {
                avaibleSeeds.RemoveAt(randomIndex);
                continue;
            }

            randomIndex = random.Next(0, frontiers[currentClusterId].Count);
            Region current = frontiers[currentClusterId][randomIndex];
            frontiers[currentClusterId].RemoveAt(randomIndex);
            belonging[currentClusterId].Add(current);

            foreach (Road road in current.roads)
            {
                Region neighbor = road.GetNeighbor(current);

                if (candidates.Contains(neighbor))
                {
                    frontiers[currentClusterId].Add(neighbor);
                    candidates.Remove(neighbor);
                }
            }
        }

        foreach (Region seed in seeds)
        {
            RegionsGraph newCluster = new RegionsGraph(belonging[seed.id]);
            clusters.Add(newCluster);

            BordersGraph newClusterBorders = RegionsGraphToBordersGraph(newCluster);
            clustersBorders.Add(newClusterBorders);
        }

        return (clusters, clustersBorders);
    }

    // public static Dictionary<int, List<int>> DefineClustersNeighbors(List<RegionsGraph> clusters)
    // {
    //     Dictionary<int, List<int>> indexNeighbors = new Dictionary<int, List<int>>();

    //     return indexNeighbors;
    // }

    //borders----------------------------------------------------------
    public static bool[,] GetAllBordersCells(Map map)
    {
        bool[,] isCellAtBorder = new bool[map.sizeInCells, map.sizeInCells];
        List<Vector2Int> line = new List<Vector2Int>(map.regionSize);

        foreach (Border border in map.bordersGraph.borders)
        {
            line.Clear();
            GetCellsOnLine(line, map, border.firstPoint.center, border.secondPoint.center);

            foreach (Vector2Int coords in line)
            {
                isCellAtBorder[coords.x, coords.y] = true;
            }
        }

        return isCellAtBorder;
    }

    public static List<BorderPoint> GetNEquidistantBorderPoints(int count, BordersGraph initialGraph, System.Random random)
    {
        List<BorderPoint> equidistantPoints = new List<BorderPoint>(count);
        HashSet<int> usedPoints = new HashSet<int>(count);

        int[] distances = new int[initialGraph.points.Count];
        Array.Fill(distances, int.MaxValue);

        BorderPoint current = initialGraph.points[random.Next(0, initialGraph.points.Count)];
        equidistantPoints.Add(current);
        usedPoints.Add(current.id);
        count--;

        while (count > 0)
        {
            current = CalculateDistanceAndReturnFarest(current);
            equidistantPoints.Add(current);
            usedPoints.Add(current.id);
            count--;
        }

        return equidistantPoints;

        BorderPoint CalculateDistanceAndReturnFarest(BorderPoint startPoint)
        {
            Queue<BorderPoint> queue = new Queue<BorderPoint>();
            HashSet<BorderPoint> checkedPoints = new HashSet<BorderPoint>();

            queue.Enqueue(startPoint);
            checkedPoints.Add(startPoint);
            distances[startPoint.id] = 0;

            while (queue.Count > 0)
            {
                BorderPoint dequeued = queue.Dequeue();

                foreach (Border border in dequeued.borders)
                {
                    BorderPoint neighbor = border.GetNeighbor(dequeued);

                    if (!checkedPoints.Contains(neighbor))
                    {
                        checkedPoints.Add(neighbor);

                        if (distances[neighbor.id] > distances[dequeued.id])
                        {
                            queue.Enqueue(neighbor);
                            distances[neighbor.id] = distances[dequeued.id] + 1;
                        }
                    }
                }
            }

            int farestIndex = startPoint.id;

            for (int i = 0; i < initialGraph.points.Count; i++) if (distances[i] > distances[farestIndex] && !usedPoints.Contains(i)) farestIndex = i;

            return initialGraph.points[farestIndex];
        }
    }

    public static List<List<BorderPoint>> GetRandomDividedBorderPointsClusters(BordersGraph initialGraph, List<BorderPoint> seeds, System.Random random)
    {
        List<List<BorderPoint>> clusters = new List<List<BorderPoint>>();

        List<int> avaibleSeeds = new List<int>();
        HashSet<BorderPoint> candidates = new HashSet<BorderPoint>(initialGraph.points);
        Dictionary<int, List<BorderPoint>> belonging = new Dictionary<int, List<BorderPoint>>();
        Dictionary<int, List<BorderPoint>> frontiers = new Dictionary<int, List<BorderPoint>>();

        foreach (BorderPoint seed in seeds)
        {
            avaibleSeeds.Add(seed.id);
            frontiers.Add(seed.id, new List<BorderPoint>());
            belonging.Add(seed.id, new List<BorderPoint>());

            frontiers[seed.id].Add(seed);
            candidates.Remove(seed);
        }

        while (avaibleSeeds.Count > 0)
        {
            int randomIndex = random.Next(0, avaibleSeeds.Count);
            int currentClusterId = avaibleSeeds[randomIndex];

            if (frontiers[currentClusterId].Count == 0)
            {
                avaibleSeeds.RemoveAt(randomIndex);
                continue;
            }

            randomIndex = random.Next(0, frontiers[currentClusterId].Count);
            BorderPoint current = frontiers[currentClusterId][randomIndex];
            frontiers[currentClusterId].RemoveAt(randomIndex);
            belonging[currentClusterId].Add(current);

            foreach (Border border in current.borders)
            {
                BorderPoint neighbor = border.GetNeighbor(current);

                if (candidates.Contains(neighbor))
                {
                    frontiers[currentClusterId].Add(neighbor);
                    candidates.Remove(neighbor);
                }
            }
        }

        foreach (BorderPoint seed in seeds)
        {
            clusters.Add(belonging[seed.id]);
        }

        return clusters;
    }

    public static List<Border> GetBorderNeighbors(List<Border> neighbors, Border border)
    {
        foreach (Border candidate in border.firstPoint.borders)
        {
            if (candidate != border) neighbors.Add(candidate);
        }

        foreach (Border candidate in border.secondPoint.borders)
        {
            if (candidate != border) neighbors.Add(candidate);
        }

        return neighbors;
    }

    //cells----------------------------------------------------------
    public static void GetCellsOnLine(List<Vector2Int> line, Map map, Vector2Int startCoords, Vector2Int endCoords)
    {
        (int x0, int y0) = startCoords;
        (int x1, int y1) = endCoords;

        // Обчислюємо абсолютну відстань по осях
        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);

        // Визначаємо напрямок руху (+1 або -1)
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;

        // Початкова помилка зсуву
        int err = dx - dy;

        while (true)
        {
            // Додаємо поточну точку в список
            line.Add(new Vector2Int(x0, y0));

            // Якщо дійшли до кінцевої точки — зупиняємось
            if (x0 == x1 && y0 == y1) break;

            int e2 = 2 * err;

            // Крок по осі X
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }

            // Крок по осі Y
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    public static void GetCellsOnLineExeptEnds(List<Vector2Int> line, Map map, Vector2Int startCoords, Vector2Int endCoords)
    {
        (int x0, int y0) = startCoords;
        (int x1, int y1) = endCoords;

        // Обчислюємо абсолютну відстань по осях
        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);

        // Визначаємо напрямок руху (+1 або -1)
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;

        // Початкова помилка зсуву
        int err = dx - dy;

        int e2 = 2 * err;

        // Крок по осі X
        if (e2 > -dy)
        {
            err -= dy;
            x0 += sx;
        }

        // Крок по осі Y
        if (e2 < dx)
        {
            err += dx;
            y0 += sy;
        }

        while (true)
        {
            // Якщо дійшли до кінцевої точки — зупиняємось
            if (x0 == x1 && y0 == y1) break;

            // Додаємо поточну точку в список
            line.Add(new Vector2Int(x0, y0));

            e2 = 2 * err;

            // Крок по осі X
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }

            // Крок по осі Y
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    //convert----------------------------------------------------------
    public static BordersGraph RegionsGraphToBordersGraph(RegionsGraph initialGraph)
    {
        HashSet<Border> newGraphBorders = new HashSet<Border>();

        foreach (Road road in initialGraph.roads)
        {
            newGraphBorders.Add(road.border);
        }

        return new BordersGraph(newGraphBorders);
    }
}