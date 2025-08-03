using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using Bidak.Data;
using Bidak.Manager;

public class TileController : MonoBehaviour
{
    [Header("Tile Data")]
    public BoardTileData tileData;

    [Header("Piece Positioning")]
    public Vector3 spawnOffset = Vector3.up * 1.5f;
    public bool adjustHorizontalPosition = true;
    public bool adjustVerticalPosition = true;
    public float horizontalSpread = 0.2f;
    public float verticalSpread = 0.2f;

    [Header("Piece Rotation")]
    public Vector3 pieceRotation = Vector3.zero;

    [Header("Piece State")]
    public GameObject currentPieceObject;
    public PieceController currentPieceController;
    public ChessPieceData currentPieceData;

    private System.Random random = new System.Random();

    private void Start()
    {
        // Initial piece spawn
        TrySpawnPiece();
    }

    private void Update()
    {
        CheckAndUpdatePiece();
    }

    public void SetTileData(BoardTileData data)
    {
        tileData = data;
        // Attempt to spawn piece
        TrySpawnPiece();
    }

    public void SetTileOffset(Vector3 offset, float hSpread, float vSpread)
    {
        spawnOffset = offset;
        horizontalSpread = hSpread;
        verticalSpread = vSpread;
    }
    
    private void CheckAndUpdatePiece()
    {
        // Skip if no tile data
        if (tileData == null)
        {
            Debug.Log("Skipping piece update"); 
            return;
        }

        // Check if current piece data differs from tile's piece data
        ChessPieceData tileOccupyingPiece = tileData.occupyingPiece;

        // Conditions for respawning:
        // 1. Tile has a piece
        // 2. Current piece object is null
        // 3. Current piece data exists
        if (tileOccupyingPiece == null &&
            currentPieceObject == null &&
            currentPieceData != null)
        {
            // Spawn piece when object is missing
            TrySpawnPiece();
            return;
        }

        // Debug.Log("Current piece controller: " + currentPieceController);
        // Debug.Log("Current piece data: " + currentPieceData);
        // Original logic for updating existing piece
        if (tileOccupyingPiece != null &&
            currentPieceObject != null &&
            currentPieceData != null && 
            currentPieceController != null 
            )
        {
            // Compare piece data
            if (currentPieceController.pieceData != currentPieceData)
            {
                // Despawn current piece
                Destroy(currentPieceObject);
                currentPieceObject = null;
                currentPieceController = null;
                tileData.occupyingPiece = currentPieceData;

                // Spawn new piece
                TrySpawnPiece();
            }
        }
        else if (tileOccupyingPiece != null)
        {
            Debug.Log("Spawning piece");
            // Spawn piece if no piece exists
            TrySpawnPiece();
        }
    }

    public void TrySpawnPiece()
    {
        if (tileData == null)
            return;

        // If piece already exists, don't respawn
        if (currentPieceObject != null)
            return;

        // Instantiate piece prefab
        ChessPieceData pieceToSpawn = tileData.occupyingPiece;
        if (pieceToSpawn == null)
            if (currentPieceData == null)
                return; // No piece to spawn
            pieceToSpawn = currentPieceData;

        if (pieceToSpawn.modelPrefab != null)
        {
            // Create an instance of the piece data to avoid modifying the original
            ChessPieceData instancePieceData = Instantiate(pieceToSpawn);
            
            // Use precise spawn position method
            Vector3 spawnPosition = GetPreciseSpawnPosition();

            // Instantiate piece with static rotation
            currentPieceObject = Instantiate(
                instancePieceData.modelPrefab,
                spawnPosition,
                Quaternion.Euler(pieceRotation),
                transform
            );

            // Add "Piece" tag to the spawned piece
            currentPieceObject.tag = "Piece";

            // Get or add PieceController
            currentPieceController = currentPieceObject.GetComponent<PieceController>();
            if (currentPieceController == null)
            {
                currentPieceController = currentPieceObject.AddComponent<PieceController>();
            }

            // Set piece data in PieceController
            currentPieceController.SetPieceData(instancePieceData);
            
            // Set current tile through movement controller
            if (currentPieceController.movementController != null)
            {
                currentPieceController.movementController.SetCurrentTile(this);
            }

            // Set current piece data
            currentPieceData = instancePieceData;
            tileData.occupyingPiece = instancePieceData;

            // Name the piece
            currentPieceObject.name = $"{instancePieceData.pieceType} ({tileData.chessNotation})";

            // Get or add PieceEffectController
            PieceEffectController pieceEffectController = currentPieceObject.GetComponent<PieceEffectController>();
            if (pieceEffectController == null)
            {
                pieceEffectController = currentPieceObject.AddComponent<PieceEffectController>();
            }
            pieceEffectController.SetPieceData(instancePieceData);
            pieceEffectController.PlaySpawnEffect();
        }
        else
        {
            Debug.LogWarning($"No prefab for piece type {pieceToSpawn.pieceType}");
        }
    }

