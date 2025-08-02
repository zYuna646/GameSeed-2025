using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum type
{
    PAWN, ROOK, KNIGHT, BISHOP, QUEEN, KING
}
public class PieceType : MonoBehaviour
{
    [SerializeField] public float spaceColission;
    [SerializeField] public float spaceRow;
    public Piece piece;
    private Vector3 moveRay;
    [SerializeField] public LayerMask tileMask;
    private TileManager tileManager;

    [SerializeField] public Collider[] ray1;
    [SerializeField] public Collider[] ray2;
    [SerializeField] public Collider[] ray3;
    [SerializeField] public Collider[] ray4;

    //pawn indicator
    public bool hasDoubleMoved = false;
    public bool isWhite;
    
    //rook indicator
    bool hasMove = false;

    //king indicator
    bool hasCastled = false;


    [SerializeField] public type typePiece;
    [SerializeField] public GameObject parent;

    [SerializeField] Vector3 a = new Vector3(0, 0, 0);
    [SerializeField] Vector3 b = new Vector3(0, 0, 0);
    [SerializeField] Vector3 c = new Vector3(0, 0, 0);
    [SerializeField] Vector3 origin;

    // Start is called before the first frame update
    private void Awake()
    {
        this.transform.SetParent(parent.transform);
        this.transform.position = parent.transform.position + new Vector3(0, 1.33f,0);
        piece = GetComponent<Piece>();
        tileManager = FindObjectOfType<TileManager>();
        
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        origin = transform.position - new Vector3(0,0.62f, 0);
        //    switch (typePiece)
        //    {
        //        case type.PAWN:

        //            break;
        //        case type.ROOK:
        //            break;
        //            moveRay = new Vector3(spaceColission *8, 0,spaceColission * 8);
        //        case type.KNIGHT:
        //            break;
        //        case type.BISHOP:
        //            break;
        //        case type.QUEEN:
        //            break;
        //        case type.KING:
        //            break;
        //    }
    }
    public void RayCheck()
    {
        Debug.Log("RayCheck");
        
        int rayNum = 0;
        

        if(typePiece == type.PAWN)
        {
            Debug.Log("PAWN");
            rayNum = 3;
            
            PawnMove();
            ray1 = Physics.OverlapCapsule(origin, origin + a, 0.1f, tileMask);
            ray2 = Physics.OverlapCapsule(origin, origin + b, 0.1f, tileMask);
            ray3 = Physics.OverlapCapsule(origin, origin + c, 0.1f, tileMask);
            

            for(int i = 0; i<ray1.Length; i++)
            {
                if (ray1[i].gameObject != this.parent)
                {
                    tileManager.ScaleUp(ray1[i].transform);
                }
                
            }
        }

        //pawn
        /*
         ray1 = Physics.OverlapCapsule(parent.transform.position, parent.transform.position + spaceColision + spaceRow, 0.01f);
         */
    }
    void PawnMove()
    {
        if (!isWhite)
        {
            if (!hasDoubleMoved)
            {
                b = new Vector3 (0,0, spaceColission * 2);
            }
            else
            {
                b = new Vector3(0, 0,spaceColission);
            }
            a = new Vector3(spaceRow, 0,spaceColission);
            c = new Vector3(-spaceRow, 0, spaceColission);
        }
        else
        {
            if (!hasDoubleMoved)
            {
                b = new Vector3(0, 0, -spaceColission * 2);
            }
            else
            {
                b = new Vector3(0, -spaceColission, 0);
            }
            a = new Vector3(-spaceRow, 0, -spaceColission);
            c = new Vector3(spaceRow, 0, -spaceColission);
        }
    }

    void RookMove()
    {

    }

    void KnightMove()
    {

    }

    void BishopMove()
    {

    }

    void QueenMove()
    {

    }

    void KingMove()
    {

    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(origin, origin + a);
    }
}
