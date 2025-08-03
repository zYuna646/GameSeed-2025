using UnityEngine;
using UnityEngine.EventSystems;
using Bidak.Data;
using Bidak.Manager;

namespace Bidak.Manager
{
    /// <summary>
    /// Handles targeting of pieces when cards are selected
    /// </summary>
    public class CardTargetingSystem : MonoBehaviour
    {
        [Header("Targeting Settings")]
        [SerializeField] private LayerMask pieceLayerMask = -1;
        [SerializeField] private Material highlightMaterial;
        [SerializeField] private Color targetHighlightColor = Color.green;
        [SerializeField] private Color invalidTargetColor = Color.red;

        [Header("Current State")]
        public bool isTargeting = false;
        public ChessCardHoverEffect selectedCard;
        public PieceController targetPiece;
        public PieceController hoveredPiece;
        
        [Header("Audio Feedback")]
        [SerializeField] private AudioClip targetConfirmSound;
        [SerializeField] private AudioClip targetCancelSound;
        [SerializeField][Range(0f, 1f)] private float soundVolume = 0.7f;

        // Visual feedback
        private Material originalMaterial;
        private Renderer targetRenderer;

        private Camera currentCamera;
        private CameraSwitch cameraSwitch;

        private void Start()
        {
            cameraSwitch = FindObjectOfType<CameraSwitch>();
        }

        private void Update()
        {
            if (isTargeting)
            {
                HandleTargeting();
            }
        }

        public void StartTargeting(ChessCardHoverEffect card)
        {
            selectedCard = card;
            isTargeting = true;
            
            // Get current camera from camera switch
            if (cameraSwitch != null)
            {
                currentCamera = cameraSwitch.currentCamera;
            }
            else
            {
                currentCamera = Camera.main;
            }

            Debug.Log($"Started targeting for card: {card.cardData?.cardName ?? "Unknown"}");
        }

        public void EndTargeting()
        {
            isTargeting = false;
            selectedCard = null;
            
            // Clear visual feedback
            ClearHighlight();
            
            targetPiece = null;
            hoveredPiece = null;

            Debug.Log("Ended targeting mode");
        }
        
        public void EndTargetingWithSound(bool wasSuccessful = false)
        {
            // Play appropriate sound
            AudioClip soundToPlay = wasSuccessful ? targetConfirmSound : targetCancelSound;
            PlayTargetingSound(soundToPlay);
            
            EndTargeting();
        }
        
        private void PlayTargetingSound(AudioClip clip)
        {
            if (clip == null) return;
            
            // Try to find active camera's audio listener
            AudioListener audioListener = null;
            
            if (currentCamera != null)
            {
                audioListener = currentCamera.GetComponent<AudioListener>();
            }
            
            if (audioListener == null)
            {
                audioListener = FindObjectOfType<AudioListener>();
            }
            
            if (audioListener != null)
            {
                // Create temporary AudioSource to play the sound
                GameObject tempAudioObject = new GameObject("TempTargetingAudio");
                tempAudioObject.transform.position = audioListener.transform.position;
                
                AudioSource audioSource = tempAudioObject.AddComponent<AudioSource>();
                audioSource.clip = clip;
                audioSource.volume = soundVolume;
                audioSource.spatialBlend = 0f; // 2D sound
                audioSource.Play();
                
                // Destroy the temporary object after the clip finishes
                Destroy(tempAudioObject, clip.length + 0.1f);
            }
        }

