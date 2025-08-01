using UnityEngine;

[CreateAssetMenu(fileName = "New Board Tile", menuName = "Chess/Board Tile")]
public class BoardTileData : ScriptableObject
{
    [Header("Tile Identification")]
    public string tileName;
    public string chessNotation; // e.g., "A1", "E4"

    [Header("Tile Properties")]
    public Color tileColor;
    public GameObject tilePrefab;

    [Header("Tile Position")]
    public int row;
    public int column;

    public bool isOccupied => occupyingPiece != null;
    public ChessPieceData occupyingPiece;

    [Header("Positioning")]
    public Vector3 worldPosition;
    public Vector2Int gridPosition; // 2D grid coordinates (0-7 for chess board)

    [Header("Movement Attributes")]
    public bool isValidMoveTarget = false;
    public bool isAttackTarget = false;

    // Method to set tile details
    public void SetTileDetails(string notation, Vector3 position, Vector2Int gridPos, Color tileColor, int row, int column)
    {
        chessNotation = notation;
        worldPosition = position;
        gridPosition = gridPos;
        this.tileColor = tileColor;
        tileName = $"Tile_{notation}";
        this.row = row;
        this.column = column;
    }

    // Method to check and set piece occupation
    public bool TryOccupyTile(ChessPieceData piece)
    {
        // If already occupied, return false
        if (occupyingPiece != null)
        {
            return false;
        }

        occupyingPiece = piece;
        return true;
    }

    // Method to remove piece from tile
    public ChessPieceData RemovePiece()
    {
        if (occupyingPiece == null)
        {
            return null;
        }

        ChessPieceData removedPiece = occupyingPiece;
        occupyingPiece = null;
        return removedPiece;
    }

    // Method to check if a piece can move to this tile
    public bool IsValidMoveTarget(ChessPieceData movingPiece)
    {
        // Basic implementation - can be expanded with more complex rules
        if (occupyingPiece == null)
        {
            return true;
        }

        // If occupied, check if it's an enemy piece that can be captured
        return occupyingPiece.playerType != movingPiece.playerType;
    }

    // Reset tile to initial state
    public void ResetTile()
    {
        occupyingPiece = null;
        isValidMoveTarget = false;
        isAttackTarget = false;
    }

    // Generate chess notation based on grid position
    private string GenerateChessNotation(Vector2Int gridPos)
    {
        char file = (char)('A' + gridPos.x);
        int rank = gridPos.y + 1;
        return $"{file}{rank}";
    }

    // Utility method to get distance between two tiles
    public float GetDistanceTo(BoardTileData otherTile)
    {
        return Vector3.Distance(worldPosition, otherTile.worldPosition);
    }
} 