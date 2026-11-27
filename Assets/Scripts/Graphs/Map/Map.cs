using System.Collections.Generic;

public class Map
{
    public int sizeInRegions;
    public int regionSize;
    public int sizeInCells;

    public Cell[,] cells;
    public RegionsGraph regionsGraph;
    public BordersGraph bordersGraph;

    public List<RegionsGraph> landmasses;
    public List<BordersGraph> landmassesBorders;

    public List<float> tectonicForces;
    public List<float> regionsTectonicForces;

    public Dictionary<BorderPoint, List<Border>> basins;

    public int[,] cellToRegion;

    public Map(int sizeInRegions, int regionSize)
    {
        this.sizeInRegions = sizeInRegions;
        this.regionSize = regionSize;

        sizeInCells = regionSize * (sizeInRegions + 2);
    }

    // public int To1DIndex(int x, int y)
    // {
    //     return y * sizeInCells + x;
    // }

    // public (int x, int y) To2DCoords(int index)
    // {
    //     return (index % sizeInCells, index / sizeInCells);
    // }

    // public (int left, int right, int up, int down) GetNeighborIndices(int index)
    // {
    //     int x = index % sizeInCells;

    //     return (
    //         left: (x > 0) ? index - 1 : -1,
    //         right: (x < sizeInCells - 1) ? index + 1 : -1,
    //         up: (index >= sizeInCells) ? index - sizeInCells : -1,
    //         down: (index + sizeInCells < totalCells) ? index + sizeInCells : -1
    //     );
    // }
}