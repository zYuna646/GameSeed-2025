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
        
        [Header("Audio Effects")]
        [SerializeField] private AudioClip hoverSound;
        [SerializeField] private AudioClip selectSound;
        [SerializeField] private AudioClip activateSound;
        [SerializeField][Range(0f, 1f)] private float soundVolume = 0.5f;

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

            // Validate and fix interaction setup
            ValidateInteractionSetup();

            // Debug logging
            Debug.Log($"ChessCardHoverEffect initialized on {gameObject.name}");
            Debug.Log($"Original Scale: {originalScale}, Original Rotation: {originalRotation}");
            Debug.Log($"Renderers found: {renderers?.Length ?? 0}");
        }
        
        /// <summary>
        /// Validate and fix card interaction setup
        /// </summary>
        private void ValidateInteractionSetup()
        {
            bool hasIssues = false;
            
            // Check collider
            Collider col = GetComponent<Collider>();
            if (col == null || !col.enabled)
            {
                Debug.LogWarning($"Card {gameObject.name} missing or disabled collider - fixing...");
                hasIssues = true;
            }
            
            // Check if renderers exist
            if (renderers == null || renderers.Length == 0)
            {
                Debug.LogWarning($"Card {gameObject.name} has no renderers - interaction may not work");
                hasIssues = true;
            }
            
            // Check if card data is assigned
            if (cardData == null)
            {
                Debug.LogWarning($"Card {gameObject.name} has no card data assigned");
                hasIssues = true;
            }
            
            // Re-ensure event trigger if issues found
            if (hasIssues)
            {
                EnsureEventTrigger();
            }
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
                Bounds bounds = GetRendererBounds();
                
                // Ensure minimum size for reliable interaction
                Vector3 size = bounds.size;
                size.x = Mathf.Max(size.x, 0.1f);
                size.y = Mathf.Max(size.y, 0.1f);
                size.z = Mathf.Max(size.z, 0.1f);
                
                boxCollider.size = size;
                boxCollider.center = Vector3.zero;
                Debug.Log($"Added BoxCollider to {gameObject.name} with size {boxCollider.size}");
            }
            else
            {
                // Ensure existing collider is enabled and properly sized
                collider.enabled = true;
                if (collider is BoxCollider box && box.size == Vector3.zero)
                {
                    Bounds bounds = GetRendererBounds();
                    Vector3 size = bounds.size;
                    size.x = Mathf.Max(size.x, 0.1f);
                    size.y = Mathf.Max(size.y, 0.1f);
                    size.z = Mathf.Max(size.z, 0.1f);
                    box.size = size;
                }
            }

            // Ensure the object is on the correct layer for raycasting
            if (gameObject.layer == 0) // Default layer
            {
                gameObject.layer = LayerMask.NameToLayer("UI");
                if (gameObject.layer == -1) // If UI layer doesn't exist, use default
                {
                    gameObject.layer = 0;
                }
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
            Debug.Log($"Pointer entered {gameObject.name} - isActivated: {isActivated}, canBeActivated: {canBeActivated}");

            // Skip hover if card is already activated
            if (isActivated || !canBeActivated)
            {
                Debug.Log($"Skipping hover - card state: activated={isActivated}, canBeActivated={canBeActivated}");
                return;
            }

            // Check if this card belongs to the current player
            if (!IsCurrentPlayersTurn())
            {
                Debug.Log($"Skipping hover - not current player's turn. Card belongs to player {playerIndex}, current player is {GameManagerChess.Instance?.currentPlayerIndex + 1}");
                return;
            }

            // Determine which camera is being used
            Camera currentCamera = GetCurrentActiveCamera(eventData);
            Debug.Log($"Current Camera: {currentCamera?.name ?? "None"}");

            // More permissive camera check - allow hover if camera exists and card is accessible
            if (currentCamera != null && IsCardAccessibleFromCamera(currentCamera))
            {
                Debug.Log($"Applying hover effect for {gameObject.name}");
                
                // Play hover sound
                PlaySound(hoverSound, currentCamera);
                
                // Scale up
                StartCoroutine(ScaleCard(originalScale * hoverScaleMultiplier));

                // Rotate slightly
                StartCoroutine(RotateCard(Quaternion.Euler(0, 0, hoverRotationAngle)));

                // Change color based on state
                Color targetColor = isSelected ? selectedColor : hoverColor;
                ApplyColorToRenderers(targetColor);
                
                Debug.Log($"Applied hover effect with color: {targetColor}");
            }
            else
            {
                Debug.Log($"Skipping hover - camera validation failed. Camera: {currentCamera?.name ?? "None"}");
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Debug.Log($"Pointer exited {gameObject.name}");

            // Skip if card is activated or can't be activated
            if (isActivated || !canBeActivated)
                return;

            // Determine which camera is being used
            Camera currentCamera = GetCurrentActiveCamera(eventData);

            // Apply exit effect (more lenient camera check)
            if (ShouldApplyHoverEffect(currentCamera))
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

            // Check if this card belongs to the current player
            if (!IsCurrentPlayersTurn())
            {
                Debug.Log($"Skipping click - not current player's turn. Card belongs to player {playerIndex}, current player is {GameManagerChess.Instance?.currentPlayerIndex + 1}");
                return;
            }

            // Determine which camera is being used
            Camera currentCamera = GetCurrentActiveCamera(eventData);
            Debug.Log($"Using camera: {currentCamera?.name ?? "None"}");

            // Same permissive camera check as hover
            if (currentCamera != null && IsCardAccessibleFromCamera(currentCamera))
            {
                Debug.Log($"Processing card click - Current selection state: {isSelected}");

                // Toggle selection
                if (!isSelected)
                {
                    Debug.Log("Selecting card...");
                    PlaySound(selectSound, currentCamera);
                    SelectCard();
                }
                else
                {
                    Debug.Log("Activating card...");
                    PlaySound(activateSound, currentCamera);
                    ActivateCard();
                }
            }
            else
            {
                Debug.Log($"Skipping click - camera validation failed. Camera: {currentCamera?.name ?? "None"}");
            }
        }

        private bool IsCardInCurrentCamera(Camera currentCamera)
        {
            if (currentCamera == null) return false;

            // Check if the card is a child of the current camera
            return transform.IsChildOf(currentCamera.transform);
        }
        
        /// <summary>
        /// Get the current active camera from event data or camera switch
        /// </summary>
        private Camera GetCurrentActiveCamera(PointerEventData eventData)
        {
            // Try to get camera from event data first
            Camera eventCamera = eventData?.pressEventCamera ?? eventData?.enterEventCamera;
            
            // If no camera from event, try to get from camera switch
            if (eventCamera == null && cameraSwitch != null && cameraSwitch.currentCamera != null)
            {
                eventCamera = cameraSwitch.currentCamera;
            }
            
            // Final fallback to main camera
            return eventCamera ?? Camera.main;
        }
        
        /// <summary>
        /// Check if hover effect should be applied (permissive camera validation)
        /// </summary>
        private bool ShouldApplyHoverEffect(Camera currentCamera)
        {
            if (currentCamera == null) 
            {
                Debug.Log("No current camera - denying hover");
                return false;
            }
            
            // Check if it's the current player's turn first
            if (!IsCurrentPlayersTurn())
            {
                Debug.Log("Not current player's turn - denying hover");
                return false;
            }
            
            // More permissive approach - just check if card is accessible
            bool isAccessible = IsCardAccessibleFromCamera(currentCamera);
            Debug.Log($"Card accessible from camera: {isAccessible}");
            
            return isAccessible;
        }
        
        /// <summary>
        /// Check if the given camera belongs to the current player
        /// </summary>
        private bool IsCameraForCurrentPlayer(Camera camera)
        {
            if (camera == null || cameraSwitch == null || GameManagerChess.Instance == null)
                return false;
            
            int currentPlayerIndex = GameManagerChess.Instance.currentPlayerIndex;
            
            // Check if camera is the main camera for current player
            if (cameraSwitch.mainCameras != null && cameraSwitch.mainCameras.Length > currentPlayerIndex)
            {
                Camera expectedMainCamera = cameraSwitch.mainCameras[currentPlayerIndex]?.GetComponent<Camera>();
                if (camera == expectedMainCamera)
                {
                    Debug.Log($"Camera matches player {currentPlayerIndex + 1} main camera");
                    return true;
                }
            }
            
            // Check if camera is the card camera for current player  
            if (cameraSwitch.cardCameras != null && cameraSwitch.cardCameras.Length > currentPlayerIndex)
            {
                Camera expectedCardCamera = cameraSwitch.cardCameras[currentPlayerIndex]?.GetComponent<Camera>();
                if (camera == expectedCardCamera)
                {
                    Debug.Log($"Camera matches player {currentPlayerIndex + 1} card camera");
                    return true;
                }
            }
            
            Debug.Log($"Camera does not match any camera for player {currentPlayerIndex + 1}");
            return false;
        }
        
        /// <summary>
        /// Check if card is accessible from the given camera
        /// </summary>
        private bool IsCardAccessibleFromCamera(Camera camera)
        {
            if (camera == null) return false;
            
            // Check if the card is within camera's view frustum
            Bounds cardBounds = GetRendererBounds();
            Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
            
            return GeometryUtility.TestPlanesAABB(frustumPlanes, cardBounds);
        }
        
        /// <summary>
        /// Check if it's the current player's turn
        /// </summary>
        private bool IsCurrentPlayersTurn()
        {
            if (GameManagerChess.Instance == null) 
            {
                Debug.Log("No GameManagerChess instance - allowing turn");
                return true; // Fallback if no game manager
            }
            
            // Convert 1-based playerIndex to 0-based for GameManagerChess
            int expectedGameManagerPlayer = playerIndex - 1;
            bool isPlayerTurn = GameManagerChess.Instance.IsPlayerTurn(expectedGameManagerPlayer);
            
            Debug.Log($"Turn check: playerIndex={playerIndex}, expected={expectedGameManagerPlayer}, current={GameManagerChess.Instance.currentPlayerIndex}, result={isPlayerTurn}");
            
            return isPlayerTurn;
        }
        
        /// <summary>
        /// Play sound through the active camera's audio listener
        /// </summary>
        private void PlaySound(AudioClip clip, Camera targetCamera)
        {
            if (clip == null || targetCamera == null) return;
            
            // Try to find AudioListener on the camera
            AudioListener audioListener = targetCamera.GetComponent<AudioListener>();
            
            // If no listener on camera, try to find any active audio listener
            if (audioListener == null)
            {
                audioListener = FindObjectOfType<AudioListener>();
            }
            
            if (audioListener != null)
            {
                // Create temporary AudioSource to play the sound
                GameObject tempAudioObject = new GameObject("TempCardAudio");
                tempAudioObject.transform.position = audioListener.transform.position;
                
                AudioSource audioSource = tempAudioObject.AddComponent<AudioSource>();
                audioSource.clip = clip;
                audioSource.volume = soundVolume;
                audioSource.spatialBlend = 0f; // 2D sound
                audioSource.Play();
                
                // Destroy the temporary object after the clip finishes
                Destroy(tempAudioObject, clip.length + 0.1f);
                
                Debug.Log($"Playing {clip.name} sound at volume {soundVolume}");
            }
            else
            {
                Debug.LogWarning("No AudioListener found to play card sound");
            }
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
            
            // Enter card mode
            if (cardManager != null)
            {
                cardManager.EnterCardMode(this);
            }
            
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
                
                // Exit card mode if this card was selected
                if (cardManager != null && cardManager.GetSelectedCard() == this)
                {
                    cardManager.ExitCardMode();
                }
                
                // End targeting if active
                if (targetingSystem != null && targetingSystem.isTargeting)
                {
                    targetingSystem.EndTargeting();
                }
                
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
                    case Bidak.Data.CardEffectType.PowerfulMove:
                    case Bidak.Data.CardEffectType.UnstoppableForce:
                    case Bidak.Data.CardEffectType.NiceDay:
                    case Bidak.Data.CardEffectType.BackMove:
                    case Bidak.Data.CardEffectType.LeapMove:
                    case Bidak.Data.CardEffectType.RestoreMove:
                        return true; // These effects need a target piece
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Determine if card should end turn based on its type
        /// </summary>
        private bool ShouldCardEndTurn(ChessCardData card)
        {
            if (card == null || card.cardEffects == null || card.cardEffects.Count == 0)
                return true; // Default to ending turn
            
            foreach (var effect in card.cardEffects)
            {
                // Attack and Defense cards end the turn
                switch (effect.effectType)
                {
                    // Attack effects - end turn
                    case Bidak.Data.CardEffectType.QueenCollision:
                    case Bidak.Data.CardEffectType.DiagonalAttack:
                    case Bidak.Data.CardEffectType.UnstoppableForce:
                    case Bidak.Data.CardEffectType.DanceLikeQueen:
                    case Bidak.Data.CardEffectType.DanceLikeElephant:
                        Debug.Log($"Attack card {effect.effectType} - will end turn");
                        return true;
                    
                    // Defense effects - end turn
                    case Bidak.Data.CardEffectType.RoyalCommand:
                    case Bidak.Data.CardEffectType.WhereIsMyDefense:
                    case Bidak.Data.CardEffectType.NotToday:
                    case Bidak.Data.CardEffectType.TimeFrozen:
                    case Bidak.Data.CardEffectType.ProtectedRing:
                    case Bidak.Data.CardEffectType.IGotYou:
                        Debug.Log($"Defense card {effect.effectType} - will end turn");
                        return true;
                    
                    // Movement/Effect cards - don't end turn (player keeps their turn)
                    case Bidak.Data.CardEffectType.DoubleMove:
                    case Bidak.Data.CardEffectType.TripleMove:
                    case Bidak.Data.CardEffectType.StraightMove:
                    case Bidak.Data.CardEffectType.BlockadeMove:
                    case Bidak.Data.CardEffectType.ForwardTwoMoves:
                    case Bidak.Data.CardEffectType.TwoDirectionMove:
                    case Bidak.Data.CardEffectType.PowerfulMove:
                    case Bidak.Data.CardEffectType.NiceDay:
                    case Bidak.Data.CardEffectType.BackMove:
                    case Bidak.Data.CardEffectType.LeapMove:
                    case Bidak.Data.CardEffectType.RestoreMove:
                    case Bidak.Data.CardEffectType.BackFromDead:
                    case Bidak.Data.CardEffectType.StoneTomorrow:
                    case Bidak.Data.CardEffectType.SpecialMove:
                        Debug.Log($"Effect card {effect.effectType} - will NOT end turn");
                        return false;
                }
            }
            
            Debug.Log("Unknown card type - defaulting to end turn");
            return true; // Default to ending turn for unknown effects
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
            // Always update card data, even if it seems the same
            cardData = data;
            playerIndex = player;
            
            // Force reset interaction states
            canBeActivated = true;
            isActivated = false;
            isSelected = false;
            
            // Re-ensure event trigger and interaction setup
            EnsureEventTrigger();
            ValidateInteractionSetup();
            
            Debug.Log($"SetCardData FORCED UPDATE - Card: {data?.cardName ?? "NULL"}, Player: {player}, canBeActivated: {canBeActivated}");
        }
        
        /// <summary>
        /// Force update card data during runtime
        /// </summary>
        [ContextMenu("Force Card Data Update")]
        public void ForceCardDataUpdate()
        {
            if (cardData != null)
            {
                SetCardData(cardData, playerIndex);
                Debug.Log($"Manually forced card data update for {cardData.cardName}");
            }
        }
        
        /// <summary>
        /// Set audio settings from ChessCardManager
        /// </summary>
        /// <param name="hover">Hover sound clip</param>
        /// <param name="select">Select sound clip</param>
        /// <param name="activate">Activate sound clip</param>
        /// <param name="volume">Sound volume (0-1)</param>
        public void SetAudioSettings(AudioClip hover, AudioClip select, AudioClip activate, float volume)
        {
            hoverSound = hover;
            selectSound = select;
            activateSound = activate;
            soundVolume = Mathf.Clamp01(volume);
            
            Debug.Log($"Audio settings applied to {gameObject.name} - Volume: {soundVolume}");
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
        
        /// <summary>
        /// Force enable interaction for debugging
        /// </summary>
        [ContextMenu("Force Enable Interaction")]
        public void ForceEnableInteraction()
        {
            canBeActivated = true;
            isActivated = false;
            isSelected = false;
            
            // Re-ensure event trigger
            EnsureEventTrigger();
            
            Debug.Log($"Forced interaction enabled for {gameObject.name}");
        }
        
        /// <summary>
        /// Test click method for debugging
        /// </summary>
        [ContextMenu("Test Click")]
        public void TestClick()
        {
            Debug.Log($"Test click on {gameObject.name} - State: activated={isActivated}, canBeActivated={canBeActivated}, selected={isSelected}");
            
            if (!isSelected)
            {
                SelectCard();
            }
            else
            {
                ActivateCard();
            }
        }
        
        [ContextMenu("Debug Card State")]
        public void DebugCardState()
        {
            Debug.Log("=== CARD STATE DEBUG ===");
            Debug.Log($"Card Name: {cardData?.cardName ?? "NULL"}");
            Debug.Log($"Player Index: {playerIndex}");
            Debug.Log($"Can Be Activated: {canBeActivated}");
            Debug.Log($"Is Activated: {isActivated}");
            Debug.Log($"Is Selected: {isSelected}");
            Debug.Log($"Game Manager Exists: {GameManagerChess.Instance != null}");
            if (GameManagerChess.Instance != null)
            {
                Debug.Log($"Current Player Index: {GameManagerChess.Instance.currentPlayerIndex}");
                Debug.Log($"Current Player Name: {GameManagerChess.Instance.GetCurrentPlayerName()}");
                Debug.Log($"Is Current Player's Turn: {IsCurrentPlayersTurn()}");
            }
            Debug.Log($"Camera Switch Exists: {cameraSwitch != null}");
            if (cameraSwitch != null)
            {
                Debug.Log($"Current Camera: {cameraSwitch.currentCamera?.name ?? "NULL"}");
                Debug.Log($"Main Cameras Count: {cameraSwitch.mainCameras?.Length ?? 0}");
                Debug.Log($"Card Cameras Count: {cameraSwitch.cardCameras?.Length ?? 0}");
            }
            Debug.Log("========================");
        }
    }
}