using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private TileManager tileManager;
    public bool inSight = false;
    public bool hover = false;
    public bool rised = false;
    public bool contained = false;
    public float normalHeight;
    public Vector3 normalScale;
    [SerializeField] LayerMask test;
    [SerializeField] LayerMask test2;

    [SerializeField] public int row;
    [SerializeField] public int collumn;
    // Start is called before the first frame update
    void Start()
    {
        tileManager = FindObjectOfType<TileManager>();
        normalHeight = transform.position.y;
        normalScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        if (inSight)
        {
            CheckInArray();
        }
        if(!hover &&  rised && tileManager.pieceColliders.Length == 0)
        {
            tileManager.ScaleDown(this.transform, normalHeight, normalScale);
        }
        CheckContain();
    }
    void CheckInArray()
    {
        if (tileManager.tileColliders != null)
        {
            for (int i = 0; i < tileManager.tileColliders.Length; i++)
            {
                if (this.name == tileManager.tileColliders[i].name)
                {
                    hover = true;
                }
                else
                {
                    hover = false;
                    if (rised)
                    {
                        tileManager.ScaleDown(this.transform, normalHeight, normalScale);
                        inSight = false;
                    }
                }
            }
        }
        if (tileManager.tileColliders.Length == 0)
        {
            hover = false;
            if (rised)
            {
                tileManager.ScaleDown(this.transform, normalHeight, normalScale);
                inSight = false;
            }
        }
    }
    void CheckContain()
    {
        if (transform.GetComponentInChildren<PieceType>() != null)
        {
            contained = true;
        }
        else
        {
            contained = false;
        }
    }
}
