using System.Collections.Generic;

public class RegionsGraph
{
    public List<Region> regions;
    public HashSet<Region> regionsHash;
    public List<Road> roads;
    public HashSet<Road> roadsHash;

    public RegionsGraph()
    {
        regions = new List<Region>();
        regionsHash = new HashSet<Region>();
        roads = new List<Road>();
        roadsHash = new HashSet<Road>();
    }

    public RegionsGraph(List<Region> regions)
    {
        this.regions = regions;
        regionsHash = new HashSet<Region>(regions);

        roadsHash = new HashSet<Road>();

        foreach (Region region in regions)
        {
            foreach (Road road in region.roads)
            {
                roadsHash.Add(road);
            }
        }

        roads = new List<Road>(roadsHash);
    }

    public void AddElement(Region region)
    {
        regions.Add(region);
        regionsHash.Add(region);
    }

    public void AddElement(Road road)
    {
        roads.Add(road);
        roadsHash.Add(road);
    }
}