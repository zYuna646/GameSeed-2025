using UnityEngine;
using System.Collections;

public class PieceController : MonoBehaviour
{
    [Header("Piece Data")]
    public ChessPieceData pieceData;

    [Header("Current Tile Information")]
    public TileController currentTile;

    [Header("Movement")]
    public bool isMoving = false;
    public float moveSpeed = 5f;

    [Header("State")]
    public bool isSelected = false;
    public bool isCaptured = false;

    [Header("Animation")]
    public PieceAnimationController animationController;

    private void Awake()
    {
        // Try to get PieceAnimationController if not assigned
        if (animationController == null)
            animationController = GetComponent<PieceAnimationController>();
    }

    private void Start()
    {
        // Initial setup if piece data is not set
        if (pieceData == null)
        {
            Debug.LogWarning("No piece data assigned to PieceController");
        }
        else
        {
            // Ensure animation controller has piece data
            if (animationController != null)
            {
                animationController.pieceData = pieceData;
                animationController.pieceController = this;
                
                // Play spawn animation
                animationController.SpawnPiece();
            }
        }
    }

    // Method to set piece data
    public void SetPieceData(ChessPieceData data)
    {
        pieceData = data;
        UpdatePieceAppearance();

        // Update animation controller if exists
        if (animationController != null)
        {
            animationController.pieceData = data;
        }
    }

    // Method to set current tile
    public void SetCurrentTile(TileController tile)
    {
        currentTile = tile;
        
        // Update piece data's current tile notation
        if (pieceData != null)
        {
            pieceData.currentTileNotation = tile.tileData?.chessNotation;
        }
    }

    // Update piece appearance based on piece data
    private void UpdatePieceAppearance()
    {
        if (pieceData == null) return;

        // Update renderer color
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = new Material(renderer.sharedMaterial);
            material.color = pieceData.pieceColor;
            renderer.material = material;
        }
    }

    // Method to move piece
    public void MoveTo(Vector3 targetPosition)
    {
        StartCoroutine(MoveToCoroutine(targetPosition));
    }

    private System.Collections.IEnumerator MoveToCoroutine(Vector3 targetPosition)
    {
        // Set moving state to true
        isMoving = true;

        // Play move animation if animation controller exists
        if (animationController != null)
        {
            // Ensure IsMoving is set to true in the animation controller
            animationController.isMoving = true;
            animationController.MovePiece();
        }

        // Move the piece
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // Reset moving state
        isMoving = false;

        // Stop move animation and return to idle
        if (animationController != null)
        {
            animationController.isMoving = false;
            animationController.StopMove();
        }
    }

    // Method to capture piece
    public void Capture()
    {
        isCaptured = true;
        
        // Play capture/death animation
        if (animationController != null)
        {
            // Use new capture-related methods
            animationController.StartCapturePiece();
            animationController.SetCapturedPiece();
        }

        // Disable piece after animation
        StartCoroutine(DisablePieceAfterDelay());
    }

    private IEnumerator DisablePieceAfterDelay()
    {
        // Wait for animation duration
        yield return new WaitForSeconds(pieceData.deathAnimationSpeed);
        gameObject.SetActive(false);
    }

    // Method to select/deselect piece
    public void ToggleSelection()
    {
        isSelected = !isSelected;
        // Optional: Add visual feedback for selection
        transform.localScale = isSelected ? Vector3.one * 1.2f : Vector3.one;
    }

    // Validate move based on piece type and current board state
    public bool ValidateMove(TileController targetTile)
    {
        if (pieceData == null) return false;

        // Basic movement validation
        switch (pieceData.pieceType)
        {
            case ChessPieceData.PieceType.Pawn:
                return ValidatePawnMove(targetTile);
            case ChessPieceData.PieceType.Rook:
                return ValidateRookMove(targetTile);
            case ChessPieceData.PieceType.Knight:
                return ValidateKnightMove(targetTile);
            case ChessPieceData.PieceType.Bishop:
                return ValidateBishopMove(targetTile);
            case ChessPieceData.PieceType.Queen:
                return ValidateQueenMove(targetTile);
            case ChessPieceData.PieceType.King:
                return ValidateKingMove(targetTile);
            default:
                return false;
        }
    }

    // Specific move validation methods (placeholder implementations)
    private bool ValidatePawnMove(TileController targetTile) { return true; }
    private bool ValidateRookMove(TileController targetTile) { return true; }
    private bool ValidateKnightMove(TileController targetTile) { return true; }
    private bool ValidateBishopMove(TileController targetTile) { return true; }
    private bool ValidateQueenMove(TileController targetTile) { return true; }
    private bool ValidateKingMove(TileController targetTile) { return true; }

    // Optional: Highlight possible moves
    public void HighlightPossibleMoves()
    {
        // Implement move highlighting logic
    }
} 