    // Method to get precise spawn position
    public Vector3 GetPreciseSpawnPosition()
    {
        // First, try to find a child object with SpawnPosition tag
        Transform spawnPositionChild = transform.Find("SpawnPosition");
        
        // If not found by name, search through all children with tag
        if (spawnPositionChild == null)
        {
            spawnPositionChild = transform.Find("*[SpawnPosition]");
        }

        // If still not found, search through all children
        if (spawnPositionChild == null)
        {
            foreach (Transform child in transform)
            {
                if (child.CompareTag("SpawnPosition"))
                {
                    spawnPositionChild = child;
                    break;
                }
            }
        }

        // If a spawn position child is found, return its world position
        if (spawnPositionChild != null)
        {
            return spawnPositionChild.position;
        }

        // Fallback to tile's transform position with offset
        return transform.position + spawnOffset;
    }

    private Vector3 CalculateSpawnPosition()
    {
        Vector3 basePosition = GetPreciseSpawnPosition();

        // Apply horizontal spread
        if (adjustHorizontalPosition)
        {
            float horizontalOffset = RandomSpread(horizontalSpread);
            basePosition += transform.right * horizontalOffset;
        }

        // Apply vertical spread
        if (adjustVerticalPosition)
        {
            float verticalOffset = RandomSpread(verticalSpread);
            basePosition += transform.up * verticalOffset;
        }

        return basePosition;
    }

    private float RandomSpread(float maxSpread)
    {
        return (float)((random.NextDouble() * 2 - 1) * maxSpread);
    }

    // Utility method to manually clear the piece
    public void ClearPiece()
    {
        if (currentPieceObject != null)
        {
            Destroy(currentPieceObject);
            currentPieceObject = null;
            currentPieceController = null;
        }

        // Clear piece data
        currentPieceData = null;
        tileData.occupyingPiece = null;
    }

    // Optional: Method to get current piece
    public GameObject GetCurrentPiece()
    {
        return currentPieceObject;
    }

    public void CaptureTile(ChessPieceData capturingPieceData)
    {
        // If there's a current piece on this tile, capture it
        if (currentPieceObject != null && currentPieceController != null)
        {
            // Trigger capture animation on the current piece
            currentPieceController.Capture();
        }

        // Clear the current piece
        ClearPiece();

        // Set the new piece data for this tile
        currentPieceData = capturingPieceData;
        
        // Update tile data's occupying piece
        if (tileData != null)
        {
            tileData.occupyingPiece = capturingPieceData;
        }

        // Spawn the new piece
        TrySpawnPiece();
    }

    // Optional: Add a method to check if the tile can be captured
    public bool CanBeCaptured(ChessPieceData capturingPieceData)
    {
        // If no piece on tile, it can be occupied
        if (currentPieceData == null)
            return true;
        
        // Check if piece can be captured based on card effects
        if (currentPieceController != null)
        {
            return currentPieceController.CanCapture(currentPieceData);
        }
        
        // Basic capture validation
        return currentPieceData.playerType != capturingPieceData.playerType;
    }
    
    /// <summary>
    /// Check if a piece can move to this tile considering card effects
    /// </summary>
    public bool CanMoveToTile(ChessPieceData movingPiece, TileController fromTile)
    {
        if (movingPiece == null)
            return false;
        
        // Check if tile is empty
        if (currentPieceData == null)
            return true;
        
        // Check if can capture piece on this tile
        if (currentPieceData.playerType != movingPiece.playerType)
        {
            // Check protection effects
            if (currentPieceData.hasFullProtection)
            {
                // Check if capturing piece has special abilities to bypass protection
                if (movingPiece.HasCardEffect(Bidak.Data.CardEffectType.UnstoppableForce) &&
                    (movingPiece.pieceType == ChessPieceData.PieceType.Queen || movingPiece.pieceType == ChessPieceData.PieceType.King))
                {
                    return true;
                }
                return false;
            }
            
            if (currentPieceData.hasLightProtection)
            {
                // Can only be captured by stronger pieces
                int movingStrength = GetPieceStrength(movingPiece.pieceType);
                int targetStrength = GetPieceStrength(currentPieceData.pieceType);
                return movingStrength > targetStrength;
            }
            
            return true; // Can capture
        }
        
        return false; // Cannot move to tile with same color piece
    }
    
