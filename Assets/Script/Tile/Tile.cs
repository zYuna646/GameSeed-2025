using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Color originColor;
    public Color highlightColor;
    public MeshRenderer MeshRenderer;
    public TileManager tileManager;
    private TileController tileController;
    public bool inSight = false;
    public bool hover = false;
    public bool rised = false;
    public bool contained = false;
    public bool canHover = true;
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
        MeshRenderer = GetTileSelectRenderer();
        tileController = GetComponent<TileController>();
        originColor = MeshRenderer.material.color;
        
        normalHeight = transform.position.y;
        normalScale = transform.localScale;
        
        row = tileController.tileData.row + 1;
        collumn = tileController.tileData.column + 1;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!tileManager.isPicked)
        {
            if (inSight)
            {
                CheckInArray();
            }
            if (!hover && rised && tileManager.pieceColliders.Length == 0)
            {
                tileManager.ScaleDown(this.transform, normalHeight, normalScale);
                MeshRenderer.material.color = originColor;
            }
        }else if(tileManager.isPicked)
        {
            if (!tileManager.isPicked)
            {
                tileManager.ScaleDown(this.transform, normalHeight, normalScale);
                MeshRenderer.material.color = originColor;
            }
        }
        if(rised && hover && tileManager.tileColliders.Length == 0 && !tileManager.isPicked)
        {
            tileManager.ScaleDown(this.transform, normalHeight, normalScale);
            MeshRenderer.material.color = originColor;
        }
        
        CheckContain();
    }
    MeshRenderer GetTileSelectRenderer()
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag("TileSelect"))
            {
                MeshRenderer renderer = child.GetComponent<MeshRenderer>();
                if (renderer != null)
                    return renderer;
            }
        }
        return null;
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
                        MeshRenderer.material.color = originColor;
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
                MeshRenderer.material.color = originColor;
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
