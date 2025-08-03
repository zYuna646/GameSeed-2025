using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering; // Added to resolve ShadowCastingMode
using Bidak.Data;

namespace Bidak.Manager
{
    public class ChessCardHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("Hover Effect Settings")]
        [SerializeField] private float hoverScaleMultiplier = 1.2f;
        [SerializeField] private float hoverRotationAngle = 10f;
        [SerializeField] private float animationSpeed = 5f;

        [Header("Color Effect")]
        [SerializeField] private Color hoverColor = new Color(1.2f, 1.2f, 1.2f, 1f);
        [SerializeField] private Color selectedColor = new Color(0.8f, 1.5f, 0.8f, 1f);
        [SerializeField] private Color activatedColor = new Color(1.5f, 0.8f, 0.8f, 1f);

        [Header("Card Data")]
        public ChessCardData cardData;
        public int playerIndex;

        [Header("Card State")]
        public bool isSelected = false;
        public bool isActivated = false;
        public bool canBeActivated = true;

        private Vector3 originalScale;
        private Quaternion originalRotation;
        private Renderer[] renderers;
        private Color[] originalColors;
        private ShadowCastingMode[] originalShadowModes;
        
        // References
        private ChessCardManager cardManager;
        private CameraSwitch cameraSwitch;
        private CardTargetingSystem targetingSystem;

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

            // Find references
            cardManager = FindObjectOfType<ChessCardManager>();
            cameraSwitch = FindObjectOfType<CameraSwitch>();
            targetingSystem = FindObjectOfType<CardTargetingSystem>();

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

            // Skip hover if card is already activated
            if (isActivated || !canBeActivated)
                return;

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

                // Change color based on state
                Color targetColor = isSelected ? selectedColor : hoverColor;
                ApplyColorToRenderers(targetColor);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Debug.Log($"Pointer exited {gameObject.name}");

            // Skip if card is activated or can't be activated
            if (isActivated || !canBeActivated)
                return;

            // Determine which camera is being used
            Camera currentCamera = eventData.pressEventCamera ?? Camera.main;

            // Only revert hover effect if the card is in the current camera's view
            if (IsCardInCurrentCamera(currentCamera))
            {
                // Scale back to original
                StartCoroutine(ScaleCard(originalScale));

                // Rotate back to original
                StartCoroutine(RotateCard(originalRotation));

                // Restore appropriate colors based on state
                if (isSelected)
                {
                    ApplyColorToRenderers(selectedColor);
                }
                else
                {
                    RestoreOriginalColors();
                }
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"Card clicked: {gameObject.name}");
            Debug.Log($"Card state - isActivated: {isActivated}, canBeActivated: {canBeActivated}, isSelected: {isSelected}");
            Debug.Log($"Player data - playerIndex: {playerIndex}, cardData: {cardData?.cardName ?? "NULL"}");
            Debug.Log($"Camera info - eventCamera: {eventData.pressEventCamera?.name ?? "NULL"}");

            // Skip if card is already activated or can't be activated
            if (isActivated || !canBeActivated)
            {
                Debug.Log($"Card cannot be activated - isActivated: {isActivated}, canBeActivated: {canBeActivated}");
                return;
            }

            // Check camera validity - for now, allow all cameras to work
            Camera currentCamera = eventData.pressEventCamera ?? Camera.main;
            Debug.Log($"Using camera: {currentCamera?.name ?? "None"}");

            // Check if it's the correct player's turn
            // CameraSwitch.player is 0-based (0,1), playerIndex is 1-based (1,2)
            int expectedCameraSwitchPlayer = playerIndex - 1;
            if (cameraSwitch != null && cameraSwitch.player != expectedCameraSwitchPlayer)
            {
                Debug.Log($"Not player {playerIndex}'s turn. CameraSwitch player: {cameraSwitch.player}, Expected: {expectedCameraSwitchPlayer}");
                return;
            }

            Debug.Log($"Processing card click - Current selection state: {isSelected}");

            // Toggle selection
            if (!isSelected)
            {
                Debug.Log("Selecting card...");
                SelectCard();
            }
            else
            {
                Debug.Log("Activating card...");
                ActivateCard();
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

        // Card State Management Methods
        
        private void SelectCard()
        {
            Debug.Log($"SelectCard() called for {cardData?.cardName ?? gameObject.name}");
            isSelected = true;
            ApplyColorToRenderers(selectedColor);
            Debug.Log($"Card selected: {cardData?.cardName ?? gameObject.name} - Color applied");
            
            // Deselect other cards
            DeselectOtherCards();
            
            // Notify camera switch
            if (cameraSwitch != null)
            {
                Debug.Log("Notifying camera switch...");
                cameraSwitch.SetSelectedCard(this);
            }
            else
            {
                Debug.LogWarning("CameraSwitch is null!");
            }
            
            // Start targeting mode if needed
            if (targetingSystem != null)
            {
                Debug.Log("Starting targeting system...");
                targetingSystem.StartTargeting(this);
            }
            else
            {
                Debug.LogWarning("TargetingSystem is null!");
            }
        }

        private void ActivateCard()
        {
            Debug.Log($"ActivateCard() called for {cardData?.cardName ?? gameObject.name}");
            
            if (cardData == null)
            {
                Debug.LogError("No card data attached to activate");
                return;
            }

            Debug.Log($"Activating card: {cardData.cardName}");

            // Apply card effects based on targeting
            if (targetingSystem != null && targetingSystem.HasValidTarget())
            {
                // Get targeted piece
                PieceController targetPiece = targetingSystem.GetTargetPiece();
                if (targetPiece != null)
                {
                    // Apply card effects to target piece
                    bool success = targetPiece.ApplyCardEffect(cardData);
                    
                    if (success)
                    {
                        // Card successfully applied
                        isActivated = true;
                        canBeActivated = false;
                        isSelected = false;
                        
                        // Change visual state
                        ApplyColorToRenderers(activatedColor);
                        
                        // Deactivate card in manager
                        if (cardManager != null)
                        {
                            cardManager.DeactivateCard(playerIndex, cardData);
                        }
                        
                        // End targeting
                        targetingSystem.EndTargeting();
                        
                        Debug.Log($"Card {cardData.cardName} successfully activated on {targetPiece.name}");
                    }
                    else
                    {
                        Debug.LogWarning($"Failed to apply card {cardData.cardName} to {targetPiece.name}");
                    }
                }
            }
            else
            {
                // Some cards might not need targeting (global effects)
                if (CardNeedsTargeting(cardData))
                {
                    Debug.LogWarning("Card requires targeting but no valid target selected");
                    return;
                }
                
                // Apply global effect
                bool success = CardEffectManager.Instance.ApplyCardEffect(cardData, null, null);
                if (success)
                {
                    isActivated = true;
                    canBeActivated = false;
                    isSelected = false;
                    ApplyColorToRenderers(activatedColor);
                    
                    if (cardManager != null)
                    {
                        cardManager.DeactivateCard(playerIndex, cardData);
                    }
                }
            }
        }

        private void DeselectOtherCards()
        {
            // Find all other card hover effects and deselect them
            ChessCardHoverEffect[] allCards = FindObjectsOfType<ChessCardHoverEffect>();
            foreach (var card in allCards)
            {
                if (card != this && card.playerIndex == playerIndex)
                {
                    card.Deselect();
                }
            }
        }

        public void Deselect()
        {
            if (isSelected && !isActivated)
            {
                isSelected = false;
                RestoreOriginalColors();
                Debug.Log($"Card deselected: {cardData?.cardName ?? gameObject.name}");
                
                // Notify camera switch
                if (cameraSwitch != null)
                {
                    cameraSwitch.ClearSelectedCard();
                }
            }
        }

        private bool CardNeedsTargeting(ChessCardData card)
        {
            if (card == null || card.cardEffects == null || card.cardEffects.Count == 0)
                return false;

            // Check if any effect requires targeting
            foreach (var effect in card.cardEffects)
            {
                switch (effect.effectType)
                {
                    case Bidak.Data.CardEffectType.DoubleMove:
                    case Bidak.Data.CardEffectType.TripleMove:
                    case Bidak.Data.CardEffectType.DiagonalAttack:
                    case Bidak.Data.CardEffectType.ProtectedRing:
                    case Bidak.Data.CardEffectType.ConquerorLeap:
                    case Bidak.Data.CardEffectType.NiceDay:
                    case Bidak.Data.CardEffectType.PowerfulMove:
                    case Bidak.Data.CardEffectType.UnstoppableForce:
                        return true; // These effects need a target piece
                }
            }
            
            return false;
        }

        private void ApplyColorToRenderers(Color color)
        {
            if (renderers != null)
            {
                foreach (Renderer renderer in renderers)
                {
                    if (renderer != null)
                    {
                        renderer.material.color = color;
                    }
                }
            }
        }

        private void RestoreOriginalColors()
        {
            if (renderers != null && originalColors != null)
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i] != null && i < originalColors.Length)
                    {
                        renderers[i].material.color = originalColors[i];
                    }
                }
            }
        }

        // Public methods for external control
        public void SetCardData(ChessCardData data, int player)
        {
            cardData = data;
            playerIndex = player;
            canBeActivated = true; // Ensure card can be activated when data is set
            isActivated = false;
            isSelected = false;
            
            Debug.Log($"SetCardData called - Card: {data?.cardName ?? "NULL"}, Player: {player}, canBeActivated: {canBeActivated}");
        }

        public void ForceDeactivate()
        {
            isActivated = true;
            canBeActivated = false;
            isSelected = false;
            ApplyColorToRenderers(activatedColor);
        }

        public void Reset()
        {
            isActivated = false;
            canBeActivated = true;
            isSelected = false;
            RestoreOriginalColors();
        }
    }
}