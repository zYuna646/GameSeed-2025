using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TileController))]
public class TileControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Ensure we have a valid target
        if (target == null)
        {
            EditorGUILayout.HelpBox("No TileController found", MessageType.Error);
            return;
        }

        try 
        {
            // Draw default inspector first
            DrawDefaultInspector();

            // Get the TileController component
            TileController tileController = (TileController)target;

            // Add some custom information
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tile Details", EditorStyles.boldLabel);

            // Display Tile Data details if available
            if (tileController.tileData != null)
            {
                EditorGUILayout.LabelField($"Notation: {tileController.tileData.chessNotation ?? "N/A"}");
                EditorGUILayout.LabelField($"Position: {tileController.tileData.worldPosition}");
                EditorGUILayout.LabelField($"Grid Position: {tileController.tileData.gridPosition}");
            }
            else
            {
                EditorGUILayout.HelpBox("No Tile Data assigned", MessageType.Warning);
            }

            // Display Current Piece details
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Piece Details", EditorStyles.boldLabel);

            if (tileController.currentPieceData != null)
            {
                EditorGUILayout.LabelField($"Piece Type: {tileController.currentPieceData.pieceType}");
            }
            else
            {
                EditorGUILayout.HelpBox("No Piece Data assigned", MessageType.Info);
            }

            // Display Current Piece Object
            if (tileController.currentPieceObject != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Current Piece Object", EditorStyles.boldLabel);
                EditorGUILayout.ObjectField("Piece Object", tileController.currentPieceObject, typeof(GameObject), true);
            }

            // Add a button to manually clear the piece
            EditorGUILayout.Space();
            if (GUILayout.Button("Clear Piece"))
            {
                tileController.ClearPiece();
            }

            // Apply any changes
            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in TileControllerEditor: {e.Message}");
            EditorGUILayout.HelpBox($"Error rendering TileController: {e.Message}", MessageType.Error);
        }
    }
} 