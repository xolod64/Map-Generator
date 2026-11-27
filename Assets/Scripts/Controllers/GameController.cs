using UnityEngine;

public class GameController : MonoBehaviour
{
    public MapVisualizer mapVisualizer;
    public World world;

    public int seed = 1488;
    public int sizeInRegions = 50;
    public int regionSize = 32;
    [Range(0f, 1f)] public float seaPercent = 0.5f;

    public void StartNewGame()
    {
        int sizeInCells = sizeInRegions * regionSize;

        world = new World();

        WorldGenerator.GenerateWorld(world, seed, sizeInRegions, regionSize, seaPercent);

        mapVisualizer.InitTexture((sizeInRegions + 2) * regionSize, (sizeInRegions + 2) * regionSize, world.map);
        mapVisualizer.DrawSeaLandGraph();
    }

    public void StartNewGameWithRandomSeed()
    {
        System.Random random = new System.Random();
        seed = random.Next(0, 1000000);
        StartNewGame();
    }
}