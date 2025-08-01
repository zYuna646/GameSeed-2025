using UnityEngine;
using System.Collections;

public class PieceMovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;
    public float movementThreshold = 0.01f;

    [Header("Component References")]
    public PieceController pieceController;
    public PieceAnimationController animationController;
    public PieceSoundsController soundsController;

    [Header("Movement State")]
    public bool isMoving = false;
    private Vector3 lastPosition;
    public TileController currentTile;
    public TileController destinationTile;

    private void Awake()
    {
        // Try to get references if not assigned
        if (pieceController == null)
            pieceController = GetComponent<PieceController>();
        if (animationController == null)
            animationController = GetComponent<PieceAnimationController>();
        if (soundsController == null)
            soundsController = GetComponent<PieceSoundsController>();

        // Initialize last position
        lastPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (destinationTile != null && !isMoving)
        {
            SetDestinationTile(destinationTile);
        }
    }

    public void SetCurrentTile(TileController tile)
    {
        currentTile = tile;

        if (pieceController != null && pieceController.pieceData != null)
        {
            pieceController.pieceData.currentTileNotation = tile.tileData?.chessNotation;
        }
    }

    public void SetDestinationTile(TileController tile)
    {
        destinationTile = tile;

        // Immediately move to the destination
        if (destinationTile != null)
        {
            // Use precise spawn position method
            Vector3 destinationPosition = destinationTile.GetPreciseSpawnPosition();
            StartCoroutine(RotateAndMove(destinationPosition));
        }
    }

    private IEnumerator RotateAndMove(Vector3 targetPosition)
    {
        Vector3 directionToDestination = targetPosition - transform.position;
        directionToDestination.y = 0;

        Quaternion targetRotation = Quaternion.LookRotation(directionToDestination);

        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            yield return null;
        }

        transform.rotation = targetRotation;

        MoveTo(targetPosition);
    }

    public void MoveTo(Vector3 targetPosition)
    {
        StartCoroutine(MoveToCoroutine(targetPosition));
    }

    private System.Collections.IEnumerator MoveToCoroutine(Vector3 targetPosition)
    {
        isMoving = true;

        if (animationController != null)
        {
            animationController.StartMove();
        }

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition;

        if (destinationTile != null)
        {
            TileController previousTile = currentTile;

            SetCurrentTile(destinationTile);

            if (previousTile != null)
            {
                previousTile.ClearPiece();
            }
            
            if (destinationTile != null &&
                pieceController.pieceData != null &&
                destinationTile.CanBeCaptured(pieceController.pieceData))
            {
                // Capture the destination tile
                destinationTile.CaptureTile(pieceController.pieceData);
            }

            destinationTile = null;
        }

        isMoving = false;

        if (animationController != null)
        {
            animationController.StopMove();
        }
    }
}