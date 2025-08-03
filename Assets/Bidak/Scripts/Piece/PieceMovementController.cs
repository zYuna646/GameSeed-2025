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

    [Header("Interaction Settings")]
    public float preCapturePauseDistance = 2f; // Increased pause distance
    public float rotationInteractionSpeed = 10f; // Speed of rotation interaction
    public float interactionPauseDuration = 0.5f; // Duration of interaction pause

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

        // Check for player type interaction
        bool shouldInteractBeforeCapture = false;
        PieceController targetPiece = null;

        if (destinationTile != null && 
            pieceController.pieceData != null && 
            destinationTile.currentPieceController != null)
        {
            ChessPieceData destinationPieceData = destinationTile.currentPieceController.pieceData;
            
            // Check if destination tile has a piece with different player type
            if (destinationPieceData.playerType != pieceController.pieceData.playerType)
            {
                shouldInteractBeforeCapture = true;
                targetPiece = destinationTile.currentPieceController;
            }
        }

        // Move towards target position
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            // Check if we're close enough to interact before capture
            if (shouldInteractBeforeCapture && 
                Vector3.Distance(transform.position, targetPosition) <= preCapturePauseDistance)
            {
                // Completely stop movement
                isMoving = false;

                // Stop the animation
                if (animationController != null)
                {
                    animationController.StopMove();
                }

                if (targetPiece != null)
                {
                    // Rotate to face the target piece
                    Vector3 directionToTarget = targetPiece.transform.position - transform.position;
                    directionToTarget.y = 0;
                    Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

                    // Rotate current piece
                    float rotationTime = 0f;
                    Quaternion startRotation = transform.rotation;
                    while (rotationTime < 1f)
                    {
                        transform.rotation = Quaternion.Slerp(startRotation, targetRotation, rotationTime);
                        rotationTime += Time.deltaTime * rotationInteractionSpeed;
                        yield return null;
                    }
                    transform.rotation = targetRotation;

                    // Rotate target piece to face current piece
                    Vector3 directionToCurrentPiece = transform.position - targetPiece.transform.position;
                    directionToCurrentPiece.y = 0;
                    Quaternion targetPieceRotation = Quaternion.LookRotation(directionToCurrentPiece);

                    // Rotate target piece
                    rotationTime = 0f;
                    Quaternion startTargetRotation = targetPiece.transform.rotation;
                    while (rotationTime < 1f)
                    {
                        targetPiece.transform.rotation = Quaternion.Slerp(startTargetRotation, targetPieceRotation, rotationTime);
                        rotationTime += Time.deltaTime * rotationInteractionSpeed;
                        yield return null;
                    }
                    targetPiece.transform.rotation = targetPieceRotation;

                    // Trigger capturing animations for both pieces
                    if (animationController != null)
                    {
                        // Current piece starts capturing
                        animationController.StartCapturing();
                    }

                    // Target piece is being captured
                    PieceAnimationController targetAnimationController = targetPiece.GetComponent<PieceAnimationController>();
                    if (targetAnimationController != null)
                    {
                        targetAnimationController.PlayCaptured();
                    }

                    // Pause for interaction
                    yield return new WaitForSeconds(interactionPauseDuration);
                }

                // Break out of movement loop
                break;
            }

            // Continue moving only if not in interaction range
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // Ensure final position is exactly at target
        transform.position = targetPosition;

        if (destinationTile != null)
        {
            TileController previousTile = currentTile;

            SetCurrentTile(destinationTile);

            if (previousTile != null)
            {
                previousTile.ClearPiece();
            }
            
            // if (destinationTile != null &&
            //     pieceController.pieceData != null &&
            //     destinationTile.CanBeCaptured(pieceController.pieceData))
            // {
                // Capture the destination tile
                destinationTile.CaptureTile(pieceController.pieceData);
            // }


            destinationTile = null;
        }

        // Ensure movement is stopped
        isMoving = false;

        if (animationController != null)
        {
            animationController.StopMove();
        }
    }
}