using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameController))] 
public class GameControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(15);

        GameController scriptTarget = (GameController)target;

        if (GUILayout.Button("New Game", GUILayout.Height(20)))
        {
            scriptTarget.StartNewGame();
        }

        if (GUILayout.Button("New Game with random seed", GUILayout.Height(20)))
        {
            scriptTarget.StartNewGameWithRandomSeed();
        }
    }
}