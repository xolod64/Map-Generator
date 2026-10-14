using UnityEngine;

[RequireComponent(typeof(MapVizualiser))]
public class MapController : MonoBehaviour
{
    public int seed = 152;
    public int width = 100;
    public int height = 100;
    [Range(0, 1)] public float mountainess = 1f;

    public int octaves = 5;
    [Range(0, 1)] public float persistence = 0.5f;
    public float roughness = 2f;
    [Range(0, 0.2f)] public float scale = 0.05f;

    private MapVizualiser mapVizualiser;
    private Map map;

    private void Awake()
    {
        mapVizualiser = GetComponent<MapVizualiser>();
    }

    [ContextMenu("Create Map")]
    public void CreateMap()
    {
        map = MapGenerator.GenerateMap(seed, width, height, octaves, persistence, roughness, scale);

        mapVizualiser.VisualizeMap(map);
    }
}