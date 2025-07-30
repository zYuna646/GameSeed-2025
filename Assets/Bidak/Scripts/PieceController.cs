using UnityEngine;
using System.Collections;

public class PieceController : MonoBehaviour
{
    [Header("Piece Data")]
    public ChessPieceData pieceData;

    [Header("Current Tile Information")]
    public TileController currentTile;
    public TileController destinationTile;

    [Header("Movement")]
    public bool isMoving = false;
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;

    [Header("State")]
    public bool isSelected = false;
    public bool isCaptured = false;

    [Header("Animation")]
    public PieceAnimationController animationController;

    [Header("Piece Body")]
    public GameObject pieceBodyObject;

    private void Awake()
    {
        if (animationController == null)
            animationController = GetComponent<PieceAnimationController>();
    }

    private void Start()
    {
        // Find PieceBody child
        pieceBodyObject = FindPieceBodyChild();

        if (pieceData == null)
        {
            Debug.LogWarning("No piece data assigned to PieceController");
        }
        else
        {
            if (animationController != null)
            {
                animationController.pieceData = pieceData;
                animationController.pieceController = this;

                // Hide piece body before spawn effect
                if (pieceBodyObject != null)
                {
                    pieceBodyObject.SetActive(false);
                }

                animationController.SpawnPiece();
            }
        }
    }

    private GameObject FindPieceBodyChild()
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag("PieceBody"))
            {
                return child.gameObject;
            }
        }
        return null;
    }

    public void ShowPieceBody()
    {
        if (pieceBodyObject != null)
        {
            pieceBodyObject.SetActive(true);
        }
    }

    private void Update()
    {
        if (destinationTile != null && !isMoving)
        {
            SetDestinationTile(destinationTile);
        }
    }

    public void SetPieceData(ChessPieceData data)
    {
        pieceData = data;
        UpdatePieceAppearance();

        if (animationController != null)
        {
            animationController.pieceData = data;
        }
    }

    public void SetCurrentTile(TileController tile)
    {
        currentTile = tile;

        if (pieceData != null)
        {
            pieceData.currentTileNotation = tile.tileData?.chessNotation;
        }
    }

    public void SetDestinationTile(TileController tile)
    {
        destinationTile = tile;

        // Check if destination tile can be captured

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
            animationController.isMoving = true;
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
         pieceData != null &&
         destinationTile.CanBeCaptured(pieceData))
            {
                // Capture the destination tile
                destinationTile.CaptureTile(pieceData);
            }


            destinationTile = null;
        }

        isMoving = false;

        if (animationController != null)
        {
            animationController.isMoving = false;
        }
    }

    public void Capture()
    {
        isCaptured = true;

        if (animationController != null)
        {
            animationController.StartCapturePiece();
            animationController.SetCapturedPiece();
        }

        StartCoroutine(DisablePieceAfterDelay());
    }

    private IEnumerator DisablePieceAfterDelay()
    {
        yield return new WaitForSeconds(pieceData.deathAnimationSpeed);
        gameObject.SetActive(false);
    }

    public void ToggleSelection()
    {
        isSelected = !isSelected;
        transform.localScale = isSelected ? Vector3.one * 1.2f : Vector3.one;
    }

    private void UpdatePieceAppearance()
    {
        if (pieceData == null) return;

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = new Material(renderer.sharedMaterial);
            material.color = pieceData.pieceColor;
            renderer.material = material;
        }
    }
}