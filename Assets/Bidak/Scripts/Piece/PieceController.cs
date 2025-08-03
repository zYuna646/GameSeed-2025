using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Added for List
using Bidak.Data;
using Bidak.Manager;

public class PieceController : MonoBehaviour
{
    [Header("Piece Data")]
    public ChessPieceData pieceData;

    public int selectedMaterialIndex = 0;
    public Color playerType = Color.white;

    [Header("Component References")]
    public PieceMovementController movementController;
    public PieceAnimationController animationController;
    public PieceEffectController effectController;
    public PieceSoundsController soundsController;

    [Header("Piece Body")]
    public GameObject pieceBodyObject;

    [Header("State")]
    public bool isSelected = false;
    public bool isCaptured = false;

    private void Awake()
    {
        if (movementController == null)
            movementController = GetComponent<PieceMovementController>();
        if (animationController == null)
            animationController = GetComponent<PieceAnimationController>();
        if (effectController == null)
            effectController = GetComponent<PieceEffectController>();
        if (soundsController == null)
            soundsController = GetComponent<PieceSoundsController>();
    }

    private void Start()
    {
        // Find PieceBody child
        pieceBodyObject = FindPieceBodyChild();

        if (pieceData == null)
        {
            Debug.LogWarning("No piece data assigned to PieceController");
        }
        else
        {
            if (animationController != null)
            {
                animationController.pieceData = pieceData;
                animationController.pieceController = this;

                // Hide piece body before spawn effect
                if (pieceBodyObject != null)
                {
                    pieceBodyObject.SetActive(false);
                }

                animationController.SpawnPiece();
                if (soundsController != null)
                {
                    soundsController.pieceData = pieceData;
                    soundsController.PlaySpawnSound();
                }
            }

            selectedMaterialIndex = pieceData.selectedMaterialIndex;
            playerType = pieceData.playerType;
        }
    }

    private void Update()
    {
        if (pieceData != null)
        {
            Debug.Log("Updating piece appearance for piece: " + pieceData.pieceName);
            if (pieceData.selectedMaterialIndex != selectedMaterialIndex)
            {
                pieceData.selectedMaterialIndex = selectedMaterialIndex;
                UpdatePieceAppearance();
            }

            if (pieceData.playerType != playerType)
            {
                pieceData.playerType = playerType;
            }
        }
        
        // Removed old pieceColor update logic
    }

    private void UpdatePieceAppearance()
    {
        if (pieceData == null) return;

        // // Update main renderer
        // Renderer mainRenderer = GetComponent<Renderer>();
        // if (mainRenderer != null)
        // {
        //     if (pieceData.availableMaterials[pieceData.selectedMaterialIndex] != null)
        //     {
        //         mainRenderer.material = pieceData.availableMaterials[pieceData.selectedMaterialIndex];
        //     }
        // }

        // Update child objects with PieceBody tag
        UpdateChildObjectMaterials();
    }

    private void UpdateChildObjectMaterials()
    {
        if (pieceData == null || pieceData.availableMaterials[pieceData.selectedMaterialIndex] == null) return;
        // Find all child objects with PieceBody tag
        Transform[] allChildren = GetComponentsInChildren<Transform>(true);

        foreach (Transform child in allChildren)
        {
            if (child.CompareTag("PieceBody"))
            {
                // Ambil semua Renderer di dalam child tersebut, termasuk nested
                Renderer[] renderers = child.GetComponentsInChildren<Renderer>(true);

                foreach (Renderer renderer in renderers)
                {
                    // Ganti semua material di renderer ini
                    Material[] newMats = new Material[renderer.materials.Length];
                    for (int i = 0; i < newMats.Length; i++)
                    {
                        newMats[i] = pieceData.availableMaterials[pieceData.selectedMaterialIndex];
                    }
                    renderer.materials = newMats;
                }
            }
        }
    }

    public void SetPieceColor(Color color)
    {
        // Removed pieceColor logic
        UpdatePieceAppearance();
    }

