using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapVisualizer))] 
public class MapVisualizerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(15);

        MapVisualizer scriptTarget = (MapVisualizer)target;

        if (GUILayout.Button("Voronoi/Delone Graph", GUILayout.Height(20)))
        {
            scriptTarget.DrawMapGraph();
        }

        if (GUILayout.Button("Sea/Land Graph", GUILayout.Height(20)))
        {
            scriptTarget.DrawSeaLandGraph();
        }

        if (GUILayout.Button("Landmasses Graph", GUILayout.Height(20)))
        {
            scriptTarget.DrawLandmasses();
        }

        if (GUILayout.Button("Tectonic Forces", GUILayout.Height(20)))
        {
            scriptTarget.DrawTectonicForces();
        }

        if (GUILayout.Button("Regions Heights", GUILayout.Height(20)))
        {
            scriptTarget.DrawRegionsHeights();
        }

        if (GUILayout.Button("Regions And Border Points Heights", GUILayout.Height(20)))
        {
            scriptTarget.DrawRegionsAndBorderPointsHeights();
        }

        if (GUILayout.Button("Cells Height", GUILayout.Height(20)))
        {
            scriptTarget.DrawCellsHeights();
        }

        if (GUILayout.Button("Basins", GUILayout.Height(20)))
        {
            scriptTarget.DrawBasins();
        }

        if (GUILayout.Button("Regions", GUILayout.Height(20)))
        {
            scriptTarget.DrawRegions();
        }
    }
}