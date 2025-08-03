using UnityEngine;

// Add this using directive
using System.Linq;

public class ChessBoardTileSpawner : MonoBehaviour
{
    [Header("Board Configuration")]
    public int boardSize = 8;
    
    [Header("Tile Configuration")]
    public BoardTileData whiteTileData;
    public BoardTileData blackTileData;

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
                var tileControllerType = System.Type.GetType("TileController");
                if (tileControllerType != null)
                {
                    var controller = tile.AddComponent(tileControllerType);
                    var setDataMethod = tileControllerType.GetMethod("SetTileData");
                    var setOffsetMethod = tileControllerType.GetMethod("SetTileOffset");
                    
                    if (setDataMethod != null)
                    {
                        setDataMethod.Invoke(controller, new object[] { spawnedTileData });
                    }

                    if (setOffsetMethod != null)
                    {
                        setOffsetMethod.Invoke(controller, new object[] { 
                            tileOffset, 
                            horizontalSpread, 
                            verticalSpread 
                        });
                    }
                }

                // Add notation text if enabled
                if (showTileNotation)
                {
                    AddTileNotation(tile, notation);
                }
            }
        }
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