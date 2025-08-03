using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Bidak.Data;

namespace Bidak.Manager
{
    public class ChessCardDisplay : MonoBehaviour
    {
        [Header("Player Card Display Settings")]
        [SerializeField] private ChessCardManager cardManager;

        [Header("Player 1 Display")]
        [SerializeField] private Camera player1Camera;
        [SerializeField] private GridLayoutGroup player1CardGrid;

        [Header("Player 2 Display")]
        [SerializeField] private Camera player2Camera;
        [SerializeField] private GridLayoutGroup player2CardGrid;

        [Header("Card Positioning")]
        [SerializeField] private Vector3 cardLocalPosition = new Vector3(0, -1f, 5f); // Local position relative to camera
        [SerializeField] private float cardSpacing = 2f; // Space between cards

        private void Start()
        {
            // Ensure Event System exists
            EnsureEventSystem();

            // Find cameras by tag if not assigned in inspector
            if (player1Camera == null)
                player1Camera = GameObject.FindGameObjectWithTag("Player1Camera")?.GetComponent<Camera>();

            if (player2Camera == null)
                player2Camera = GameObject.FindGameObjectWithTag("Player2Camera")?.GetComponent<Camera>();

            // Find card manager if not assigned
            if (cardManager == null)
                cardManager = FindObjectOfType<ChessCardManager>();

            // Validate references
            if (player1Camera == null || player2Camera == null || cardManager == null)
            {
                Debug.LogError("Missing critical references: Cameras or Card Manager not found.");
                enabled = false;
                return;
            }

            // Ensure main camera has PhysicsRaycaster for 3D hover effects
            EnsurePhysicsRaycaster();

            // Subscribe to player cards changed event
            cardManager.OnPlayerCardsChanged += UpdatePlayerCardDisplay;

            // Initial display
            UpdatePlayerCardDisplay(1);
            UpdatePlayerCardDisplay(2);
        }

        private void EnsureEventSystem()
        {
            // Check if Event System exists
            if (FindObjectOfType<EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<EventSystem>();
                eventSystemObj.AddComponent<StandaloneInputModule>();
                Debug.Log("Created EventSystem for card interactions");
            }
        }

        private void EnsurePhysicsRaycaster()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                PhysicsRaycaster raycaster = mainCamera.GetComponent<PhysicsRaycaster>();
                if (raycaster == null)
                {
                    mainCamera.gameObject.AddComponent<PhysicsRaycaster>();
                    Debug.Log($"Added PhysicsRaycaster to {mainCamera.name}");
                }
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events to prevent memory leaks
            if (cardManager != null)
            {
                cardManager.OnPlayerCardsChanged -= UpdatePlayerCardDisplay;
            }
        }

        private void UpdatePlayerCardDisplay(int playerIndex)
        {
            // Select the appropriate camera
            Camera targetCamera = playerIndex == 1 ? player1Camera : player2Camera;

            // Validate references
            if (targetCamera == null || cardManager == null)
            {
                Debug.LogWarning($"Missing references for Player {playerIndex} card display");
                return;
            }

            // Get active cards for the player
            List<ChessCardData> activeCards = cardManager.GetActiveCards(playerIndex);

            // Destroy existing cards under the camera
            foreach (Transform existingCard in targetCamera.transform)
            {
                if (existingCard.CompareTag("PlayerCard"))
                {
                    Destroy(existingCard.gameObject);
                }
            }

            // Calculate card positioning
            int cardCount = activeCards.Count;

            // Instantiate cards
            for (int i = 0; i < activeCards.Count; i++)
            {
                ChessCardData cardData = activeCards[i];

                // Use cardPrefab from ChessCardData
                if (cardData.cardPrefab == null)
                {
                    Debug.LogWarning($"No prefab found for card: {cardData.cardName}");
                    continue;
                }

                // Instantiate card as child of camera
                GameObject cardInstance = Instantiate(cardData.cardPrefab, targetCamera.transform);

                // Set tag for easy identification and future cleanup
                cardInstance.tag = "PlayerCard";

                // Position card locally relative to camera
                cardInstance.transform.localPosition = cardLocalPosition +
                       Vector3.right * (i - (cardCount - 1) / 2f) * cardSpacing;

                // Rotate card to face camera directly
                cardInstance.transform.rotation = Quaternion.LookRotation(
                       targetCamera.transform.forward,
                       targetCamera.transform.up
                );

                // Find card header by tag recursively
                Transform cardHeaderTransform = FindCardHeaderByTag(cardInstance.transform);
                if (cardHeaderTransform == null)
                {
                    Debug.LogWarning($"No CardHeader found in card prefab: {cardData.cardName}");
                    continue;
                }

                // Get renderer components
                Renderer cardHeaderRenderer = cardHeaderTransform.GetComponent<Renderer>();
                if (cardHeaderRenderer == null)
                {
                    Debug.LogWarning($"No Renderer found on CardHeader: {cardData.cardName}");
                    continue;
                }

                // Apply material from ChessCardData
                Material cardMaterial = new Material(cardData.cardMaterial);
                cardHeaderRenderer.material = cardMaterial;

                // Add hover effect and event trigger
                AddHoverEffectAndEventTrigger(cardInstance, cardData, playerIndex);
            }
        }

