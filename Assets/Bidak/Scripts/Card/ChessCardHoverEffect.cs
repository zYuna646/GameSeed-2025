using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering; // Added to resolve ShadowCastingMode

namespace Bidak.Manager
{
    public class ChessCardHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Hover Effect Settings")]
        [SerializeField] private float hoverScaleMultiplier = 1.2f;
        [SerializeField] private float hoverRotationAngle = 10f;
        [SerializeField] private float animationSpeed = 5f;

        [Header("Color Effect")]
        [SerializeField] private Color hoverColor = new Color(1.2f, 1.2f, 1.2f, 1f);

        private Vector3 originalScale;
        private Quaternion originalRotation;
        private Renderer[] renderers;
        private Color[] originalColors;
        private ShadowCastingMode[] originalShadowModes;

        private void Awake()
        {
            // Ensure the object can receive pointer events
            EnsureEventTrigger();
        }

        private void Start()
        {
            // Store original transformations
            originalScale = transform.localScale;
            originalRotation = transform.localRotation;

            // Get all renderers in this object and its children
            renderers = GetComponentsInChildren<Renderer>();
            
            // Store original colors and shadow modes
            if (renderers != null && renderers.Length > 0)
            {
                originalColors = new Color[renderers.Length];
                originalShadowModes = new ShadowCastingMode[renderers.Length];

                for (int i = 0; i < renderers.Length; i++)
                {
                    originalColors[i] = renderers[i].material.color;
                    originalShadowModes[i] = renderers[i].shadowCastingMode;

                    // Disable shadows for all renderers
                    renderers[i].shadowCastingMode = ShadowCastingMode.Off;
                }
            }

            // Debug logging
            Debug.Log($"ChessCardHoverEffect initialized on {gameObject.name}");
            Debug.Log($"Original Scale: {originalScale}, Original Rotation: {originalRotation}");
            Debug.Log($"Renderers found: {renderers?.Length ?? 0}");
        }

        private void OnDestroy()
        {
            // Restore shadow modes when the object is destroyed
            if (renderers != null && originalShadowModes != null)
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i] != null)
                    {
                        renderers[i].shadowCastingMode = originalShadowModes[i];
                    }
                }
            }
        }

        private void EnsureEventTrigger()
        {
            // Add collider if not present
            Collider collider = GetComponent<Collider>();
            if (collider == null)
            {
                BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
                boxCollider.size = GetRendererBounds().size;
                Debug.Log($"Added BoxCollider to {gameObject.name} with size {boxCollider.size}");
            }

            // Add Physics Raycaster to all cameras
            AddPhysicsRaycasterToAllCameras();

            // Ensure event system exists
            EnsureEventSystem();
        }

        private void AddPhysicsRaycasterToAllCameras()
        {
            Camera[] allCameras = Camera.allCameras;
            foreach (Camera cam in allCameras)
            {
                PhysicsRaycaster raycaster = cam.GetComponent<PhysicsRaycaster>();
                if (raycaster == null)
                {
                    cam.gameObject.AddComponent<PhysicsRaycaster>();
                    Debug.Log($"Added PhysicsRaycaster to {cam.name}");
                }
            }
        }

        private void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<EventSystem>();
                eventSystemObj.AddComponent<StandaloneInputModule>();
                Debug.Log("Created EventSystem for card interactions");
            }
        }

        private Bounds GetRendererBounds()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return new Bounds(transform.position, Vector3.one);

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
            return bounds;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log($"Pointer entered {gameObject.name}");

            // Determine which camera is being used
            Camera currentCamera = eventData.pressEventCamera ?? Camera.main;
            Debug.Log($"Current Camera: {currentCamera?.name ?? "None"}");

            // Only apply hover effect if the card is in the current camera's view
            if (IsCardInCurrentCamera(currentCamera))
            {
                // Scale up
                StartCoroutine(ScaleCard(originalScale * hoverScaleMultiplier));

                // Rotate slightly
                StartCoroutine(RotateCard(Quaternion.Euler(0, 0, hoverRotationAngle)));

                // Change color
                if (renderers != null)
                {
                    foreach (Renderer renderer in renderers)
                    {
                        if (renderer != null)
                        {
                            renderer.material.color = hoverColor;
                            Debug.Log($"Changed color of renderer {renderer.name} to {hoverColor}");
                        }
                    }
                }
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Debug.Log($"Pointer exited {gameObject.name}");

            // Determine which camera is being used
            Camera currentCamera = eventData.pressEventCamera ?? Camera.main;

            // Only revert hover effect if the card is in the current camera's view
            if (IsCardInCurrentCamera(currentCamera))
            {
                // Scale back to original
                StartCoroutine(ScaleCard(originalScale));

                // Rotate back to original
                StartCoroutine(RotateCard(originalRotation));

                // Restore original colors
                if (renderers != null && originalColors != null)
                {
                    for (int i = 0; i < renderers.Length; i++)
                    {
                        if (renderers[i] != null)
                        {
                            renderers[i].material.color = originalColors[i];
                            Debug.Log($"Restored color of renderer {renderers[i].name} to {originalColors[i]}");
                        }
                    }
                }
            }
        }

        private bool IsCardInCurrentCamera(Camera currentCamera)
        {
            if (currentCamera == null) return false;

            // Check if the card is a child of the current camera
            return transform.IsChildOf(currentCamera.transform);
        }

        private System.Collections.IEnumerator ScaleCard(Vector3 targetScale)
        {
            float elapsedTime = 0;
            Vector3 startScale = transform.localScale;

            Debug.Log($"Scaling from {startScale} to {targetScale}");

            while (elapsedTime < 1f / animationSpeed)
            {
                transform.localScale = Vector3.Lerp(startScale, targetScale, elapsedTime * animationSpeed);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.localScale = targetScale;
        }

        private System.Collections.IEnumerator RotateCard(Quaternion targetRotation)
        {
            float elapsedTime = 0;
            Quaternion startRotation = transform.localRotation;

            Debug.Log($"Rotating from {startRotation} to {targetRotation}");

            while (elapsedTime < 1f / animationSpeed)
            {
                transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime * animationSpeed);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.localRotation = targetRotation;
        }
    }
}