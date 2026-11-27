public class Road
{
    public int id;
    
    public Region firstRegion;
    public Region secondRegion;

    public Border border;

    public Road(Region firstRegion, Region secondRegion)
    {
        this.firstRegion = firstRegion;
        this.secondRegion = secondRegion;
    }

    public Region GetNeighbor(Region region)
    {
        return region == firstRegion ? secondRegion : firstRegion;
    }
}