        private void AddHoverEffectAndEventTrigger(GameObject cardObject, ChessCardData cardData, int playerIndex)
        {
            // Debug logging
            Debug.Log($"Setting up hover effect for {cardObject.name}");

            // Add hover effect script
            ChessCardHoverEffect hoverEffect = cardObject.AddComponent<ChessCardHoverEffect>();
            
            // Set card data and player index
            hoverEffect.SetCardData(cardData, playerIndex);

            // Ensure collider exists
            Collider collider = cardObject.GetComponent<Collider>();
            if (collider == null)
            {
                BoxCollider boxCollider = cardObject.AddComponent<BoxCollider>();
                
                // Try to set collider size based on renderer bounds
                Renderer[] renderers = cardObject.GetComponentsInChildren<Renderer>();
                if (renderers.Length > 0)
                {
                    Bounds combinedBounds = renderers[0].bounds;
                    for (int i = 1; i < renderers.Length; i++)
                    {
                        combinedBounds.Encapsulate(renderers[i].bounds);
                    }
                    boxCollider.size = combinedBounds.size;
                    Debug.Log($"Added BoxCollider to {cardObject.name} with size {boxCollider.size}");
                }
            }

            // Add Event Trigger component
            EventTrigger eventTrigger = cardObject.GetComponent<EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = cardObject.AddComponent<EventTrigger>();
            }

            // Clear existing triggers to prevent duplicates
            eventTrigger.triggers.Clear();

            // Pointer Enter event
            EventTrigger.Entry enterEvent = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            enterEvent.callback.AddListener((data) => { 
                PointerEventData pointerData = data as PointerEventData;
                Debug.Log($"Pointer Enter triggered on {cardObject.name} by {pointerData?.pointerCurrentRaycast.gameObject?.name}");
                hoverEffect.OnPointerEnter(pointerData); 
            });
            eventTrigger.triggers.Add(enterEvent);

            // Pointer Exit event
            EventTrigger.Entry exitEvent = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerExit
            };
            exitEvent.callback.AddListener((data) => { 
                PointerEventData pointerData = data as PointerEventData;
                Debug.Log($"Pointer Exit triggered on {cardObject.name} by {pointerData?.pointerCurrentRaycast.gameObject?.name}");
                hoverEffect.OnPointerExit(pointerData); 
            });
            eventTrigger.triggers.Add(exitEvent);

            // Pointer Click event
            EventTrigger.Entry clickEvent = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerClick
            };
            clickEvent.callback.AddListener((data) => { 
                PointerEventData pointerData = data as PointerEventData;
                Debug.Log($"Pointer Click triggered on {cardObject.name} by {pointerData?.pointerCurrentRaycast.gameObject?.name}");
                hoverEffect.OnPointerClick(pointerData); 
            });
            eventTrigger.triggers.Add(clickEvent);

            // Ensure the card is set as a layer that can be raycasted
            cardObject.layer = LayerMask.NameToLayer("UI");
        }

        // Recursive method to find CardHeader by tag
        private Transform FindCardHeaderByTag(Transform parent)
        {
            // First, check if current object is tagged as CardHeader
            if (parent.CompareTag("CardHeader"))
                return parent;

            // Search through all children recursively
            foreach (Transform child in parent)
            {
                // Check if current child is tagged as CardHeader
                if (child.CompareTag("CardHeader"))
                    return child;

                // Recursively search child's children
                Transform cardHeader = FindCardHeaderByTag(child);
                if (cardHeader != null)
                    return cardHeader;
            }

            // Not found
            return null;
        }
    }
}