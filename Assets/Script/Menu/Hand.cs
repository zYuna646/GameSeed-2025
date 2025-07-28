using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

enum state
{
    FIRST, PICKING
}
public class Hand : MonoBehaviour
{
    
    Animator animator;
    [SerializeField] Transform point;
    [SerializeField] Ray_cam camPoint;
    [SerializeField] Vector3 hold;
    [SerializeField] Vector3 targetPoint;
    [SerializeField] Vector3 change;
    [SerializeField] float radius;
    [SerializeField] float range;
    [SerializeField] LayerMask PieceMask;
    [SerializeField] Vector3 maxRay;
    [SerializeField] bool released;
    [SerializeField] float3 castLength;
     Vector3 holder;
    [SerializeField] Vector3 pointer;
    public bool picking = false;

    [SerializeField]float shortest = Mathf.Infinity;
    [SerializeField]Piece closestGO = null;
    [SerializeField] state currentState = state.FIRST;
    [SerializeField] Collider[] Objects;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        
    }

    // Update is called once per frame
    void Update()
    {
        targetPoint = point.transform.position;
        holder = point.transform.position + pointer;

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            animator.SetBool("isHolding", true);
        }
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            animator.SetBool("isHolding", false);
            Release();
            
        }

        
    }
    private void FixedUpdate()
    {
        if(animator.GetBool("isHolding") == true)
        {
            PickUp();
        }
    }

    private void PickUp()
    {
        //Rute 2
        /*Objects = Physics.OverlapCapsule(point.transform.position, point.transform.position + castLength, radius, PieceMask);*/ // rute sphere cast
        Objects = Physics.OverlapCapsule(camPoint.top, camPoint.bottom + castLength, radius, PieceMask);


        if (currentState == state.FIRST)
        {
            for (int i = 0; i < Objects.Length; i++)
                {
                Objects[i].GetComponent<Piece>().Highlight();
                    if (shortest >= (Vector2.Distance(Objects[i].transform.position, point.transform.position)))
                    {
                        shortest = Vector2.Distance(Objects[i].transform.position, point.transform.position);
                        closestGO = Objects[i].GetComponent<Piece>();
                    }
                }
            currentState = state.PICKING;
        }
        if (currentState == state.PICKING)
        {
            Objects = null;
            if(closestGO != null)
            {
                closestGO.Picked(holder);
            }
            
        }
        
        
    }
    private void Release()
    {
        currentState = state.FIRST;
        if(closestGO!= null)
        {
            Debug.Log("LEPAS");
            closestGO.cek = true;
            closestGO.SnapToHolder();
            closestGO = null;
        }
        shortest = Mathf.Infinity;
        
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(point.transform.position, radius);
        Gizmos.DrawLine(camPoint.top, camPoint.bottom + castLength);
    }
}

//Dibuang sayang
//Rute 1
//RaycastHit hitInfo /*= Physics.SphereCast(point.transform.position, radius, Vector3.down, out hitInfo, range, PieceMask)*/;

//Physics.SphereCast(point.transform.position, radius, Vector3.down, out hitInfo, PieceMask);
//if (hitInfo.rigidbody != null)
//{
//    Piece control = GetComponent<Piece>();
//    control.Picked(point.transform);
//}
//Debug.DrawRay(point.transform.position, maxRay);
//Physics.Raycast(point.transform.position, maxRay, PieceMask);//Rute Raycast
