using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.WSA;

public class TileManager : MonoBehaviour
{
    private GameManager gameManager;
    private ChessBoardTileSpawner chessBoardTileSpawner;
    [SerializeField]private TileController goTo;
    public GameObject kiri;
    public GameObject kanan;
    public Collider[] tileColliders;
    public Collider[] pieceColliders;
    [SerializeField] float cameraOffset;
    [SerializeField] public bool isPicked = false;
    [SerializeField] bool isGoing = false;
    Vector3 cameraPoint;
    Vector3 mousePos;

    [SerializeField] public LayerMask tileMask;
    [SerializeField] public LayerMask pieceMask;
    [SerializeField] float hoverHeight;
    [SerializeField] Vector3 hoverScale;
    Collider piece;

    [SerializeField] public List<GameObject> tiles;

    [SerializeField] Transform test;
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        chessBoardTileSpawner = FindObjectOfType<ChessBoardTileSpawner>();
    }

    // Update is called once per frame
    void Update()
    {
        
        if(tiles.Count != 64)
        {
            tiles = GatherTiles(chessBoardTileSpawner);

            tiles.Sort((a, b) => a.name.CompareTo(b.name));
        }
        mousePos = Input.mousePosition;
        transform.position = Camera.main.ScreenToWorldPoint(mousePos);
        mousePos.z = cameraOffset;

        if (Input.GetKeyDown(KeyCode.S))
        {
            ScaleUp(test);
        }

        if(Input.GetMouseButtonDown(0))
        {
            
            if(!isPicked && pieceColliders.Length != 0)
            {
                piece = pieceColliders[pieceColliders.Length - 1];
                Highlight(piece.GetComponentInParent<Tile>(), piece.GetComponentInParent<Tile>().highlightColor);
                isPicked = true;
            }
            else if(isPicked && tileColliders.Length == 0)
            {
                isPicked = false;
                isGoing = false;
            }
            else if (isPicked && !tileColliders[tileColliders.Length - 1].GetComponent<Tile>().rised)
            {
                isPicked = false;
                isGoing = false;
            }
            else if(isPicked && goTo != null) 
            {
                piece.GetComponent<PieceMovementController>().destinationTile = goTo;
                
                Highlight(piece.GetComponentInParent<Tile>(), piece.GetComponentInParent<Tile>().originColor);
                isPicked = false;
                isGoing = true;
            }

        }
        if (!isGoing)
        {
            Hover();
        }else if (isGoing && piece == null)
        {
            Highlight(goTo.gameObject.GetComponent<Tile>(), goTo.gameObject.GetComponent<Tile>().originColor);
            isGoing = false;
        }
        if(isGoing && Vector3.Distance(piece.transform.position, goTo.transform.position) > 0.01f)
        {
            Highlight(goTo.gameObject.GetComponent<Tile>(), goTo.gameObject.GetComponent<Tile>().highlightColor);
        }
            
        
    }
    void Highlight(Tile a, Color color)
    {
        a.MeshRenderer.material.color = color;
        
    }
    void Hover()
    {
        cameraPoint = Camera.main.ScreenToWorldPoint(mousePos);
        tileColliders = Physics.OverlapCapsule(transform.position, cameraPoint, 0.1f, tileMask);
        pieceColliders = Physics.OverlapCapsule(transform.position, cameraPoint, 0.1f, pieceMask);
        if (!isPicked)
        {
            if (tileColliders.Length > 0 && pieceColliders.Length == 0)
            {
                Collider tile = tileColliders[tileColliders.Length - 1];
                tile.GetComponent<Tile>().inSight = true;
                if (!tile.GetComponent<Tile>().contained)
                {
                    if (tile.GetComponent<Tile>().hover && !tile.GetComponent<Tile>().rised && tile.GetComponent<Tile>().canHover)
                    {
                        ScaleUp(tile.transform);
                    }
                }
            }
            if (pieceColliders.Length > 0)
            {
                GameObject piece = pieceColliders[pieceColliders.Length - 1].gameObject;

                PieceHover(piece.GetComponent<PieceType>().parent.transform,piece.GetComponent<PieceType>());
            
            }
        }else if (tileColliders.Length != 0 && tileColliders[tileColliders.Length-1].GetComponent<Tile>().rised && isPicked)
        {
            goTo = tileColliders[tileColliders.Length - 1].GetComponent<TileController>();
        }
            
    }
    public void ScaleUp(Transform tile)
    {
        Vector3 rise = new Vector3(tile.position.x, hoverHeight, tile.position.z);
        tile.position = Vector3.MoveTowards(tile.position, rise, 100f * Time.deltaTime);
        //tile.localScale = Vector3.Lerp(tile.localScale, hoverScale, 200f * Time.deltaTime);
        tile.GetComponent<Tile>().rised = true;
        Debug.Log("Scale UP "+ tile.gameObject);
    }

    public void ScaleDown(Transform tile, float normalHeight, Vector3 normalScale)
    {
        Vector3 down = new Vector3(tile.position.x, normalHeight, tile.position.z);
        tile.position = Vector3.MoveTowards(tile.position, down, 100f * Time.deltaTime);
        tile.localScale = Vector3.Lerp(tile.localScale, normalScale, 200f * Time.deltaTime);
        tile.GetComponent<Tile>().rised = false;
        Debug.Log("Scale Down");    
    }

    //void PieceHover(PieceType piece)
    //{
    //    piece.RayCheck();
    //}
    void PieceHover(Transform tile, PieceType piece)
    {
        if (tile == null || piece == null || tiles == null) return;

        Tile currentTile = tile.GetComponent<Tile>();
        int currentRow = currentTile.row;
        int currentCol = currentTile.collumn;
        bool isWhite = piece.isWhite;

        for (int i = 0; i < tiles.Count; i++)
        {
            Tile targetTile = tiles[i].GetComponent<Tile>();
            if (targetTile == null || !targetTile.canHover) continue;

            int rowDiff = targetTile.row - currentRow;
            int colDiff = targetTile.collumn - currentCol;
            int absRowDiff = Mathf.Abs(rowDiff);
            int absColDiff = Mathf.Abs(colDiff);

            bool isValidMove = false;
            bool isBlocked = false;

            switch (piece.typePiece)
            {
                case type.ROOK:
                    isValidMove = (targetTile.row == currentRow) ^ (targetTile.collumn == currentCol);
                    if (isValidMove)
                        isBlocked = IsPathBlocked(currentTile, targetTile, false);
                    break;

                case type.PAWN:
                    int forwardDir = isWhite ? 1 : -1;
                    isValidMove = (rowDiff == forwardDir && colDiff == 0 && !targetTile.contained) ||
                                 (!piece.hasDoubleMoved && rowDiff == 2 * forwardDir && colDiff == 0 && !targetTile.contained) ||
                                 (absColDiff == 1 && rowDiff == forwardDir && targetTile.contained &&
                                  targetTile.GetComponentInChildren<PieceType>().isWhite != isWhite);
                    break;

                case type.BISHOP:
                    isValidMove = (absRowDiff == absColDiff && absRowDiff > 0);
                    if (isValidMove)
                        isBlocked = IsPathBlocked(currentTile, targetTile, true);
                    break;

                case type.KNIGHT:
                    isValidMove = (absRowDiff == 2 && absColDiff == 1) || (absRowDiff == 1 && absColDiff == 2);
                    break;

                case type.KING:
                    isValidMove = (absRowDiff <= 1 && absColDiff <= 1 && (absRowDiff + absColDiff) > 0 && !targetTile.contained);
                    // Castling logic remains unchanged
                    break;

                case type.QUEEN:
                    bool isStraight = (targetTile.row == currentRow) ^ (targetTile.collumn == currentCol);
                    bool isDiagonal = absRowDiff == absColDiff;
                    isValidMove = isStraight || isDiagonal;
                    if (isValidMove)
                        isBlocked = IsPathBlocked(currentTile, targetTile, isDiagonal);
                    break;
            }

            if (isValidMove && !isBlocked)
            {
                ScaleUp(tiles[i].transform);
            }
        }
    }

    bool IsPathBlocked(Tile start, Tile end, bool isDiagonal)
    {
        int xDir = Mathf.Clamp(end.collumn - start.collumn, -1, 1);
        int yDir = Mathf.Clamp(end.row - start.row, -1, 1);

        int steps = isDiagonal ?
            Mathf.Abs(end.collumn - start.collumn) :
            Mathf.Max(Mathf.Abs(end.collumn - start.collumn), Mathf.Abs(end.row - start.row));

        // Check each tile along the path (excluding start and end)
        for (int i = 1; i < steps; i++)
        {
            int checkX = start.collumn + xDir * i;
            int checkY = start.row + yDir * i;

            Tile intermediateTile = tiles.Find(t =>
                t.GetComponent<Tile>().collumn == checkX &&
                t.GetComponent<Tile>().row == checkY)?.GetComponent<Tile>();

            if (intermediateTile != null && intermediateTile.contained)
                return true;
        }

        return false;
    }


    public List<GameObject> GatherTiles(ChessBoardTileSpawner gm)
    {
        // Create empty list if null
        if (gm == null || gm.transform == null) return new List<GameObject>();

        // Proper way to get all child GameObjects
        List<GameObject> childObjects = new List<GameObject>();

        foreach (Transform child in gm.transform)
        {
            childObjects.Add(child.gameObject);
        }

        return childObjects;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, cameraPoint);
    }
}
