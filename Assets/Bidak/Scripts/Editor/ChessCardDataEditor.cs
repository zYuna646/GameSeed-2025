using UnityEngine;
using UnityEditor;
using Bidak.Data;

namespace Bidak.Editor
{
    [CustomEditor(typeof(ChessCardData))]
    public class ChessCardDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // Get the target ChessCardData
            ChessCardData chessCardData = (ChessCardData)target;

            // Start checking for changes
            EditorGUI.BeginChangeCheck();

            // Draw default inspector fields
            DrawDefaultInspector();

            // Optional: Add custom validation or additional GUI elements
            if (chessCardData.points < 0)
            {
                EditorGUILayout.HelpBox("Card points cannot be negative.", MessageType.Warning);
            }

            // Check if any changes were made
            if (EditorGUI.EndChangeCheck())
            {
                // Save changes
                EditorUtility.SetDirty(target);
            }
        }
    }
}