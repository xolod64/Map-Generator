using System;
using System.Linq;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using NUnit.Framework;
using Unity.Mathematics;


public static class MapGenerator
{
    public static void GenerateMap(Map map, System.Random random, float seaPercent)
    {
        //local settings
        int sizeInPlates = map.sizeInRegions / 25;

        GC.Collect();
        var timings = new System.Collections.Generic.Dictionary<string, long>();
        var sw = new System.Diagnostics.Stopwatch();
        long before = GC.GetTotalMemory(true);

        sw.Restart();
        GenerateCells();
        timings["GenerateCells"] = sw.ElapsedMilliseconds;

        sw.Restart();
        GenerateRegionsRoadsAndBorders();
        timings["GenerateRegionsRoadsAndBorders"] = sw.ElapsedMilliseconds;

        sw.Restart();
        DivideCellsByRegions();
        timings["DivideCellsByRegions"] = sw.ElapsedMilliseconds;

        sw.Restart();
        GenerateSea();
        timings["GenerateSea"] = sw.ElapsedMilliseconds;

        sw.Restart();
        GenerateLandmasses();
        timings["GenerateLandmasses"] = sw.ElapsedMilliseconds;

        sw.Restart();
        GenerateTectonicForces();
        timings["GenerateTectonicForces"] = sw.ElapsedMilliseconds;

        sw.Restart();
        GenerateBasins();
        timings["GenerateBasins"] = sw.ElapsedMilliseconds;

        sw.Restart();
        GenerateRegionsHeight();
        timings["GenerateRegionsHeight"] = sw.ElapsedMilliseconds;

        sw.Restart();
        GenerateBorderPointsHeight();
        timings["GenerateBorderPointsHeight"] = sw.ElapsedMilliseconds;

        // sw.Restart();
        // CalculateCellsHeights();
        // timings["CalculateCellsHeights"] = sw.ElapsedMilliseconds;

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== Map generation timings ===");
        long total = 0;
        foreach (var kvp in timings)
        {
            sb.AppendLine($"{kvp.Key}: {kvp.Value} ms");
            total += kvp.Value;
        }
        sb.AppendLine($"Total: {total} ms");
        Debug.Log(sb.ToString());

        long after = GC.GetTotalMemory(true);
        Debug.Log($"Memory used by GenerateMap: {(after - before) / 1024 / 1024} MB");

        void GenerateCells()
        {
            map.cells = new Cell[map.sizeInCells, map.sizeInCells];
            map.cellToRegion = new int[map.sizeInCells, map.sizeInCells];
        }

        void GenerateRegionsRoadsAndBorders()
        {
            int sizeInCells = (map.sizeInRegions + 2) * map.regionSize;

            int sizeInRegions = map.sizeInRegions + 2;
            int regionSize = map.regionSize;

            int[,,,] possibleCombinationsMatrix = new int[5, 5, 5, 5];
            Vector2Int[,,,,,] borderCoordsMatrix = new Vector2Int[5, 5, 5, 5, 2, 2];

            float allowedOffsetRadius = regionSize / 8f;
            float minBorderLengthSqr = 4f * 4f; //2.82842712475f * 2.82842712475f;

            //calculate possible combinations
            //diagonal - voronoi edge perpendicular
            //a - current
            //b - left
            //c - down-left
            //d - down
            //possibleCombinationsMatrix == 0 - combination not possible
            //possibleCombinationsMatrix == 1 - current/down-left diagonal
            //possibleCombinationsMatrix == 2 - left/down diagonal
            //first-second circles - left-down diagonal
            //third-fourth circles - current/down-left diagonal
            for (int a = -2; a < 3; a++)
            {
                int aX = (int)(0.5f * a * allowedOffsetRadius);
                int aY = aX;
                Vector2 currentCenter = new Vector2(aX, aY);

                for (int b = -2; b < 3; b++)
                {
                    int bX = (int)(0.5f * b * allowedOffsetRadius) - regionSize;
                    int bY = (int)(0.5f * b * allowedOffsetRadius);
                    Vector2 leftCenter = new Vector2(bX, bY);

                    for (int c = -2; c < 3; c++)
                    {
                        int cX = (int)(0.5f * c * allowedOffsetRadius) - regionSize;
                        int cY = cX;
                        Vector2 downLeftCenter = new Vector2(cX, cY);

                        for (int d = -2; d < 3; d++)
                        {
                            int dX = (int)(0.5f * d * allowedOffsetRadius);
                            int dY = dX - regionSize;
                            Vector2 downCenter = new Vector2(dX, dY);

                            Vector2Int firstCircleCenter = Vector2Int.RoundToInt(Utils.GetCircumcenter(leftCenter, downCenter, downLeftCenter));
                            Vector2Int secondCircleCenter = Vector2Int.RoundToInt(Utils.GetCircumcenter(leftCenter, downCenter, currentCenter));
                            Vector2Int thirdCircleCenter = Vector2Int.RoundToInt(Utils.GetCircumcenter(currentCenter, downLeftCenter, leftCenter));
                            Vector2Int fourthCircleCenter = Vector2Int.RoundToInt(Utils.GetCircumcenter(currentCenter, downLeftCenter, downCenter));

                            float firstCirclesDist = (firstCircleCenter - secondCircleCenter).sqrMagnitude;
                            float secondCirclesDist = (thirdCircleCenter - fourthCircleCenter).sqrMagnitude;

                            int combination = 0;

                            if (firstCirclesDist >= minBorderLengthSqr && secondCirclesDist >= minBorderLengthSqr)
                            {
                                combination = firstCirclesDist > secondCirclesDist ? 1 : 2;
                            }
                            else if (firstCirclesDist >= minBorderLengthSqr)
                            {
                                combination = 2;
                            }
                            else if (secondCirclesDist >= minBorderLengthSqr)
                            {
                                combination = 1;
                            }

                            possibleCombinationsMatrix[a + 2, b + 2, c + 2, d + 2] = combination;

                            Vector2Int firstBorderNode;
                            Vector2Int secondBorderNode;

                            if (combination == 1)
                            {
                                firstBorderNode = thirdCircleCenter;
                                secondBorderNode = fourthCircleCenter;
                            }
                            else
                            {
                                firstBorderNode = firstCircleCenter;
                                secondBorderNode = secondCircleCenter;
                            }

                            bool isFirstLefter = firstBorderNode.x < secondBorderNode.x || (firstBorderNode.x == secondBorderNode.x && firstBorderNode.y > secondBorderNode.y);

                            Vector2Int lefterPoint = isFirstLefter ? firstBorderNode : secondBorderNode;
                            Vector2Int righterPoint = isFirstLefter ? secondBorderNode : firstBorderNode;

                            bool isLeftUpper = lefterPoint.y > righterPoint.y;

                            Vector2Int upperPoint = isLeftUpper ? lefterPoint : righterPoint;
                            Vector2Int lowerPoint = isLeftUpper ? righterPoint : lefterPoint;

                            borderCoordsMatrix[a + 2, b + 2, c + 2, d + 2, 0, 0] = lefterPoint;
                            borderCoordsMatrix[a + 2, b + 2, c + 2, d + 2, 0, 1] = righterPoint;
                            borderCoordsMatrix[a + 2, b + 2, c + 2, d + 2, 1, 0] = upperPoint;
                            borderCoordsMatrix[a + 2, b + 2, c + 2, d + 2, 1, 1] = lowerPoint;
                        }
                    }
                }
            }

            //generate centers positions
            //1 - 0.00 * diagonal
            //2 - 0.25 * diagonal
            //3 - 0.50 * diagonal
            //4 - 0.75 * diagonal
            //5 - 1.00 * diagonal

            int[,] nodesPositions = new int[sizeInRegions, sizeInRegions];

            List<int> currentPossiblePositions = new List<int>(5);

            for (int x = 0; x < sizeInRegions; x++)
            {
                for (int y = 0; y < sizeInRegions; y++)
                {
                    if (x == 0 || y == 0)
                    {
                        nodesPositions[x, y] = random.Next(0, 5);
                    }
                    else
                    {
                        currentPossiblePositions.Clear();

                        for (int i = 0; i < 5; i++)
                        {
                            if (possibleCombinationsMatrix[i, nodesPositions[x - 1, y], nodesPositions[x - 1, y - 1], nodesPositions[x, y - 1]] != 0) currentPossiblePositions.Add(i);
                        }

                        nodesPositions[x, y] = currentPossiblePositions[random.Next(0, currentPossiblePositions.Count)];
                    }
                }
            }

            //create nodes and edges

            Region[,] regions = new Region[sizeInRegions, sizeInRegions];
            BorderPoint[,,,] borderPoints = new BorderPoint[sizeInRegions, sizeInRegions, 2, 2]; //1 - lefter/righter, 2 - upper/downer

            RegionsGraph newRegionsGraph = new RegionsGraph();
            BordersGraph newBordersGraph = new BordersGraph();

            //create regions
            for (int x = 0; x < sizeInRegions; x++)
            {
                for (int y = 0; y < sizeInRegions; y++)
                {
                    int coordsOffset = nodesPositions[x, y] - 2;

                    int newNodeX = x * regionSize + regionSize / 2 + (int)(0.5f * coordsOffset * allowedOffsetRadius);
                    int newNodeY = y * regionSize + regionSize / 2 + (int)(0.5f * coordsOffset * allowedOffsetRadius);

                    Region newRegion = new Region(newNodeX, newNodeY, map.regionSize);

                    regions[x, y] = newRegion;
                }
            }

            //create border points
            for (int x = 1; x < sizeInRegions; x++)
            {
                for (int y = 1; y < sizeInRegions; y++)
                {
                    int aPosition = nodesPositions[x, y];
                    int bPosition = nodesPositions[x - 1, y];
                    int cPosition = nodesPositions[x - 1, y - 1];
                    int dPosition = nodesPositions[x, y - 1];

                    Vector2Int lefterBorderNodeCoords = borderCoordsMatrix[aPosition, bPosition, cPosition, dPosition, 0, 0];
                    Vector2Int righterBorderNodeCoords = borderCoordsMatrix[aPosition, bPosition, cPosition, dPosition, 0, 1];
                    Vector2Int upperBorderNodeCoords = borderCoordsMatrix[aPosition, bPosition, cPosition, dPosition, 1, 0];

                    lefterBorderNodeCoords.x += x * regionSize + regionSize / 2;
                    lefterBorderNodeCoords.y += y * regionSize + regionSize / 2;
                    righterBorderNodeCoords.x += x * regionSize + regionSize / 2;
                    righterBorderNodeCoords.y += y * regionSize + regionSize / 2;
                    upperBorderNodeCoords.x += x * regionSize + regionSize / 2;
                    upperBorderNodeCoords.y += y * regionSize + regionSize / 2;

                    BorderPoint lefterBorderNode = new BorderPoint(lefterBorderNodeCoords.x, lefterBorderNodeCoords.y);
                    BorderPoint righterBorderNode = new BorderPoint(righterBorderNodeCoords.x, righterBorderNodeCoords.y);
                    BorderPoint upperBorderNode = lefterBorderNodeCoords == upperBorderNodeCoords ? lefterBorderNode : righterBorderNode;
                    BorderPoint downerBorderNode = lefterBorderNodeCoords == upperBorderNodeCoords ? righterBorderNode : lefterBorderNode;

                    borderPoints[x, y, 0, 0] = lefterBorderNode;
                    borderPoints[x, y, 0, 1] = righterBorderNode;
                    borderPoints[x, y, 1, 0] = upperBorderNode;
                    borderPoints[x, y, 1, 1] = downerBorderNode;
                }
            }

            //create roads and borders
            for (int x = 1; x < sizeInRegions; x++)
            {
                for (int y = 1; y < sizeInRegions; y++)
                {
                    int aPosition = nodesPositions[x, y];
                    int bPosition = nodesPositions[x - 1, y];
                    int cPosition = nodesPositions[x - 1, y - 1];
                    int dPosition = nodesPositions[x, y - 1];

                    bool isDiagonalRight = possibleCombinationsMatrix[aPosition, bPosition, cPosition, dPosition] == 1;

                    Region aNode = regions[x, y];
                    Region bNode = regions[x - 1, y];
                    Region cNode = regions[x - 1, y - 1];
                    Region dNode = regions[x, y - 1];

                    BorderPoint leftNode = borderPoints[x, y, 0, 0];
                    BorderPoint rightNode = borderPoints[x, y, 0, 1];
                    BorderPoint upNode = borderPoints[x, y, 1, 0];
                    BorderPoint downNode = borderPoints[x, y, 1, 1];

                    BorderPoint upNeighborBorderNode = y != sizeInRegions - 1 ? borderPoints[x, y + 1, 1, 1] : null;
                    BorderPoint rightNeighborBorderNode = x != sizeInRegions - 1 ? borderPoints[x + 1, y, 0, 0] : null;

                    if (y != sizeInRegions - 1) CreateEdge(bNode, aNode, upNode, upNeighborBorderNode, false);
                    if (x != sizeInRegions - 1) CreateEdge(dNode, aNode, rightNode, rightNeighborBorderNode, false);
                    if (isDiagonalRight && !((x == sizeInRegions - 1 && y == 1) || (x == 1 && y == sizeInRegions - 1))) CreateEdge(cNode, aNode, upNode, downNode, true);
                    if (!isDiagonalRight && !((x == 1 && y == 1) || (x == sizeInRegions - 1 && y == sizeInRegions - 1))) CreateEdge(bNode, dNode, leftNode, rightNode, true);

                    void CreateEdge(Region firstNode, Region secondNode, BorderPoint firstBorderNode, BorderPoint secondBorderNode, bool isDiagonal)
                    {
                        Road deloneEdge = new Road(firstNode, secondNode);
                        Border voronoiEdge = new Border(firstBorderNode, secondBorderNode);

                        firstNode.roads.Add(deloneEdge);
                        secondNode.roads.Add(deloneEdge);
                        firstBorderNode.borders.Add(voronoiEdge);
                        secondBorderNode.borders.Add(voronoiEdge);

                        newRegionsGraph.AddElement(deloneEdge);
                        newBordersGraph.AddElement(voronoiEdge);
                        deloneEdge.border = voronoiEdge;
                        voronoiEdge.road = deloneEdge;

                        voronoiEdge.isDiagonal = isDiagonal;
                    }
                }
            }

            for (int x = 1; x < sizeInRegions - 1; x++)
            {
                for (int y = 1; y < sizeInRegions - 1; y++)
                {
                    Region currentNode = regions[x, y];

                    currentNode.isPadding = false;

                    newRegionsGraph.AddElement(currentNode);
                }
            }

            for (int x = 1; x < sizeInRegions; x++)
            {
                for (int y = 1; y < sizeInRegions; y++)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        BorderPoint currentNode = borderPoints[x, y, 0, i];

                        if (currentNode.borders.Count > 0)
                        {
                            newBordersGraph.AddElement(currentNode);
                        }
                    }
                }
            }

