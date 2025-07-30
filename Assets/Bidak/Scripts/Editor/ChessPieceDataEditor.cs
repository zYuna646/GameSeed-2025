using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ChessPieceData))]
public class ChessPieceDataDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Null checks
        if (property == null)
        {
            Debug.LogError("SerializedProperty is null");
            return;
        }

        EditorGUI.BeginProperty(position, label, property);

        try 
        {
            // Full width object field for direct assignment
            Rect objectFieldRect = new Rect(
                position.x, 
                position.y, 
                position.width, 
                EditorGUIUtility.singleLineHeight
            );

            // Draw object field first
            EditorGUI.PropertyField(objectFieldRect, property, label);

            // If property is expanded, show details
            if (property.isExpanded)
            {
                // Increase indent
                EditorGUI.indentLevel++;

                // Track vertical position
                float verticalPosition = objectFieldRect.y + EditorGUIUtility.singleLineHeight;
                float lineHeight = EditorGUIUtility.singleLineHeight;
                float verticalSpacing = EditorGUIUtility.standardVerticalSpacing;

                // Helper method to draw a property field
                void DrawPropertyField(string propertyName, ref float yPos)
                {
                    SerializedProperty prop = property.FindPropertyRelative(propertyName);
                    if (prop != null)
                    {
                        Rect propRect = new Rect(
                            position.x, 
                            yPos, 
                            position.width, 
                            lineHeight
                        );
                        EditorGUI.PropertyField(propRect, prop);
                        yPos += lineHeight + verticalSpacing;
                    }
                }

                // Draw individual properties
                DrawPropertyField("pieceType", ref verticalPosition);
                DrawPropertyField("modelPrefab", ref verticalPosition);
                DrawPropertyField("pieceColor", ref verticalPosition);

                // Decrease indent
                EditorGUI.indentLevel--;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in ChessPieceDataDrawer: {e.Message}");
            EditorGUI.LabelField(position, "Error rendering ChessPieceData");
        }
        finally
        {
            EditorGUI.EndProperty();
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // If expanded, return height for all fields
        if (property != null && property.isExpanded)
        {
            // Base height + additional properties
            return EditorGUIUtility.singleLineHeight * 4 + 
                   EditorGUIUtility.standardVerticalSpacing * 3;
        }
        
        // If not expanded, return default single line height
        return EditorGUIUtility.singleLineHeight;
    }
} 