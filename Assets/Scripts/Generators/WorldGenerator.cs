using System;
using System.Linq;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using NUnit.Framework;


public static class WorldGenerator
{
    public static void GenerateWorld(World world, int seed, int sizeInRegions, int regionSize, float seaPercent)
    {
        Map map = new Map(sizeInRegions, regionSize);

        world.seed = seed;
        world.random = new System.Random(seed);
        world.map = map;

        MapGenerator.GenerateMap(map, world.random, seaPercent);
    }
}