    private GameObject FindPieceBodyChild()
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag("PieceBody"))
            {
                return child.gameObject;
            }
        }
        return null;
    }

    public void ShowPieceBody()
    {
        if (pieceBodyObject != null)
        {
            pieceBodyObject.SetActive(true);
        }
    }

    public void SetPieceData(ChessPieceData data)
    {
        pieceData = data;
        UpdatePieceAppearance();

        if (animationController != null)
        {
            animationController.pieceData = data;
        }
    }

    public void Capture()
    {
        isCaptured = true;

        if (animationController != null)
        {
            animationController.StartCapturing();
            animationController.SetCapturedPiece();
        }

        StartCoroutine(DisablePieceAfterDelay());
    }

    private IEnumerator DisablePieceAfterDelay()
    {
        yield return new WaitForSeconds(pieceData.deathAnimationSpeed);
        gameObject.SetActive(false);
    }

    public void ToggleSelection()
    {
        isSelected = !isSelected;
        transform.localScale = isSelected ? Vector3.one * 1.2f : Vector3.one;
    }

    public void SetPlayerType(Color playerType)
    {
        if (pieceData != null)
        {
            pieceData.playerType = playerType;
        }
    }

    public void CycleSelectedMaterial()
    {
        if (pieceData != null)
        {
            pieceData.selectedMaterialIndex = (pieceData.selectedMaterialIndex + 1) % pieceData.availableMaterials.Count;
            UpdatePieceAppearance();
        }
    }

    public void SetMaterial(int index)
    {
        if (pieceData != null)
        {
            pieceData.selectedMaterialIndex = index;
            UpdatePieceAppearance();
        }
    }

    // Card Effect Methods
    
    /// <summary>
    /// Apply a card effect to this piece
    /// </summary>
    public bool ApplyCardEffect(ChessCardData cardData)
    {
        if (cardData == null || pieceData == null)
        {
            Debug.LogWarning("Cannot apply card effect: missing card data or piece data");
            return false;
        }
        
        return CardEffectManager.Instance.ApplyCardEffect(cardData, pieceData, this);
    }
    
    /// <summary>
    /// Apply a specific effect to this piece
    /// </summary>
    public bool ApplyEffect(Bidak.Data.CardEffectData effectData)
    {
        if (effectData == null || pieceData == null)
        {
            Debug.LogWarning("Cannot apply effect: missing effect data or piece data");
            return false;
        }
        
        return CardEffectManager.Instance.ApplyEffect(effectData, pieceData, this);
    }
    
    /// <summary>
    /// Remove a specific card effect from this piece
    /// </summary>
    public void RemoveCardEffect(Bidak.Data.CardEffectType effectType)
    {
        if (pieceData != null)
        {
            CardEffectManager.Instance.RemoveEffect(effectType, pieceData);
        }
    }
    
    /// <summary>
    /// Check if this piece has a specific card effect
    /// </summary>
    public bool HasCardEffect(Bidak.Data.CardEffectType effectType)
    {
        return pieceData != null && pieceData.HasCardEffect(effectType);
    }
    
    /// <summary>
    /// Get all active card effects on this piece
    /// </summary>
    public List<CardEffectManager.ActiveCardEffect> GetActiveCardEffects()
    {
        if (pieceData == null)
            return new List<CardEffectManager.ActiveCardEffect>();
            
        return CardEffectManager.Instance.GetActiveEffectsForPiece(pieceData);
    }
    
    /// <summary>
    /// Reset movement for new turn (considering card effects)
    /// </summary>
    public void StartNewTurn()
    {
        if (pieceData != null)
        {
            pieceData.ResetMovementForTurn();
            
            // Increment turns without moving if piece didn't move last turn
            if (pieceData.remainingMoves == (pieceData.canMoveMultipleTimes ? 
                (pieceData.HasCardEffect(Bidak.Data.CardEffectType.TripleMove) ? 3 : 2) : 1))
            {
                pieceData.turnsWithoutMoving++;
            }
        }
    }
    
    /// <summary>
    /// Execute a move and handle card effect logic
    /// </summary>
    public bool TryMove(TileController targetTile)
    {
        if (pieceData == null || !pieceData.CanMoveThisTurn())
        {
            Debug.LogWarning("Cannot move: piece is not allowed to move this turn");
            return false;
        }
        
        // Check special movement restrictions from card effects
        if (pieceData.isBlockaded)
        {
            Debug.LogWarning("Cannot move: piece is blockaded");
            return false;
        }
        
        // Validate move based on card effects
        if (!ValidateMoveWithCardEffects(targetTile))
        {
            Debug.LogWarning("Move is not valid with current card effects");
            return false;
        }
        
        // Execute the move through movement controller
        if (movementController != null)
        {
            movementController.SetDestinationTile(targetTile);
            
            // Use one move
            pieceData.UseMove();
            
            // Handle special effects after move
            HandlePostMoveEffects(targetTile);
            
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Validate if a move is allowed based on active card effects
    /// </summary>
    private bool ValidateMoveWithCardEffects(TileController targetTile)
    {
        if (pieceData == null || targetTile == null)
            return false;
        
        // Get current tile
        TileController currentTile = movementController?.currentTile;
        if (currentTile == null)
            return false;
        
        Vector3 currentPos = currentTile.transform.position;
        Vector3 targetPos = targetTile.transform.position;
        Vector3 direction = targetPos - currentPos;
        
        // Check backward movement
        if (pieceData.canMoveBackward)
        {
            // Allow backward moves if effect is active
        }
        
        // Check sideways movement
        if (pieceData.canMoveSideways)
        {
            // Allow sideways moves if effect is active
            if (Mathf.Abs(direction.x) > 0 && Mathf.Abs(direction.z) < 0.1f)
            {
                return true; // Pure sideways move
            }
        }
        
        // Check diagonal attack capability
        if (pieceData.canAttackDiagonally && targetTile.currentPieceData != null)
        {
            // Allow diagonal attacks if effect is active
            if (Mathf.Abs(direction.x) > 0 && Mathf.Abs(direction.z) > 0)
            {
                return true; // Diagonal attack
            }
        }
        
        // Add more validation based on piece type and effects
        return ValidateBasicMove(currentTile, targetTile);
    }
    
    /// <summary>
    /// Basic move validation (can be extended)
    /// </summary>
    private bool ValidateBasicMove(TileController currentTile, TileController targetTile)
    {
        // Basic validation logic
        // This should be expanded with proper chess move validation
        return true;
    }
    
    /// <summary>
    /// Handle effects that trigger after a move
    /// </summary>
    private void HandlePostMoveEffects(TileController targetTile)
    {
        if (pieceData == null)
            return;
        
        // Handle Queen Collision effect
        if (pieceData.HasCardEffect(Bidak.Data.CardEffectType.QueenCollision) && 
            pieceData.pieceType == ChessPieceData.PieceType.Bishop &&
            targetTile.currentPieceData != null)
        {
            HandleQueenCollisionEffect(targetTile);
        }
        
        // Handle Dance Like Queen effect
        if (pieceData.HasCardEffect(Bidak.Data.CardEffectType.DanceLikeQueen) &&
            pieceData.pieceType == ChessPieceData.PieceType.Bishop &&
            targetTile.currentPieceData != null)
        {
            // Allow next move to be in straight line like queen
            pieceData.canMoveMultipleTimes = true;
            pieceData.remainingMoves = 1;
        }
        
        // Handle Nice Day effect (Queen can move twice if no attack)
        if (pieceData.HasCardEffect(Bidak.Data.CardEffectType.NiceDay) &&
            pieceData.pieceType == ChessPieceData.PieceType.Queen &&
            targetTile.currentPieceData == null)
        {
            // Allow second move if first was not an attack
            if (pieceData.remainingMoves == 0)
            {
                pieceData.remainingMoves = 1;
            }
        }
    }
    
    /// <summary>
    /// Handle Queen Collision effect (destroy diagonal pieces)
    /// </summary>
    private void HandleQueenCollisionEffect(TileController capturedTile)
    {
        // Find and destroy pieces on the same diagonal
        // This is a simplified implementation
        Debug.Log("Queen Collision effect triggered - should destroy diagonal pieces");
        
        // Implementation would find diagonal pieces and capture them
        // This requires access to the board manager to find diagonal tiles
    }
    
    /// <summary>
    /// Check if this piece can capture another piece
    /// </summary>
    public bool CanCapture(ChessPieceData targetPiece)
    {
        if (pieceData == null || targetPiece == null)
            return false;
        
        // Check protection effects
        if (targetPiece.hasFullProtection)
        {
            return false;
        }
        
        if (targetPiece.hasLightProtection)
        {
            // Can only be captured by stronger pieces
            return GetPieceStrength(pieceData.pieceType) > GetPieceStrength(targetPiece.pieceType);
        }
        
        // Check if different players
        return pieceData.playerType != targetPiece.playerType;
    }
    
    /// <summary>
    /// Get piece strength for protection comparison
    /// </summary>
    private int GetPieceStrength(ChessPieceData.PieceType pieceType)
    {
        switch (pieceType)
        {
            case ChessPieceData.PieceType.Pawn: return 1;
            case ChessPieceData.PieceType.Knight: return 3;
            case ChessPieceData.PieceType.Bishop: return 3;
            case ChessPieceData.PieceType.Rook: return 5;
            case ChessPieceData.PieceType.Queen: return 9;
            case ChessPieceData.PieceType.King: return 10;
            default: return 0;
        }
    }
}