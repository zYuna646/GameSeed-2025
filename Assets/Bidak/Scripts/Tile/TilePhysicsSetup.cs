using UnityEngine;

public class TilePhysicsSetup : MonoBehaviour
{
    [Header("Collider Configuration")]
    [Tooltip("Default center offset for the collider")]
    public Vector3 defaultCenter = new Vector3(1.761603f, 1.347913f, 1.743786f);

    [Tooltip("Default size for the collider")]
    public Vector3 defaultSize = new Vector3(1.761603f, 0.8260437f, 1.743786f);

    [Tooltip("Additional size offset")]
    public Vector3 colliderSizeOffset = Vector3.zero;

    [Tooltip("Additional center offset")]
    public Vector3 colliderCenterOffset = Vector3.zero;

    [Header("Layer and Tag")]
    [Tooltip("Layer to set for the tile")]
    public string boardLayerName = "Board";

    [Tooltip("Tag to set for the tile")]
    public string tileTag = "Tile";

    [Header("Debug")]
    public bool showColliderGizmo = true;

    private BoxCollider tileCollider;

    private void Awake()
    {
        SetupLayerAndTag();
        SetupCollider();
    }

    private void SetupLayerAndTag()
    {
        // Set layer
        int boardLayer = LayerMask.NameToLayer(boardLayerName);
        if (boardLayer != -1)
        {
            gameObject.layer = boardLayer;
        }
        else
        {
            Debug.LogWarning($"Layer '{boardLayerName}' not found. Creating it.");
            // Optionally create the layer if it doesn't exist
        }

        // Set tag
        gameObject.tag = tileTag;
    }

    private void SetupCollider()
    {
        // Remove existing colliders
        Collider[] existingColliders = GetComponents<Collider>();
        foreach (Collider col in existingColliders)
        {
            Destroy(col);
        }

        // Remove any existing Rigidbody
        Rigidbody existingRb = GetComponent<Rigidbody>();
        if (existingRb != null)
        {
            Destroy(existingRb);
        }

        // Add BoxCollider
        tileCollider = gameObject.AddComponent<BoxCollider>();

        // Use default size and center
        tileCollider.size = defaultSize + colliderSizeOffset;
        tileCollider.center = defaultCenter + colliderCenterOffset;

        // Ensure collider is not a trigger
        tileCollider.isTrigger = false;
    }

    // Method to manually adjust collider
    public void AdjustCollider(Vector3? size = null, Vector3? center = null)
    {
        if (tileCollider != null)
        {
            // Use provided values or defaults
            tileCollider.size = size ?? (defaultSize + colliderSizeOffset);
            tileCollider.center = center ?? (defaultCenter + colliderCenterOffset);
        }
    }

    // Debug visualization of collider in scene view
    private void OnDrawGizmosSelected()
    {
        if (!showColliderGizmo || tileCollider == null) return;

        Gizmos.color = Color.green;
        Gizmos.matrix = transform.localToWorldMatrix;

        // Draw wire cube representing the collider
        Gizmos.DrawWireCube(tileCollider.center, tileCollider.size);
    }

    // Utility method to create the Board layer if it doesn't exist
    [ContextMenu("Create Board Layer")]
    private void CreateBoardLayer()
    {
        #if UNITY_EDITOR
        UnityEditor.SerializedObject tagManager = new UnityEditor.SerializedObject(UnityEditor.AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        UnityEditor.SerializedProperty layersProp = tagManager.FindProperty("layers");

        // Find first empty layer slot
        for (int i = 8; i < layersProp.arraySize; i++)
        {
            UnityEditor.SerializedProperty layerProp = layersProp.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(layerProp.stringValue))
            {
                layerProp.stringValue = boardLayerName;
                tagManager.ApplyModifiedProperties();
                Debug.Log($"Created layer: {boardLayerName}");
                return;
            }
        }

        Debug.LogWarning("Could not create layer. All layer slots are full.");
        #endif
    }
} 