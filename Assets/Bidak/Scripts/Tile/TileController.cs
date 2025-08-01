using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
        // Tile can be captured if it has a piece and the piece is not from the same color
        // return currentPieceObject != null && 
        //        currentPieceData != null && 
        //        currentPieceData.pieceColor != capturingPieceData.pieceColor;
        return true;
    }
}