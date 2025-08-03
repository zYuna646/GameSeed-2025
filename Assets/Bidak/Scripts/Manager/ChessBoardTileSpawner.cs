using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ChessBoardTileSpawner : MonoBehaviour
{
    public GameObject tileManager;
    [Header("Board Configuration")]
    public int boardSize = 8;
    
    [Header("Tile Configuration")]
    public BoardTileData whiteTileData;
    public BoardTileData blackTileData;
    
    [Header("Piece Configuration")]
    public List<ChessPieceData> pieceTemplates = new List<ChessPieceData>();

    [Header("Tile Spacing")]
    public float tileWidth = 1f;
    public float tileLength = 1f;
    public float tileSeparation = 0f;

    [Header("Tile Offset")]
    public Vector3 tileOffset = Vector3.up * 1.5f;
    public float horizontalSpread = 0.2f;
    public float verticalSpread = 0.2f;

    [Header("Board Layout")]
    public Vector3 boardOrigin = Vector3.zero;
    public bool centerBoard = true;
    public bool rotateBoard = false;

    [Header("Rendering")]
    public bool showTileNotation = true;

    private void Start()
    {
        SpawnBoard();
    }

    public void SpawnBoard()
    {
        // Clear existing tiles
        ClearExistingTiles();

        // Calculate board center if needed
        Vector3 boardCenter = CalculateBoardCenter();

        // Spawn tiles
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                // Determine tile color
                bool isWhiteTile = (x + y) % 2 == 0;
                BoardTileData tileData = isWhiteTile ? Instantiate(whiteTileData) : Instantiate(blackTileData);

                // Calculate tile position
                Vector3 tilePosition = CalculateTilePosition(x, y, boardCenter);

                // Spawn tile
                GameObject tile = Instantiate(
                    tileData.tilePrefab, 
                    tilePosition, 
                    Quaternion.identity, 
                    transform
                );

                // Optional: Name the tile for hierarchy clarity
                string notation = $"{(char)('A' + x)}{y + 1}";
                tile.name = $"Tile_{notation}";

                // Create and set tile data
                BoardTileData spawnedTileData = ScriptableObject.CreateInstance<BoardTileData>();
                spawnedTileData.SetTileDetails(
                    notation, 
                    tilePosition, 
                    new Vector2Int(x, y), 
                    tileData.tileColor,
                    y,  // row
                    x   // column
                );
                spawnedTileData.tilePrefab = tileData.tilePrefab;

                // Add TileController
                TileController tileController = tile.AddComponent<TileController>();
                tileController.SetTileData(spawnedTileData);
                tileController.SetTileOffset(tileOffset, horizontalSpread, verticalSpread);
                
                // Setup chess piece for this tile
                ChessPieceData pieceToSpawn = GetPieceForTile(notation);
                if (pieceToSpawn != null)
                {
                    tileController.currentPieceData = pieceToSpawn;
                }

                // Add notation text if enabled
                if (showTileNotation)
                {
                    AddTileNotation(tile, notation);
                }
            }
        }
        tileManager.SetActive(true);
    }

    private Vector3 CalculateTilePosition(int x, int y, Vector3 boardCenter)
    {
        // Calculate base position with separation
        Vector3 localPosition = new Vector3(
            x * (tileWidth + tileSeparation),
            0,
            y * (tileLength + tileSeparation)
        );

        // Adjust for board centering
        if (centerBoard)
        {
            Vector3 boardOffset = new Vector3(
                -(boardSize * (tileWidth + tileSeparation)) / 2 + ((tileWidth + tileSeparation) / 2),
                0,
                -(boardSize * (tileLength + tileSeparation)) / 2 + ((tileLength + tileSeparation) / 2)
            );

            localPosition += boardOffset;
        }

        // Optional board rotation
        if (rotateBoard)
        {
            localPosition = Quaternion.Euler(0, 180, 0) * localPosition;
        }

        // Apply board origin
        return boardOrigin + localPosition;
    }

    private Vector3 CalculateBoardCenter()
    {
        // Calculate board center based on size, tile size, and separation
        return boardOrigin + new Vector3(
            (boardSize * (tileWidth + tileSeparation)) / 2,
            0,
            (boardSize * (tileLength + tileSeparation)) / 2
        );
    }

    private void AddTileNotation(GameObject tile, string notation)
    {
        // You might want to create a TextMesh or UI Text for notation
        // This is a placeholder - implement based on your UI system
        Debug.Log($"Tile {notation} created");
    }

    private void ClearExistingTiles()
    {
        // Destroy all existing child tiles
        foreach (Transform child in transform)
        {
            // Use Destroy in edit mode and play mode
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
    }

    // Regenerate board with new parameters
    public void RespawnBoard(int newSize, float newTileWidth, float newTileLength, float newSeparation)
    {
        boardSize = newSize;
        tileWidth = newTileWidth;
        tileLength = newTileLength;
        tileSeparation = newSeparation;
        SpawnBoard();
    }

    /// <summary>
    /// Get the appropriate chess piece for a given tile notation
    /// </summary>
    private ChessPieceData GetPieceForTile(string notation)
    {
        if (pieceTemplates == null || pieceTemplates.Count == 0)
            return null;
            
        // Parse notation (e.g., "A1", "B2", etc.)
        char column = notation[0];
        int row = int.Parse(notation.Substring(1));
        
        ChessPieceData.PieceType pieceType;
        bool isPlayer1; // true for player 1 (white), false for player 2 (black)
        
        // Determine piece type and player based on standard chess layout
        if (row == 2) // White pawns
        {
            pieceType = ChessPieceData.PieceType.Pawn;
            isPlayer1 = true;
        }
        else if (row == 7) // Black pawns
        {
            pieceType = ChessPieceData.PieceType.Pawn;
            isPlayer1 = false;
        }
        else if (row == 1) // White back row
        {
            isPlayer1 = true;
            pieceType = GetBackRowPieceType(column);
        }
        else if (row == 8) // Black back row
        {
            isPlayer1 = false;
            pieceType = GetBackRowPieceType(column);
        }
        else
        {
            // Empty tile
            return null;
        }
        
        // Find template for this piece type
        ChessPieceData template = pieceTemplates.Find(p => p.pieceType == pieceType);
        if (template == null)
        {
            Debug.LogWarning($"No template found for piece type: {pieceType}");
            return null;
        }
        
        // Create instance with proper material and player settings
        return CreatePieceDataInstance(template, isPlayer1, notation);
    }
    
    /// <summary>
    /// Get piece type for back row based on column
    /// </summary>
    private ChessPieceData.PieceType GetBackRowPieceType(char column)
    {
        switch (column)
        {
            case 'A':
            case 'H':
                return ChessPieceData.PieceType.Rook;
            case 'B':
            case 'G':
                return ChessPieceData.PieceType.Knight;
            case 'C':
            case 'F':
                return ChessPieceData.PieceType.Bishop;
            case 'D':
                return ChessPieceData.PieceType.Queen;
            case 'E':
                return ChessPieceData.PieceType.King;
            default:
                return ChessPieceData.PieceType.Pawn;
        }
    }
    
    /// <summary>
    /// Create a piece data instance with proper material and player settings
    /// </summary>
    private ChessPieceData CreatePieceDataInstance(ChessPieceData template, bool isPlayer1, string notation)
    {
        // Create instance
        ChessPieceData pieceData = Instantiate(template);
        
        // Get player settings from GameManagerChess
        GameManagerChess gameManager = GameManagerChess.Instance;
        
        // Set player-specific properties
        if (isPlayer1)
        {
            if (gameManager != null)
            {
                pieceData.playerType = gameManager.player1Color;
                pieceData.selectedMaterialIndex = gameManager.player1MaterialIndex;
            }
            else
            {
                // Fallback values if GameManagerChess is not available
                pieceData.playerType = Color.white;
                pieceData.selectedMaterialIndex = 0;
            }
            pieceData.isWhite = true;
        }
        else
        {
            if (gameManager != null)
            {
                pieceData.playerType = gameManager.player2Color;
                pieceData.selectedMaterialIndex = gameManager.player2MaterialIndex;
            }
            else
            {
                // Fallback values if GameManagerChess is not available
                pieceData.playerType = Color.black;
                pieceData.selectedMaterialIndex = 1;
            }
            pieceData.isWhite = false;
        }
        
        // Set position data
        pieceData.currentTileNotation = notation;
        pieceData.SetDefaultMovement();
        
        return pieceData;
    }

    // Editor support for manual respawning
    #if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Respawn Chess Board")]
    private static void RespawnBoardMenuItem()
    {
        ChessBoardTileSpawner spawner = FindObjectOfType<ChessBoardTileSpawner>();
        if (spawner != null)
        {
            spawner.SpawnBoard();
        }
    }
    #endif
} 