using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.WSA;

public class TileManager : MonoBehaviour
{
    public Collider[] tileColliders;
    public Collider[] pieceColliders;
    [SerializeField] float cameraOffset;
    Vector3 cameraPoint;
    Vector3 mousePos;

    [SerializeField] public LayerMask tileMask;
    [SerializeField] public LayerMask pieceMask;
    [SerializeField] float hoverHeight;
    [SerializeField] Vector3 hoverScale;

    [SerializeField] public List<GameObject> tiles;

    [SerializeField] Transform test;
    void Start()
    {
        tiles = GatherTiles(tileMask);

        tiles.Sort((a, b) => a.name.CompareTo(b.name));

    }

    // Update is called once per frame
    void Update()
    {
        mousePos = Input.mousePosition;
        transform.position = Camera.main.ScreenToWorldPoint(mousePos);
        mousePos.z = cameraOffset;

        if (Input.GetKeyDown(KeyCode.S))
        {
            ScaleUp(test);
        }

        Hover();
    }
    void Hover()
    {
        cameraPoint = Camera.main.ScreenToWorldPoint(mousePos);
        tileColliders = Physics.OverlapCapsule(transform.position, cameraPoint, 0.1f, tileMask);
        pieceColliders = Physics.OverlapCapsule(transform.position, cameraPoint, 0.1f, pieceMask);
        if (tileColliders.Length > 0 && pieceColliders.Length == 0)
        {
            Collider tile = tileColliders[tileColliders.Length - 1];
            tile.GetComponent<Tile>().inSight = true;
            if (!tile.GetComponent<Tile>().contained)
            {
                if (tile.GetComponent<Tile>().hover && !tile.GetComponent<Tile>().rised)
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
    }
    public void ScaleUp(Transform tile)
    {
        Vector3 rise = new Vector3(tile.position.x, hoverHeight, tile.position.z);
        tile.position = Vector3.MoveTowards(tile.position, rise, 100f * Time.deltaTime);
        tile.localScale = Vector3.Lerp(tile.localScale, hoverScale, 200f * Time.deltaTime);
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
        int currentRow = tile.GetComponent<Tile>().row;
        int currentCol = tile.GetComponent<Tile>().collumn;
        bool isWhite = piece.isWhite; // Assuming you have this field

        for (int i = 0; i < tiles.Count; i++)
        {
            Tile targetTile = tiles[i].GetComponent<Tile>();
            int rowDiff = targetTile.row - currentRow;
            int colDiff = targetTile.collumn - currentCol;
            int absRowDiff = Mathf.Abs(rowDiff);
            int absColDiff = Mathf.Abs(colDiff);

            switch (piece.typePiece)
            {
                case type.ROOK:
                    // Rook moves (straight lines)
                    if ((targetTile.row == currentRow) ^ (targetTile.collumn == currentCol))
                    {
                        ScaleUp(tiles[i].transform);
                    }
                    break;

                case type.PAWN:
                    // Pawn moves (forward only)
                    int forwardDir = isWhite ? 1 : -1;
                    bool isForwardOne = rowDiff == forwardDir && colDiff == 0;
                    bool isForwardTwo = !piece.hasDoubleMoved && rowDiff == 2 * forwardDir && colDiff == 0;

                    if (isForwardOne || isForwardTwo)
                    {
                        ScaleUp(tiles[i].transform);
                    }
                    break;

                case type.BISHOP:
                    // Bishop moves (diagonals)
                    if (absRowDiff == absColDiff && absRowDiff > 0)
                    {
                        ScaleUp(tiles[i].transform);
                    }
                    break;

                case type.KNIGHT:
                    // Knight moves (L-shape)
                    if ((absRowDiff == 2 && absColDiff == 1) || (absRowDiff == 1 && absColDiff == 2))
                    {
                        ScaleUp(tiles[i].transform);
                    }
                    break;

                case type.KING:
                    // King moves (1 square any direction)
                    if (absRowDiff <= 1 && absColDiff <= 1 && (absRowDiff != 0 || absColDiff != 0))
                    {
                        ScaleUp(tiles[i].transform);
                    }
                    break;

                case type.QUEEN:
                    // Queen moves (rook + bishop)
                    bool isStraight = (targetTile.row == currentRow) ^ (targetTile.collumn == currentCol);
                    bool isDiagonal = absRowDiff == absColDiff;

                    if (isStraight || isDiagonal && ((targetTile.row != currentRow) && (targetTile.collumn != currentCol)))
                    {
                        ScaleUp(tiles[i].transform);
                    }
                    break;
            }
        }
    }
    //for (int i = 0; i < tiles.Count; i++)
    //{
    //    //rook
    //    if ((tiles[i].GetComponent<Tile>().row == tile.GetComponent<Tile>().row) ^ (tiles[i].GetComponent<Tile>().collumn == tile.GetComponent<Tile>().collumn))
    //    {
    //        ScaleUp(tiles[i].transform);
    //    }

    //    //pawn
    //    if (tiles[i].GetComponent<Tile>().row == tile.GetComponent<Tile>().row + 1)
    //    {
    //        ScaleUp(tiles[i].transform);
    //    }
    //    else if (!piece.hasDoubleMoved && tiles[i].GetComponent<Tile>().row == tile.GetComponent<Tile>().row + 2)
    //    {
    //        ScaleUp(tiles[i].transform);
    //    }

    //}


    public List<GameObject> GatherTiles(LayerMask layerMask)
    {
        Collider[] colliders = Physics.OverlapSphere(Vector3.zero, float.MaxValue, layerMask);
        return colliders.Select(c => c.gameObject).ToList();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, cameraPoint);
    }
}
