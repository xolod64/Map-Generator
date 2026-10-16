using UnityEngine;

[RequireComponent(typeof(MapVizualiser))]
public class MapController : MonoBehaviour
{
    //map settings
    public int seed = 152;
    public int width = 1000;
    public int height = 1000;

    //wavefront algorithm settings
    [Range(0, 1)] public float mountainess = 1f;

    //perlin noise settings
    public int octaves = 5;
    [Range(0, 1)] public float persistence = 0.5f;
    public float roughness = 6f;
    [Range(0, 0.2f)] public float scale = 0.015f;

    //quantizing settings
    public int quantizingPrecision = 2;

    private MapVizualiser mapVizualiser;
    private Map map;

    private void Awake()
    {
        mapVizualiser = GetComponent<MapVizualiser>();
    }

    [ContextMenu("Create Map")]
    public void CreateMap()
    {
        map = new Map(width, height);
        System.Random random = new System.Random(seed);

        float[,] noise = MapGenerator.PerlinNoiseHeightmap(width, height, octaves, persistence, roughness, scale, random);
        int[,] quantizedNoise = Utils.QuantizeFloatArray(noise, quantizingPrecision);
        map.heightmap = MapGenerator.WaterfrontHeightmap(width, height, mountainess, quantizedNoise, random);

        mapVizualiser.VisualizeMap(map);
    }
}