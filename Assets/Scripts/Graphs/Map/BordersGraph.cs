using System.Collections.Generic;

public class BordersGraph
{
    public List<BorderPoint> points;
    public HashSet<BorderPoint> pointsHash;
    public List<Border> borders;
    public HashSet<Border> bordersHash;

    public BordersGraph()
    {
        points = new List<BorderPoint>();
        pointsHash = new HashSet<BorderPoint>();
        borders = new List<Border>();
        bordersHash = new HashSet<Border>();
    }

    public BordersGraph(HashSet<Border> bordersHash)
    {
        this.bordersHash = bordersHash;
        borders = new List<Border>(bordersHash);
        pointsHash = new HashSet<BorderPoint>();

        foreach (Border border in borders)
        {
            pointsHash.Add(border.firstPoint);
            pointsHash.Add(border.secondPoint);
        }

        points = new List<BorderPoint>(pointsHash);
    }

    public void AddElement(BorderPoint point)
    {
        points.Add(point);
        pointsHash.Add(point);
    }

    public void AddElement(Border border)
    {
        borders.Add(border);
        bordersHash.Add(border);
    }
}