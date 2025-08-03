using System.Collections.Generic;
using UnityEngine;

public class ChessPieceSpawner : MonoBehaviour
{
    [Header("Piece Configuration")]
    public List<ChessPieceData> piecesTemplate;

    [Header("Spawn References")]
    public ChessBoardTileSpawner boardSpawner;

    [Header("Piece Positioning")]
    public float pieceYOffset = 0.5f;

    // Separate parent for pieces to keep hierarchy clean
    private Transform piecesParent;

    private void Awake()
    {
        // Create a parent transform for pieces
        piecesParent = new GameObject("ChessPieces").transform;
        piecesParent.SetParent(transform);
    }

    private void Start()
    {
        // Ensure board spawner is set
        if (boardSpawner == null)
        {
            boardSpawner = FindObjectOfType<ChessBoardTileSpawner>();
            if (boardSpawner == null)
            {
                Debug.LogError("No Board Spawner found in the scene!");
                return;
            }
        }

        // Ensure pieces template is not empty
        if (piecesTemplate == null || piecesTemplate.Count == 0)
        {
            Debug.LogError("No piece templates assigned!");
            return;
        }

        // Validate piece templates
        foreach (var template in piecesTemplate)
        {
            if (template == null)
            {
                Debug.LogError("Null piece template found!");
                return;
            }
            if (template.modelPrefab == null)
            {
                Debug.LogError($"No model prefab for piece type: {template.pieceType}");
                return;
            }
        }

        // Attempt to spawn pieces
        SpawnChessPieces();
    }

    public void SpawnChessPieces()
    {
        // Clear existing pieces
        ClearExistingPieces();

        Debug.Log("Spawning White Pieces");
        // Spawn white pieces
        SpawnWhitePieces();

        Debug.Log("Spawning Black Pieces");
        // Spawn black pieces
        SpawnBlackPieces();
    }

    private void SpawnWhitePieces()
    {
        // Spawn white pawns
        for (int x = 0; x < boardSpawner.boardSize; x++)
        {
            SpawnPiece(
                GetTilePosition($"{(char)('A' + x)}2"), 
                ChessPieceData.PieceType.Pawn, 
                Color.white
            );
        }

        // Spawn white back row
        SpawnPiece(GetTilePosition("A1"), ChessPieceData.PieceType.Rook, Color.white);
        SpawnPiece(GetTilePosition("B1"), ChessPieceData.PieceType.Knight, Color.white);
        SpawnPiece(GetTilePosition("C1"), ChessPieceData.PieceType.Bishop, Color.white);
        SpawnPiece(GetTilePosition("D1"), ChessPieceData.PieceType.Queen, Color.white);
        SpawnPiece(GetTilePosition("E1"), ChessPieceData.PieceType.King, Color.white);
        SpawnPiece(GetTilePosition("F1"), ChessPieceData.PieceType.Bishop, Color.white);
        SpawnPiece(GetTilePosition("G1"), ChessPieceData.PieceType.Knight, Color.white);
        SpawnPiece(GetTilePosition("H1"), ChessPieceData.PieceType.Rook, Color.white);
    }

    private void SpawnBlackPieces()
    {
        // Spawn black pawns
        for (int x = 0; x < boardSpawner.boardSize; x++)
        {
            SpawnPiece(
                GetTilePosition($"{(char)('A' + x)}7"), 
                ChessPieceData.PieceType.Pawn, 
                Color.black
            );
        }

        // Spawn black back row
        SpawnPiece(GetTilePosition("A8"), ChessPieceData.PieceType.Rook, Color.black);
        SpawnPiece(GetTilePosition("B8"), ChessPieceData.PieceType.Knight, Color.black);
        SpawnPiece(GetTilePosition("C8"), ChessPieceData.PieceType.Bishop, Color.black);
        SpawnPiece(GetTilePosition("D8"), ChessPieceData.PieceType.Queen, Color.black);
        SpawnPiece(GetTilePosition("E8"), ChessPieceData.PieceType.King, Color.black);
        SpawnPiece(GetTilePosition("F8"), ChessPieceData.PieceType.Bishop, Color.black);
        SpawnPiece(GetTilePosition("G8"), ChessPieceData.PieceType.Knight, Color.black);
        SpawnPiece(GetTilePosition("H8"), ChessPieceData.PieceType.Rook, Color.black);
    }

    private void SpawnPiece(Vector3 position, ChessPieceData.PieceType pieceType, Color pieceColor)
    {
        // Find the appropriate piece template based on type
        ChessPieceData templatePiece = piecesTemplate.Find(p => p.pieceType == pieceType);
        
        if (templatePiece == null)
        {
            Debug.LogError($"No template found for piece type: {pieceType}");
            return;
        }

        if (templatePiece.modelPrefab == null)
        {
            Debug.LogError($"No model prefab for piece type: {pieceType}");
            return;
        }

        Debug.Log($"Spawning {pieceColor} {pieceType} at position {position}");

        // Create piece game object using prefab from template
        GameObject pieceObject = Instantiate(
            templatePiece.modelPrefab, 
            position + Vector3.up * pieceYOffset, 
            Quaternion.identity, 
            piecesParent  // Use dedicated pieces parent
        );

        // Name the piece for clarity
        pieceObject.name = $"{pieceColor} {pieceType}";

        // Modify piece color
        Renderer pieceRenderer = pieceObject.GetComponent<Renderer>();
        if (pieceRenderer != null)
        {
            // Create a new material instance to avoid modifying the original
            Material pieceMaterial = new Material(pieceRenderer.sharedMaterial);
            pieceMaterial.color = pieceColor;
            pieceRenderer.material = pieceMaterial;
        }

        // Create piece data
        ChessPieceData pieceData = ScriptableObject.CreateInstance<ChessPieceData>();
        pieceData.pieceType = pieceType;
        pieceData.startPosition = position;
        pieceData.currentPosition = position;
        pieceData.SetDefaultMovement();

        // Optional: Add piece data component to game object
        PieceDataHolder dataHolder = pieceObject.AddComponent<PieceDataHolder>();
        dataHolder.pieceData = pieceData;
    }

    private Vector3 GetTilePosition(string notation)
    {
        Debug.Log($"Getting position for tile: {notation}");

        // Find the tile by notation
        for (int x = 0; x < boardSpawner.boardSize; x++)
        {
            for (int y = 0; y < boardSpawner.boardSize; y++)
            {
                string currentNotation = $"{(char)('A' + x)}{y + 1}";
                if (currentNotation == notation)
                {
                    // Calculate world position based on board spawner's tile size and separation
                    Vector3 position = new Vector3(
                        boardSpawner.boardOrigin.x + x * (boardSpawner.tileWidth + boardSpawner.tileSeparation),
                        0,
                        boardSpawner.boardOrigin.z + y * (boardSpawner.tileLength + boardSpawner.tileSeparation)
                    );

                    Debug.Log($"Found position for {notation}: {position}");
                    return position;
                }
            }
        }

        // Fallback if notation not found
        Debug.LogError($"Tile notation {notation} not found!");
        return Vector3.zero;
    }

    private void ClearExistingPieces()
    {
        // Destroy all existing child pieces
        if (piecesParent != null)
        {
            foreach (Transform child in piecesParent)
            {
                Destroy(child.gameObject);
            }
        }
    }

    // Utility component to hold piece data on game objects
    public class PieceDataHolder : MonoBehaviour
    {
        public ChessPieceData pieceData;
    }

    // Optional: Respawn pieces method
    public void RespawnPieces()
    {
        SpawnChessPieces();
    }
} 