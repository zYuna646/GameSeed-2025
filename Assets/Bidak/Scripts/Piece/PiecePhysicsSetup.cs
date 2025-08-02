using UnityEngine;

public class PiecePhysicsSetup : MonoBehaviour
{
    [Header("Physics Configuration")]
    public bool freezeVerticalPosition = true;
    public bool useKinematicMode = true;
    public ColliderType colliderType = ColliderType.Box;

    public enum ColliderType
    {
        Box,
        Sphere,
        Capsule
    }

    private Rigidbody rb;
    private Collider pieceCollider;

    private void Awake()
    {
        SetupRigidbody();
        SetupCollider();
    }

    private void SetupRigidbody()
    {
        // Add or get existing Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // Configure Rigidbody
        rb.useGravity = false;
        rb.isKinematic = useKinematicMode;

        if (freezeVerticalPosition)
        {
            rb.constraints = RigidbodyConstraints.FreezePositionY | 
                             RigidbodyConstraints.FreezeRotationX | 
                             RigidbodyConstraints.FreezeRotationZ;
        }
    }

    private void SetupCollider()
    {
        // Remove existing colliders
        Collider[] existingColliders = GetComponents<Collider>();
        foreach (Collider col in existingColliders)
        {
            Destroy(col);
        }

        // Add appropriate collider based on type
        switch (colliderType)
        {
            case ColliderType.Box:
                pieceCollider = gameObject.AddComponent<BoxCollider>();
                break;
            case ColliderType.Sphere:
                pieceCollider = gameObject.AddComponent<SphereCollider>();
                break;
            case ColliderType.Capsule:
                pieceCollider = gameObject.AddComponent<CapsuleCollider>();
                break;
        }

        // Configure collider
        if (pieceCollider != null)
        {
            // Optional: Auto-size collider based on renderer
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                if (pieceCollider is BoxCollider boxCollider)
                {
                    boxCollider.size = renderer.bounds.size;
                }
                else if (pieceCollider is SphereCollider sphereCollider)
                {
                    sphereCollider.radius = Mathf.Max(
                        renderer.bounds.extents.x, 
                        renderer.bounds.extents.z
                    );
                }
                else if (pieceCollider is CapsuleCollider capsuleCollider)
                {
                    capsuleCollider.height = renderer.bounds.size.y;
                    capsuleCollider.radius = Mathf.Max(
                        renderer.bounds.extents.x, 
                        renderer.bounds.extents.z
                    );
                }
            }

            // Make collider a trigger if needed
            pieceCollider.isTrigger = false;
        }
    }

    // Optional: Method to adjust collider manually
    public void AdjustCollider(Vector3 size)
    {
        if (pieceCollider is BoxCollider boxCollider)
        {
            boxCollider.size = size;
        }
    }
} 