        private void HandleTargeting()
        {
            if (currentCamera == null) return;

            // Raycast from camera to detect pieces
            Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, pieceLayerMask))
            {
                // Check if hit object is a piece
                PieceController piece = hit.collider.GetComponent<PieceController>();
                if (piece == null)
                {
                    // Try to get piece from parent
                    piece = hit.collider.GetComponentInParent<PieceController>();
                }

                if (piece != null && piece != hoveredPiece)
                {
                    // Clear previous highlight
                    ClearHighlight();
                    
                    // Highlight new piece
                    HighlightPiece(piece);
                    hoveredPiece = piece;
                }

                // Handle click to select target
                if (Input.GetMouseButtonDown(0))
                {
                    if (piece != null && IsValidTarget(piece))
                    {
                        targetPiece = piece;
                        Debug.Log($"Selected target piece: {piece.name}");
                        
                        // If card is already selected, activate it immediately
                        if (selectedCard != null && selectedCard.isSelected)
                        {
                            selectedCard.OnPointerClick(new PointerEventData(EventSystem.current));
                        }
                    }
                    else
                    {
                        Debug.Log("Invalid target selected");
                    }
                }
            }
            else
            {
                // No piece hit, clear highlight
                if (hoveredPiece != null)
                {
                    ClearHighlight();
                    hoveredPiece = null;
                }
            }

            // Cancel targeting with right click or escape
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                CancelTargeting();
            }
        }

        private bool IsValidTarget(PieceController piece)
        {
            if (selectedCard == null || selectedCard.cardData == null || piece == null)
                return false;

            // Check if piece belongs to the correct player
            int cardPlayerIndex = selectedCard.playerIndex;
            
            // Most cards can only target own pieces, some can target enemy pieces
            foreach (var effect in selectedCard.cardData.cardEffects)
            {
                // Effects that can target enemy pieces
                if (effect.effectType == CardEffectType.QueenCollision ||
                    effect.effectType == CardEffectType.ConquerorLeap ||
                    effect.effectType == CardEffectType.PowerfulMove)
                {
                    // These can target any piece for attack
                    return true;
                }
                
                // Effects that target own pieces
                if (effect.effectType == CardEffectType.DoubleMove ||
                    effect.effectType == CardEffectType.TripleMove ||
                    effect.effectType == CardEffectType.ProtectedRing ||
                    effect.effectType == CardEffectType.UnstoppableForce)
                {
                    // Check if piece belongs to the same player
                    // This is a simplified check - you might want to implement proper player ownership
                    Color cardPlayerColor = cardPlayerIndex == 0 ? Color.white : Color.black;
                    return piece.pieceData.playerType == cardPlayerColor;
                }
            }

            // Default: can target own pieces
            return true;
        }

        private void HighlightPiece(PieceController piece)
        {
            if (piece == null) return;

            // Get renderer from piece body
            if (piece.pieceBodyObject != null)
            {
                targetRenderer = piece.pieceBodyObject.GetComponent<Renderer>();
                if (targetRenderer == null)
                {
                    targetRenderer = piece.pieceBodyObject.GetComponentInChildren<Renderer>();
                }
            }
            else
            {
                targetRenderer = piece.GetComponentInChildren<Renderer>();
            }

            if (targetRenderer != null)
            {
                // Store original material
                originalMaterial = targetRenderer.material;
                
                // Create highlight material
                Material highlightMat = new Material(originalMaterial);
                
                // Set highlight color based on validity
                Color highlightColor = IsValidTarget(piece) ? targetHighlightColor : invalidTargetColor;
                highlightMat.color = highlightColor;
                highlightMat.SetFloat("_Metallic", 0.8f);
                highlightMat.SetFloat("_Glossiness", 0.9f);
                
                targetRenderer.material = highlightMat;
            }
        }

        private void ClearHighlight()
        {
            if (targetRenderer != null && originalMaterial != null)
            {
                targetRenderer.material = originalMaterial;
                targetRenderer = null;
                originalMaterial = null;
            }
        }

        private void CancelTargeting()
        {
            Debug.Log("Targeting cancelled");
            
            // Deselect the card
            if (selectedCard != null)
            {
                selectedCard.Deselect();
            }
            
            EndTargeting();
        }

        // Public methods for external access
        public bool HasValidTarget()
        {
            return targetPiece != null && IsValidTarget(targetPiece);
        }

        public PieceController GetTargetPiece()
        {
            return targetPiece;
        }

        public bool IsCurrentlyTargeting()
        {
            return isTargeting;
        }

        // Cleanup
        private void OnDestroy()
        {
            ClearHighlight();
        }
    }
}