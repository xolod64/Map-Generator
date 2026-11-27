public class Border
{
    public int id;

    public bool isDiagonal;

    public BorderPoint firstPoint;
    public BorderPoint secondPoint;

    public Road road;

    public Border(BorderPoint firstPoint, BorderPoint secondPoint)
    {
        this.firstPoint = firstPoint;
        this.secondPoint = secondPoint;
    }

    public BorderPoint GetNeighbor(BorderPoint point)
    {
        return point == firstPoint ? secondPoint : firstPoint;
    }
}