public class Map
{
    public int width;
    public int height;

    public float[,] heightmap;
    public float seaLevel;

    public Map(int width, int height)
    {
        this.width = width;
        this.height = height;

        heightmap = new float[width, height];
        seaLevel = 0;
    }
}
