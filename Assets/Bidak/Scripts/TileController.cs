using System;
using UnityEngine;

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

        Debug.Log("Trying to update piece");
        // Debug.Log("Current piece controller: " + currentPieceController);
        // Debug.Log("Current piece data: " + currentPieceData);
        Debug.Log("Tile occupying piece: " + tileOccupyingPiece);
        // Original logic for updating existing piece
        if (tileOccupyingPiece != null &&
            currentPieceObject != null &&
            currentPieceData != null && 
            currentPieceController != null 
            )
        {
            // Compare piece data
            Debug.Log("Comparing piece data");
            Debug.Log("Current piece data: " + currentPieceController.pieceData);
            Debug.Log("Tile occupying piece: " + tileOccupyingPiece);
            Debug.Log("Curent Piece Data: " + currentPieceData);
            if (currentPieceController.pieceData != currentPieceData)
            {
                Debug.Log("Despawning piece");
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
            // Calculate spawn position with optional spread
            Vector3 spawnPosition = CalculateSpawnPosition();

            // Instantiate piece with static rotation
            currentPieceObject = Instantiate(
                pieceToSpawn.modelPrefab,
                spawnPosition,
                Quaternion.Euler(pieceRotation),
                transform
            );

            // Get or add PieceController
            currentPieceController = currentPieceObject.GetComponent<PieceController>();
            if (currentPieceController == null)
            {
                currentPieceController = currentPieceObject.AddComponent<PieceController>();
            }

            // Set piece data in PieceController
            currentPieceController.SetPieceData(pieceToSpawn);
            currentPieceController.SetCurrentTile(this);

            // Set current piece data
            currentPieceData = pieceToSpawn;
            tileData.occupyingPiece = pieceToSpawn;

            // Name the piece
            currentPieceObject.name = $"{pieceToSpawn.pieceType} ({tileData.chessNotation})";
        }
        else
        {
            Debug.LogWarning($"No prefab for piece type {pieceToSpawn.pieceType}");
        }
    }

    private Vector3 CalculateSpawnPosition()
    {
        Vector3 basePosition = transform.position + spawnOffset;

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
    }

    // Optional: Method to get current piece
    public GameObject GetCurrentPiece()
    {
        return currentPieceObject;
    }
}