            int idCounter = 0;
            foreach (Region region in newRegionsGraph.regions)
            {
                region.id = idCounter;
                idCounter++;
            }

            idCounter = 0;
            foreach (BorderPoint point in newBordersGraph.points)
            {
                point.id = idCounter;
                idCounter++;
            }

            idCounter = 0;
            foreach (Road road in newRegionsGraph.roads)
            {
                road.id = idCounter;
                road.border.id = idCounter;
                idCounter++;
            }

            map.regionsGraph = newRegionsGraph;
            map.bordersGraph = newBordersGraph;
        }

        void DivideCellsByRegions()
        {
            bool[,] isCellAtBorder = GraphUtils.GetAllBordersCells(map);
            bool[,] isCellChecked = new bool[map.sizeInCells, map.sizeInCells];

            Queue<Vector2Int> frontier = new Queue<Vector2Int>(map.sizeInCells * 4);

            foreach (Region currentRegion in map.regionsGraph.regions)
            {
                if (currentRegion.isPadding) continue;

                frontier.Enqueue(currentRegion.center);
                isCellChecked[currentRegion.center.x, currentRegion.center.y] = true;

                while (frontier.Count > 0)
                {
                    Vector2Int currentCell = frontier.Dequeue();

                    int x = currentCell.x;
                    int y = currentCell.y;

                    map.cellToRegion[currentCell.x, currentCell.y] = currentRegion.id;
                    currentRegion.cellsCoords.Add(currentCell);

                    if (x > 0 && !isCellAtBorder[x - 1, y] && !isCellChecked[x - 1, y])
                    {
                        frontier.Enqueue(new Vector2Int(x - 1, y));
                        isCellChecked[x - 1, y] = true;
                    }
                    if (x < map.sizeInCells - 1 && !isCellAtBorder[x + 1, y] && !isCellChecked[x + 1, y])
                    {
                        frontier.Enqueue(new Vector2Int(x + 1, y));
                        isCellChecked[x + 1, y] = true;
                    }
                    if (y > 0 && !isCellAtBorder[x, y - 1] && !isCellChecked[x, y - 1])
                    {
                        frontier.Enqueue(new Vector2Int(x, y - 1));
                        isCellChecked[x, y - 1] = true;
                    }
                    if (y < map.sizeInCells - 1 && !isCellAtBorder[x, y + 1] && !isCellChecked[x, y + 1])
                    {
                        frontier.Enqueue(new Vector2Int(x, y + 1));
                        isCellChecked[x, y + 1] = true;
                    }
                }
            }

            List<Vector2Int> line = new List<Vector2Int>(map.regionSize);
            foreach (Border border in map.bordersGraph.borders)
            {
                if (border.road.firstRegion.isPadding || border.road.secondRegion.isPadding)
                {
                    line.Clear();
                    GraphUtils.GetCellsOnLineExeptEnds(line, map, border.firstPoint.center, border.secondPoint.center);
                    Region notPadding = border.road.firstRegion.isPadding ? border.road.secondRegion : border.road.firstRegion;

                    foreach (Vector2Int coords in line) map.cellToRegion[coords.x, coords.y] = notPadding.id;
                }
                else
                {
                    line.Clear();
                    GraphUtils.GetCellsOnLineExeptEnds(line, map, border.firstPoint.center, border.secondPoint.center);

                    foreach (Vector2Int coords in line)
                    {
                        Region region = random.NextDouble() > 0.5 ? border.road.secondRegion : border.road.firstRegion;
                        map.cellToRegion[coords.x, coords.y] = region.id;
                        region.cellsCoords.Add(coords);
                    }
                }
            }

            foreach (BorderPoint point in map.bordersGraph.points)
            {
                Region region = null;

                foreach (Border border in point.borders)
                {
                    if (!border.road.firstRegion.isPadding)
                    {
                        region = border.road.firstRegion;
                        break;
                    }
                    if (!border.road.secondRegion.isPadding)
                    {
                        region = border.road.secondRegion;
                        break;
                    }
                }

                map.cellToRegion[point.center.x, point.center.y] = region.id;
                region.cellsCoords.Add(point.center);
            }

            // for (int x = 0; x < map.sizeInCells; x++)
            // {
            //     for (int y = 0; y < map.sizeInCells; y++)
            //     {
            //         map.regionsGraph.regions[map.cellToRegion[x, y]].cellsCoords.Add(new Vector2Int(x, y));
            //     }
            // }
        }

        void GenerateSea()
        {
            RegionsGraph initialGraph = map.regionsGraph;
            int regionsCount = map.sizeInRegions * map.sizeInRegions;

            HashSet<Region> checkedRegions = new HashSet<Region>();
            List<Region> frontier = new List<Region>();

            foreach (Region currentRegion in GraphUtils.GetGraphBorder(initialGraph.regions, initialGraph.regionsHash))
            {
                frontier.Add(currentRegion);
                checkedRegions.Add(currentRegion);
            }

            while (checkedRegions.Count < regionsCount * seaPercent && frontier.Count > 0)
            {
                int randomIndex = random.Next(0, frontier.Count);

                Region currentRegion = frontier[randomIndex];

                frontier.RemoveAt(randomIndex);
                frontier.AddRange(GraphUtils.GetRandomWalkPath(currentRegion, checkedRegions, initialGraph.regionsHash, random));
                if (!GraphUtils.IsRegionIsolated(currentRegion, checkedRegions)) frontier.Add(currentRegion);
            }

            foreach (Region currentRegion in checkedRegions)
            {
                currentRegion.isSea = true;
            }
        }

        void GenerateLandmasses()
        {
            var (clusters, clastersBorders) = GraphUtils.GetIsolatedClusters(map.regionsGraph, true, r => r.isSea);
            map.landmasses = clusters;
            map.landmassesBorders = clastersBorders;
        }

        void GenerateTectonicForces()
        {
            int iterationsCount = 3;
            int platesCount = Math.Max(4, (map.sizeInRegions / 25) * (map.sizeInRegions / 25));
            int borderWidth = 3;

            List<float> tectonicForces = new List<float>(map.bordersGraph.points.Count);
            for (int i = 0; i < map.bordersGraph.points.Count; i++)
            {
                tectonicForces.Add((float)random.NextDouble() / 2);
            }

            for (int q = 0; q < iterationsCount; q++)
            {
                for (int i = 0; i < tectonicForces.Count; i++)
                {
                    tectonicForces[i] = tectonicForces[i] / 2;
                }

                List<BorderPoint> seeds = GraphUtils.GetNEquidistantBorderPoints(platesCount, map.bordersGraph, random);
                List<List<BorderPoint>> plates = GraphUtils.GetRandomDividedBorderPointsClusters(map.bordersGraph, seeds, random);
                HashSet<BorderPoint>[,,] platesBorders = new HashSet<BorderPoint>[plates.Count, plates.Count, borderWidth];
                int[] pointToPlate = new int[map.bordersGraph.points.Count];

                int plateIdCounter = 0;
                foreach (List<BorderPoint> plate in plates)
                {
                    foreach (BorderPoint point in plate)
                    {
                        pointToPlate[point.id] = plateIdCounter;
                    }

                    plateIdCounter++;
                }

                foreach (Border border in map.bordersGraph.borders)
                {
                    if (pointToPlate[border.firstPoint.id] != pointToPlate[border.secondPoint.id])
                    {
                        int firstId = pointToPlate[border.firstPoint.id];
                        int secondId = pointToPlate[border.secondPoint.id];

                        if (platesBorders[firstId, secondId, 0] == null)
                        {
                            HashSet<BorderPoint> firstPlateBorder = new HashSet<BorderPoint>();
                            HashSet<BorderPoint> secondPlateBorder = new HashSet<BorderPoint>();

                            firstPlateBorder.Add(border.firstPoint);
                            secondPlateBorder.Add(border.secondPoint);

                            platesBorders[firstId, secondId, 0] = firstPlateBorder;
                            platesBorders[secondId, firstId, 0] = secondPlateBorder;
                        }
                        else
                        {
                            platesBorders[firstId, secondId, 0].Add(border.firstPoint);
                            platesBorders[secondId, firstId, 0].Add(border.secondPoint);
                        }
                    }
                }

                for (int i = 1; i < borderWidth; i++)
                {
                    for (int x = 0; x < plates.Count; x++)
                    {
                        HashSet<BorderPoint> currentPlate = new HashSet<BorderPoint>(plates[x]);

                        for (int y = 0; y < plates.Count; y++)
                        {
                            HashSet<BorderPoint> prevWave = platesBorders[x, y, i - 1];
                            HashSet<BorderPoint> beyondPrevWave = i == 1 ? null : platesBorders[x, y, i - 2];

                            if (prevWave == null) continue;

                            HashSet<BorderPoint> currentWave = new HashSet<BorderPoint>();

                            platesBorders[x, y, i] = currentWave;

                            foreach (BorderPoint point in prevWave)
                            {
                                foreach (Border border in point.borders)
                                {
                                    BorderPoint neighbor = border.GetNeighbor(point);

                                    if (currentPlate.Contains(neighbor) && !prevWave.Contains(neighbor) && (i == 1 ? true : !beyondPrevWave.Contains(neighbor)))
                                    {
                                        currentWave.Add(neighbor);
                                    }
                                }
                            }
                        }
                    }
                }


                for (int x = 0; x < plates.Count; x++)
                {
                    for (int y = 0; y < plates.Count; y++)
                    {
                        if (platesBorders[x, y, 0] == null) continue;

                        float plateBorderStress = (float)random.NextDouble();

                        for (int i = 0; i < borderWidth; i++)
                        {
                            foreach (BorderPoint point in platesBorders[x, y, i])
                            {
                                tectonicForces[point.id] = math.min(1f, tectonicForces[point.id] + (plateBorderStress / (i + 1)));
                            }
                        }
                    }
                }
            }

            List<float> regionsForces = new List<float>(map.regionsGraph.regions.Count);

            HashSet<BorderPoint> regionPoints = new HashSet<BorderPoint>();
            foreach (Region region in map.regionsGraph.regions)
            {
                regionPoints.Clear();

                foreach (Road road in region.roads)
                {
                    regionPoints.Add(road.border.firstPoint);
                    regionPoints.Add(road.border.secondPoint);
                }

                float average = 0f;
                int count = 0;

                foreach (BorderPoint point in regionPoints)
                {
                    count++;
                    average += tectonicForces[point.id];
                }

                average = average / count;

                regionsForces.Add(average);
            }

            map.tectonicForces = tectonicForces;
            map.regionsTectonicForces = regionsForces;
        }

        void GenerateBasins()
        {
            bool[] isPointCoast = new bool[map.bordersGraph.points.Count];

            foreach (BorderPoint point in map.bordersGraph.points)
            {
                foreach (Border border in point.borders)
                {
                    if (border.road.firstRegion.isSea || border.road.secondRegion.isSea)
                    {
                        isPointCoast[point.id] = true;
                        break;
                    }
                }
            }

            Dictionary<BorderPoint, List<Border>> allBasins = new Dictionary<BorderPoint, List<Border>>();
            HashSet<Border> checkedBorders = new HashSet<Border>();

            int landmassIndex = 0;
            foreach (BordersGraph bordersGraph in map.landmassesBorders)
            {
                Dictionary<BorderPoint, List<Border>> localBasins = new Dictionary<BorderPoint, List<Border>>();
                Dictionary<BorderPoint, Queue<Border>> localBasinsFrontiers = new Dictionary<BorderPoint, Queue<Border>>();
                Dictionary<Border, BorderPoint> belonging = new Dictionary<Border, BorderPoint>();

                checkedBorders.AddRange(GraphUtils.GetAllVisitedBordersFromSeaPathes(map, map.landmasses[landmassIndex]));

                foreach (Border border in bordersGraph.borders)
                {
                    if ((isPointCoast[border.firstPoint.id] && isPointCoast[border.secondPoint.id]) || (!isPointCoast[border.firstPoint.id] && !isPointCoast[border.secondPoint.id]))
                    {
                        if (isPointCoast[border.firstPoint.id] && isPointCoast[border.secondPoint.id]) checkedBorders.Add(border);
                        continue;
                    }

                    BorderPoint mouth = isPointCoast[border.firstPoint.id] ? border.firstPoint : border.secondPoint;

                    checkedBorders.Add(border);

                    List<Border> basin = new List<Border>();
                    Queue<Border> frontier = new Queue<Border>();
                    frontier.Enqueue(border);

                    localBasins.Add(mouth, basin);
                    localBasinsFrontiers.Add(mouth, frontier);
                }

                List<BorderPoint> avaibleKeys = new List<BorderPoint>(localBasins.Count);
                foreach (BorderPoint point in localBasins.Keys) avaibleKeys.Add(point);

                List<Border> currentNeighbors = new List<Border>(4);
                while (avaibleKeys.Count > 0)
                {
                    int randomIndexIndex = random.Next(0, avaibleKeys.Count);
                    BorderPoint key = avaibleKeys[randomIndexIndex];

                    if (localBasinsFrontiers[key].Count == 0)
                    {
                        avaibleKeys.RemoveAt(randomIndexIndex);
                        continue;
                    }

                    Border current = localBasinsFrontiers[key].Dequeue();
                    currentNeighbors.Clear();
                    GraphUtils.GetBorderNeighbors(currentNeighbors, current);

                    bool hasOtherBasinNeighbor = false;
                    foreach (Border neighbor in currentNeighbors)
                    {
                        if (!checkedBorders.Contains(neighbor))
                        {
                            localBasinsFrontiers[key].Enqueue(neighbor);

                            checkedBorders.Add(neighbor);
                        }

                        BorderPoint neighborKey;
                        if (belonging.TryGetValue(neighbor, out neighborKey))
                        {
                            if (neighborKey != key) hasOtherBasinNeighbor = true;
                        }
                    }

                    if (!hasOtherBasinNeighbor)
                    {
                        localBasins[key].Add(current);
                        belonging.Add(current, key);
                    }
                }

                allBasins.AddRange(localBasins);
                landmassIndex++;
            }

            map.basins = allBasins;
        }

        void GenerateRegionsHeight()
        {
            HashSet<Border> bannedBorders = new HashSet<Border>();

            foreach (List<Border> basin in map.basins.Values)
            {
                bannedBorders.AddRange(basin);
            }

            foreach (RegionsGraph landmass in map.landmasses)
            {
                HashSet<Region> uncheckedRegions = new HashSet<Region>();
                Queue<Region> startRegions = new Queue<Region>();
                Queue<Region> frontier = new Queue<Region>();

                uncheckedRegions.AddRange(landmass.regions);

                foreach (Region candidate in uncheckedRegions.ToArray())
                {
                    foreach (Road road in candidate.roads)
                    {
                        Region neighbor = road.GetNeighbor(candidate);

                        if (neighbor.isSea)
                        {
                            startRegions.Enqueue(candidate);
                            uncheckedRegions.Remove(candidate);

                            candidate.height = Utils.GetOppositeLegLength(map.regionsTectonicForces[candidate.id], Utils.GetLineLength(candidate.center, neighbor.center));
                            break;
                        }
                    }
                }

                while (startRegions.Count > 0)
                {
                    // Region start = startRegions.Dequeue();
                    // if (!uncheckedRegions.Contains(start)) continue;
                    // uncheckedRegions.Remove(start);
                    // frontier.Enqueue(start);

                    // while (frontier.Count > 0)
                    // {
                    Region current = startRegions.Dequeue();
                    uncheckedRegions.Remove(current);

                    foreach (Road road in current.roads)
                    {
                        Region neighbor = road.GetNeighbor(current);

                        if (uncheckedRegions.Contains(neighbor) && !bannedBorders.Contains(road.border))
                        {
                            startRegions.Enqueue(neighbor);
                            uncheckedRegions.Remove(neighbor);
                            neighbor.height = Utils.GetOppositeLegLength(map.regionsTectonicForces[neighbor.id], Utils.GetLineLength(neighbor.center, current.center)) + current.height;
                        }
                    }
                    // }
                }
            }

            float maxHeight = 0f;
            float minHeight = 10f;
            foreach (Region region in map.regionsGraph.regions)
            {
                if (region.height > maxHeight) maxHeight = region.height;
                if (region.height < minHeight && !region.isSea) minHeight = region.height;
            }
            Debug.Log($"Max region Height: {maxHeight} Min region Height: {minHeight}");
        }

        void GenerateBorderPointsHeight()
        {
            HashSet<Border> uncheckedBorders = new HashSet<Border>();
            HashSet<BorderPoint> checkedPoints = new HashSet<BorderPoint>();
            foreach (var (mouth, borders) in map.basins)
            {
                if (borders.Count == 0) continue;

                uncheckedBorders.Clear();
                checkedPoints.Clear();
                uncheckedBorders.AddRange(borders);

                Dictionary<int, List<BorderPoint>> pointsHierarchy = new Dictionary<int, List<BorderPoint>>();
                Queue<BorderPoint> queue = new Queue<BorderPoint>();
                checkedPoints.Add(mouth);
                List<BorderPoint> firstWave = new List<BorderPoint>();
                firstWave.Add(mouth);
                pointsHierarchy.Add(0, firstWave);

                int hierarchyCount = 0;
                while (uncheckedBorders.Count > 0)
                {
                    foreach (var point in pointsHierarchy[hierarchyCount]) queue.Enqueue(point);

                    List<BorderPoint> nextWave = new List<BorderPoint>();
                    while (queue.Count > 0)
                    {
                        BorderPoint current = queue.Dequeue();

                        foreach (Border border in current.borders)
                        {
                            BorderPoint neighbor = border.GetNeighbor(current);

                            if (uncheckedBorders.Contains(border))
                            {
                                nextWave.Add(neighbor);
                                checkedPoints.Add(neighbor);
                                uncheckedBorders.Remove(border);
                            }
                        }
                    }

                    pointsHierarchy.Add(hierarchyCount + 1, nextWave);
                    hierarchyCount++;
                }

                uncheckedBorders.AddRange(borders);
                while (hierarchyCount >= 0)
                {
                    foreach (BorderPoint point in pointsHierarchy[hierarchyCount])
                    {
                        checkedPoints.Remove(point);

                        float minHeight = float.MaxValue;
                        Vector2Int prevPoint = new Vector2Int();
                        foreach (Border border in point.borders)
                        {
                            BorderPoint neighbor = border.GetNeighbor(point);

                            if (!checkedPoints.Contains(neighbor) && uncheckedBorders.Contains(border))
                            {
                                if (neighbor.height < minHeight)
                                {
                                    minHeight = neighbor.height;
                                    prevPoint = neighbor.center;
                                }
                            }
                            else if (uncheckedBorders.Contains(border))
                            {
                                if (minHeight == float.MaxValue)
                                {
                                    Region first = border.road.firstRegion;
                                    Region second = border.road.secondRegion;
                                    minHeight = first.height < second.height ? first.height : second.height;
                                    prevPoint = first.height < second.height ? first.center : second.center;
                                }
                            }
                        }

                        point.height = math.max(0, minHeight + Utils.GetOppositeLegLength(0.000190986f, Utils.GetLineLength(point.center, prevPoint)));
                    }

                    hierarchyCount--;
                }
            }

            float maxHeight = 0f;
            foreach (BorderPoint point in map.bordersGraph.points)
            {
                if (point.height > maxHeight) maxHeight = point.height;
            }
            Debug.Log($"Max point Height: {maxHeight}");
        }

        void CalculateCellsHeights()
        {
            Dictionary<int, (Vector2, Vector2, Vector2)> borderBezierCurves = new Dictionary<int, (Vector2, Vector2, Vector2)>();
            Dictionary<(int, int), (Vector2, Vector2, Vector2)> regionBezierCurves = new Dictionary<(int, int), (Vector2, Vector2, Vector2)>();
            bool[] isBorderRiver = new bool[map.bordersGraph.borders.Count];

            foreach (var list in map.basins.Values) foreach (Border border in list) isBorderRiver[border.id] = true;

            foreach (Border border in map.bordersGraph.borders)
            {
                if (border.road.firstRegion.isSea && border.road.secondRegion.isSea) continue;

                BorderPoint a = border.firstPoint;
                BorderPoint b = border.secondPoint;
                Region c = border.road.firstRegion;
                Region d = border.road.secondRegion;

                SetRegionBorderPointCurve(c, a);
                SetRegionBorderPointCurve(c, b);
                SetRegionBorderPointCurve(d, a);
                SetRegionBorderPointCurve(d, b);
                if (isBorderRiver[border.id]) SetRiverBorderCurve(border);
                else SetPlainBorderCurve(border, c, d);

                void SetRegionBorderPointCurve(Region region, BorderPoint point)
                {
                    if (regionBezierCurves.ContainsKey((region.id, point.id))) return;

                    bool isXDominant = Math.Abs(point.center.x - region.center.x) > Math.Abs(point.center.y - region.center.y);

                    Vector2 a = new Vector2(isXDominant ? point.center.x : point.center.y, point.height);
                    Vector2 b = new Vector2(isXDominant ? region.center.x : region.center.y, region.height);
                    Vector2 c = new Vector2(a.x, b.y);

                    regionBezierCurves.Add((region.id, point.id), (a, b, c));
                }

                void SetRiverBorderCurve(Border border)
                {
                    if (borderBezierCurves.ContainsKey(border.id)) return;

                    BorderPoint first = border.firstPoint;
                    BorderPoint second = border.secondPoint;

                    bool isXDominant = Math.Abs(first.center.x - second.center.x) > Math.Abs(first.center.y - second.center.y);

                    Vector2 a = new Vector2(isXDominant ? first.center.x : first.center.y, first.height);
                    Vector2 b = new Vector2(isXDominant ? second.center.x : second.center.y, second.height);
                    Vector2 c = new Vector2((a.x + b.x) / 2, a.y > b.y ? a.y : b.y);

                    borderBezierCurves.Add(border.id, (a, b, c));
                }

                void SetPlainBorderCurve(Border border, Region firstRegion, Region secondRegion)
                {
                    if (borderBezierCurves.ContainsKey(border.id)) return;

                    BorderPoint first = border.firstPoint;
                    BorderPoint second = border.secondPoint;

                    bool isXDominant = Math.Abs(first.center.x - second.center.x) > Math.Abs(first.center.y - second.center.y);

                    Vector2 a = new Vector2(isXDominant ? first.center.x : first.center.y, first.height);
                    Vector2 b = new Vector2(isXDominant ? second.center.x : second.center.y, second.height);
                    Vector2 c = new Vector2((a.x + b.x) / 2f, (2f * (firstRegion.height + secondRegion.height) / 2f) - ((first.height + second.height) / 2f));

                    borderBezierCurves.Add(border.id, (a, b, c));
                }
            }

            HashSet<BorderPoint> pointsHash = new HashSet<BorderPoint>(8);
            foreach (Region region in map.regionsGraph.regions)
            {
                if (region.isSea || region.isPadding) continue;

                pointsHash.Clear();

                foreach (Road road in region.roads)
                {
                    pointsHash.Add(road.border.firstPoint);
                    pointsHash.Add(road.border.secondPoint);
                }

                float[] pointsAngles = new float[pointsHash.Count];
                BorderPoint[] points = new BorderPoint[pointsHash.Count];

                int indexCounter = 0;
                foreach (BorderPoint point in pointsHash)
                {
                    points[indexCounter] = point;
                    indexCounter++;
                }

                for (int i = 0; i < pointsHash.Count; i++)
                {
                    int dx = points[i].center.x - region.center.x;
                    int dy = points[i].center.y - region.center.y;
                    pointsAngles[i] = MathF.Atan2(dy, dx);
                }

                Array.Sort(pointsAngles, points);

                foreach (var coord in region.cellsCoords)
                {
                    int dx = coord.x - region.center.x;
                    int dy = coord.y - region.center.y;
                    float cellAngle = MathF.Atan2(dy, dx);

                    BorderPoint firstPoint = null;
                    BorderPoint secondPoint = null;
                    for (int i = 0; i < pointsHash.Count; i++)
                    {
                        if (i == pointsHash.Count - 1)
                        {
                            firstPoint = points[0];
                            secondPoint = points[i];
                            break;
                        }
                        else if (pointsAngles[i] <= cellAngle && cellAngle < pointsAngles[i + 1])
                        {
                            firstPoint = points[i];
                            secondPoint = points[i + 1];
                            break;
                        }
                    }

                    Border border = null;
                    foreach (Border current in firstPoint.borders)
                    {
                        if ((current.firstPoint == firstPoint && current.secondPoint == secondPoint) || (current.firstPoint == secondPoint && current.secondPoint == firstPoint))
                        {
                            border = current;
                            break;
                        }
                    }

                    Vector2 A = firstPoint.center;
                    Vector2 B = secondPoint.center;
                    Vector2 C = region.center;
                    Vector2 P = coord;

                    float triangleArea = (B.y - C.y) * (A.x - C.x) + (C.x - B.x) * (A.y - C.y);

                    if (triangleArea == 0) continue;

                    float u = ((B.y - C.y) * (P.x - C.x) + (C.x - B.x) * (P.y - C.y)) / triangleArea;
                    float v = ((C.y - A.y) * (P.x - C.x) + (A.x - C.x) * (P.y - C.y)) / triangleArea;
                    float w = 1 - u - v;

                    float sharpness = 2.0f;
                    float edgeWeight = u + v; // Загальна вага меж (по суті це 1 - w)

                    if (edgeWeight > 0f)
                    {
                        // Варіант 1: Гостра вершина (схил різко падає від центру, але м'який біля меж)
                        // w = MathF.Pow(w, sharpness);

                        // Варіант 2: Широке плато (висота тримається довго, а біля меж різко обривається)
                        w = 1f - MathF.Pow(1f - w, sharpness);

                        // Масштабуємо u та v так, щоб вони зберегли свої чисті пропорції,
                        // але в сумі з новим 'w' знову давали ідеальну одиницю
                        float scale = (1f - w) / edgeWeight;
                        u *= scale;
                        v *= scale;
                    }

                    float hA = firstPoint.height;
                    float hB = secondPoint.height;
                    float hC = region.height;

                    float hAB = borderBezierCurves[border.id].Item3.y;
                    float hBC = regionBezierCurves[(region.id, secondPoint.id)].Item3.y;
                    float hCA = regionBezierCurves[(region.id, firstPoint.id)].Item3.y;

                    float hP = (u * u * hA) + (v * v * hB) + (w * w * hC) + (2 * u * v * hAB) + (2 * v * w * hBC) + (2 * w * u * hCA);

                    map.cells[coord.x, coord.y].height = MathF.Round(hP / 0.0524f) * 0.0524f;

                    //     float aSideT;
                    //     float bSideT;
                    //     float cSideT;
                    //     float aBisSqrLength;
                    //     float bBisSqrLength;
                    //     float cBisSqrLength;

                    //     (aSideT, aBisSqrLength) = GetBisectorTAndSqrLength(firstPoint.center, secondPoint.center, coord);
                    //     (bSideT, bBisSqrLength) = GetBisectorTAndSqrLength(region.center, secondPoint.center, coord);
                    //     (cSideT, cBisSqrLength) = GetBisectorTAndSqrLength(firstPoint.center, region.center, coord);

                    //     float aSideH = GetYAtPointAtXAtBezierCurve(aSideT, borderBezierCurves[border.id]);
                    //     float bSideH = GetYAtPointAtXAtBezierCurve(bSideT, regionBezierCurves[(region.id, secondPoint.id)]);
                    //     float cSideH = GetYAtPointAtXAtBezierCurve(cSideT, regionBezierCurves[(region.id, firstPoint.id)]);

                    //     map.cells[coord.x, coord.y].height = InterpolateHeightBySquaredDistances(aSideH, bSideH, cSideH, aBisSqrLength, bBisSqrLength, cBisSqrLength);

                    //     (float, float) GetBisectorTAndSqrLength(Vector2 a, Vector2 b, Vector2 p)
                    //     {
                    //         float lenA = Vector2.Distance(p, a);
                    //         float lenB = Vector2.Distance(p, b);

                    //         float t = lenA / (lenA + lenB);

                    //         Vector2 d = a + t * (b - a);

                    //         float sqrLength = (d - p).sqrMagnitude;

                    //         return (t, sqrLength);
                    //     }

                    //     float GetYAtPointAtXAtBezierCurve(float xT, (Vector2, Vector2, Vector2) bezeirPoints)
                    //     {
                    //         var (a, b, c) = bezeirPoints;
                    //         float pointX = a.x + xT * (b.x - a.x);

                    //         float A = a.x - (2 * c.x) + b.x;
                    //         if (A == 0) return (1 - xT) * (1 - xT) * a.y + 2 * (1 - xT) * xT * c.y + xT * xT * b.y;
                    //         float B = 2 * (c.x - a.x);
                    //         float C = a.x - pointX;

                    //         float D = B * B - (4 * A * C);
                    //         if (D < 0) D = 0;

                    //         float t = ((-1 * B) + MathF.Sqrt(D)) / (2 * A);

                    //         if (t > 1 || t < 0) t = ((-1 * B) - MathF.Sqrt(D)) / (2 * A);

                    //         return (1 - t) * (1 - t) * a.y + 2 * (1 - t) * t * c.y + t * t * b.y;
                    //     }

                    //     float InterpolateHeightBySquaredDistances(float hA, float hB, float hC, float sqrDistA, float sqrDistB, float sqrDistC)
                    //     {
                    //         if (sqrDistA < 0.01f) sqrDistA = 0.01f;
                    //         if (sqrDistB < 0.01f) sqrDistB = 0.01f;
                    //         if (sqrDistC < 0.01f) sqrDistC = 0.01f;

                    //         float wA = 1f / sqrDistA;
                    //         float wB = 1f / sqrDistB;
                    //         float wC = 1f / sqrDistC;

                    //         float totalWeight = wA + wB + wC;

                    //         return (hA * wA + hB * wB + hC * wC) / totalWeight;
                    //     }
                }
            }
        }
    }
}