    /// <summary>
    /// Apply special card effects when a piece moves to this tile
    /// </summary>
    public void HandleMoveEffects(ChessPieceData movingPiece, PieceController movingController)
    {
        if (movingPiece == null || movingController == null)
            return;
        
        // Handle Back from Dead effect
        if (movingPiece.HasCardEffect(Bidak.Data.CardEffectType.BackFromDead) && currentPieceData != null)
        {
            // Store the captured piece for potential revival
            HandleBackFromDeadEffect(currentPieceData, movingPiece);
        }
        
        // Handle Stone Tomorrow effect
        if (movingPiece.HasCardEffect(Bidak.Data.CardEffectType.StoneTomorrow) &&
            movingPiece.pieceType == ChessPieceData.PieceType.Pawn &&
            movingPiece.immobilityTurns >= 15)
        {
            HandleStoneTomorrowEffect(movingPiece, movingController);
        }
        
        // Handle Not Today effect (for kings under attack)
        if (currentPieceData != null && 
            currentPieceData.HasCardEffect(Bidak.Data.CardEffectType.NotToday) &&
            currentPieceData.pieceType == ChessPieceData.PieceType.King)
        {
            HandleNotTodayEffect(movingPiece, movingController);
        }
    }
    
    /// <summary>
    /// Handle the Back from Dead effect
    /// </summary>
    private void HandleBackFromDeadEffect(ChessPieceData capturedPiece, ChessPieceData capturingPiece)
    {
        // Store captured piece data for potential revival
        // This would require a revival manager or game state manager
        Debug.Log($"Back from Dead effect: {capturedPiece.pieceName} can potentially be revived");
        
        // Implementation would add the piece to a revival list
        // For now, just log the effect
    }
    
    /// <summary>
    /// Handle the Stone Tomorrow effect (pawn promotion after immobility)
    /// </summary>
    private void HandleStoneTomorrowEffect(ChessPieceData pawn, PieceController pawnController)
    {
        if (pawn.pieceType == ChessPieceData.PieceType.Pawn)
        {
            Debug.Log("Stone Tomorrow effect: Pawn can promote in place");
            
            // Allow in-place promotion
            pawn.canPromoteInPlace = true;
            
            // This would typically open a UI for piece selection
            // For now, auto-promote to Queen
            PromotePawnInPlace(pawn, pawnController, ChessPieceData.PieceType.Queen);
        }
    }
    
    /// <summary>
    /// Handle the Not Today effect (cancel attacks on king)
    /// </summary>
    private void HandleNotTodayEffect(ChessPieceData attackingPiece, PieceController attackingController)
    {
        Debug.Log("Not Today effect: Attack on king cancelled, attacker moves back");
        
        // Cancel the attack and move attacker back
        // This would require access to the previous position
        // For now, just prevent the capture
        
        if (attackingController.movementController != null)
        {
            // Move attacking piece back to previous position
            // This requires storing the previous tile
            Debug.Log("Moving attacking piece back to previous position");
        }
    }
    
    /// <summary>
    /// Promote a pawn in place
    /// </summary>
    private void PromotePawnInPlace(ChessPieceData pawn, PieceController pawnController, ChessPieceData.PieceType newPieceType)
    {
        Debug.Log($"Promoting pawn to {newPieceType} in place");
        
        // Update piece type
        pawn.pieceType = newPieceType;
        pawn.SetDefaultMovement();
        
        // Update the piece controller's appearance
        // This would require changing the model/prefab
        // For now, just update the name
        if (currentPieceObject != null)
        {
            currentPieceObject.name = $"{newPieceType} (Promoted - {tileData.chessNotation})";
        }
        
        // Remove the promotion effect
        pawn.RemoveCardEffect(Bidak.Data.CardEffectType.StoneTomorrow);
        pawn.canPromoteInPlace = false;
    }
    
    /// <summary>
    /// Get piece strength for comparison
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
    
    /// <summary>
    /// Check if this tile is in a diagonal line from another tile
    /// </summary>
    public bool IsOnDiagonalFrom(TileController otherTile)
    {
        if (otherTile == null)
            return false;
        
        Vector3 direction = transform.position - otherTile.transform.position;
        
        // Check if movement is purely diagonal
        return Mathf.Abs(Mathf.Abs(direction.x) - Mathf.Abs(direction.z)) < 0.1f;
    }
    
    /// <summary>
    /// Get all tiles on the same diagonal from this tile
    /// </summary>
    public List<TileController> GetDiagonalTiles(int maxDistance = 8)
    {
        List<TileController> diagonalTiles = new List<TileController>();
        
        // This would require access to the board manager
        // For now, return empty list
        // Implementation would find all tiles on the four diagonals
        
        return diagonalTiles;
    }
}