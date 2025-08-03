using System;
using System.Collections.Generic;
using UnityEngine;

public class ChessBoardManager : MonoBehaviour
{
    [Header("Board Configuration")]
    public int boardSize = 8;
    public float tileSizeInUnits = 1f;

    [Header("Tile Data")]
    public BoardTileData defaultTileTemplate;

    [Header("Board State")]
    public BoardTileData[,] boardTiles;
    public Transform tilesParent;

    [Header("Piece Configurations")]
    public List<ChessPieceData> availablePieces = new List<ChessPieceData>();
    public List<ChessPieceData> defaultWhitePieces = new List<ChessPieceData>();
    public List<ChessPieceData> defaultBlackPieces = new List<ChessPieceData>();
    public List<ChessPieceData> activePieces = new List<ChessPieceData>();

    [Serializable]
    public class PiecePosition
    {
        public ChessPieceData piece;
        public string chessNotation;
        public Color pieceColor;
    }

    [Header("Custom Board Setup")]
    public List<PiecePosition> customBoardSetup = new List<PiecePosition>();

    private void Awake()
    {
        // Ensure tiles parent exists
        if (tilesParent == null)
        {
            tilesParent = new GameObject("BoardTiles").transform;
            tilesParent.SetParent(transform);
            tilesParent.localPosition = Vector3.zero;
        }
    }

