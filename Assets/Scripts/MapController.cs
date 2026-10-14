using UnityEngine;

[RequireComponent(typeof(MapVizualiser))]
public class MapController : MonoBehaviour
{
    public int seed = 152;
    public int width = 100;
    public int height = 100;
    [Range(0, 1)] public float mountainess = 1f;

    private MapVizualiser mapVizualiser;
    private Map map;

    private void Awake()
    {
        mapVizualiser = GetComponent<MapVizualiser>();
    }

    [ContextMenu("Create Map")]
    public void CreateMap()
    {
        map = MapGenerator.GenerateMap(seed, width, height, mountainess);
        mapVizualiser.VisualizeMap(map);
    }
}