    public void InitializeBoard()
    {
        // Clear existing board
        ClearExistingBoard();

        // Initialize board tiles array
        boardTiles = new BoardTileData[boardSize, boardSize];

        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                // Calculate world position
                Vector3 worldPosition = new Vector3(
                    x * tileSizeInUnits, 
                    0, 
                    y * tileSizeInUnits
                );

                // Create tile data
                BoardTileData tileData = ScriptableObject.CreateInstance<BoardTileData>();
                
                // Set tile properties
                tileData.tilePrefab = defaultTileTemplate.tilePrefab;
                tileData.tileColor = (x + y) % 2 == 0 ? Color.white : Color.black;

                // Set tile details
                string notation = $"{(char)('A' + x)}{y + 1}";
                BoardTileData spawnedTileData = ScriptableObject.CreateInstance<BoardTileData>();
                spawnedTileData.SetTileDetails(
                    notation, 
                    worldPosition, 
                    new Vector2Int(x, y), 
                    tileData.tileColor,
                    y,  // row
                    x   // column
                );

                // Spawn tile game object
                GameObject tileObject = Instantiate(
                    spawnedTileData.tilePrefab, 
                    worldPosition, 
                    Quaternion.identity, 
                    tilesParent
                );

                // Modify tile appearance
                Renderer tileRenderer = tileObject.GetComponent<Renderer>();
                if (tileRenderer != null)
                {
                    tileRenderer.material.color = spawnedTileData.tileColor;
                }

                // Add tile data component
                TileDataHolder dataHolder = tileObject.AddComponent<TileDataHolder>();
                dataHolder.tileData = spawnedTileData;

                // Store in board array
                boardTiles[x, y] = spawnedTileData;
            }
        }
    }

    private void ClearExistingBoard()
    {
        // Destroy existing tile objects
        if (tilesParent != null)
        {
            foreach (Transform child in tilesParent)
            {
                Destroy(child.gameObject);
            }
        }

        // Destroy tile data ScriptableObjects
        if (boardTiles != null)
        {
            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    if (boardTiles[x, y] != null)
                    {
                        Destroy(boardTiles[x, y]);
                    }
                }
            }
        }
    }

    // Utility component to hold tile data on game objects
    public class TileDataHolder : MonoBehaviour
    {
        public BoardTileData tileData;
    }

    // Get tile by chess notation
    public BoardTileData GetTileByNotation(string notation)
    {
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                if (boardTiles[x, y].chessNotation == notation)
                {
                    return boardTiles[x, y];
                }
            }
        }
        return null;
    }

    // Get tile by world position
    public BoardTileData GetTileByWorldPosition(Vector3 worldPos)
    {
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                if (Vector3.Distance(boardTiles[x, y].worldPosition, worldPos) < 0.1f)
                {
                    return boardTiles[x, y];
                }
            }
        }
        return null;
    }

    // Create default chess piece configurations
    public void CreateDefaultPieces()
    {
        // Clear existing pieces
        availablePieces.Clear();
        defaultWhitePieces.Clear();
        defaultBlackPieces.Clear();

        // Create piece types
        ChessPieceData[] pieceTypes = new ChessPieceData[]
        {
            CreatePiece(ChessPieceData.PieceType.Rook),
            CreatePiece(ChessPieceData.PieceType.Knight),
            CreatePiece(ChessPieceData.PieceType.Bishop),
            CreatePiece(ChessPieceData.PieceType.Queen),
            CreatePiece(ChessPieceData.PieceType.King),
            CreatePiece(ChessPieceData.PieceType.Bishop),
            CreatePiece(ChessPieceData.PieceType.Knight),
            CreatePiece(ChessPieceData.PieceType.Rook)
        };

        // Create white pieces
        for (int i = 0; i < pieceTypes.Length; i++)
        {
            var piece = Instantiate(pieceTypes[i]);
            piece.startPosition = boardTiles[i, 1].worldPosition;
            defaultWhitePieces.Add(piece);
        }

        // Create white pawns
        for (int i = 0; i < boardSize; i++)
        {
            var pawn = CreatePiece(ChessPieceData.PieceType.Pawn);
            pawn.startPosition = boardTiles[i, 1].worldPosition;
            defaultWhitePieces.Add(pawn);
        }

        // Create black pieces (mirror white pieces)
        for (int i = 0; i < pieceTypes.Length; i++)
        {
            var piece = Instantiate(pieceTypes[i]);
            piece.startPosition = boardTiles[i, 6].worldPosition;
            defaultBlackPieces.Add(piece);
        }

        // Create black pawns
        for (int i = 0; i < boardSize; i++)
        {
            var pawn = CreatePiece(ChessPieceData.PieceType.Pawn);
            pawn.startPosition = boardTiles[i, 6].worldPosition;
            defaultBlackPieces.Add(pawn);
        }

        // Add to available pieces
        availablePieces.AddRange(defaultWhitePieces);
        availablePieces.AddRange(defaultBlackPieces);
    }

    // Helper method to create a piece
    private ChessPieceData CreatePiece(ChessPieceData.PieceType type)
    {
        ChessPieceData piece = ScriptableObject.CreateInstance<ChessPieceData>();
        piece.pieceType = type;
        piece.SetDefaultMovement();
        return piece;
    }

    // Setup default chess board
    public void SetupDefaultPieces()
    {
        CreateDefaultPieces();

        // If custom setup is empty, use default
        if (customBoardSetup.Count == 0)
        {
            foreach (var piece in defaultWhitePieces)
            {
                PlacePieceOnBoard(piece, piece.startPosition);
            }
            foreach (var piece in defaultBlackPieces)
            {
                PlacePieceOnBoard(piece, piece.startPosition);
            }
        }
        else
        {
            // Use custom board setup
            foreach (var piecePos in customBoardSetup)
            {
                BoardTileData targetTile = GetTileByNotation(piecePos.chessNotation);
                if (targetTile != null)
                {
                    PlacePieceOnBoard(piecePos.piece, targetTile.worldPosition);
                }
            }
        }
    }

    // Place a piece on a specific board tile
    public bool PlacePieceOnBoard(ChessPieceData piece, Vector3 position)
    {
        BoardTileData targetTile = GetTileByWorldPosition(position);
        
        if (targetTile != null && !targetTile.isOccupied)
        {
            targetTile.TryOccupyTile(piece);
            piece.currentPosition = position;
            activePieces.Add(piece);
            return true;
        }
        return false;
    }

    // Method to add a custom piece to the board
    public void AddCustomPiece(ChessPieceData piece, string chessNotation, Color pieceColor)
    {
        BoardTileData targetTile = GetTileByNotation(chessNotation);
        
        if (targetTile != null)
        {
            PlacePieceOnBoard(piece, targetTile.worldPosition);
        }
    }

    // Reset the entire board
    public void ResetBoard()
    {
        // Clear active pieces
        activePieces.Clear();

        // Reset all tiles
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                boardTiles[x, y].ResetTile();
            }
        }

        // Reinitialize pieces
        SetupDefaultPieces();
    }
} 