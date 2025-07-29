using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.GraphicsBuffer;

public class Piece : MonoBehaviour
{
    Rigidbody rb;
    public bool cek = true;
    public bool stray = true;
    public int id;
    [SerializeField] float movSpeed;
    [SerializeField]Collider[] holder;
    [SerializeField] bool chase;
    [SerializeField] Vector3 movingPosition;
    public MeshRenderer mesh;
    public float up;
    [SerializeField]LayerMask holderLayer;
    RaycastHit hit;
    Vector3 startPosition;
    Vector3 closestHolder;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mesh = GetComponent<MeshRenderer>();
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void FixedUpdate()
    {
        if (chase)
        {
            MoveToward(movingPosition);
        }
    }
    public void Picked(Vector3 target)
    {
        Vector3 hover = new Vector3(target.x, transform.position.y, target.z);
        transform.position = Vector3.MoveTowards(rb.position, hover, movSpeed);
        DetectHolder();
        if(cek)
        {
            StartCoroutine(EarlyHolderCheck());
            
        }
    }
    IEnumerator EarlyHolderCheck()
    {
        if (holder.Length > 0)
        {
            if (holder[0].GetComponent<Holder>().contained)
            {
                holder[0].GetComponent<Holder>().contained = false;
            }
        }
        cek = false;
        yield return new WaitForSeconds(0.01f);
        
    }

    public void DetectHolder()
    {
        holder =  Physics.OverlapBox(transform.position, transform.localScale * 0.5f, Quaternion.identity, holderLayer);
        
    }

    public bool CheckHolder(Collider hldr)
    {
        if(hldr.GetComponent<Holder>().contained == false)
        {
            return true;
        }
        movingPosition = startPosition;
        return false;
    }
    public void BackToStart()
    {
        chase = true;
        stray = true;
        movingPosition = startPosition;
    }

    public void SnapToHolder()
    {
        if (holder.Length == 1)
        {
            if (CheckHolder(holder[0]))
            {
                chase = true;
                stray = false;
                movingPosition = holder[0].transform.position + new Vector3(0, up, 0);
                holder[0].GetComponent<Holder>().contained = true;
                //transform.position = Vector3.MoveTowards(rb.position, holder[0].transform.position + new Vector3(0, up, 0), movSpeed);

            }
            else
            {
                BackToStart();
            }
        }
        else if (holder.Length > 1)
        {
            float closestDistance = Mathf.Infinity;
            
            for (int i=0; i<holder.Length; i++)
            {
                if (CheckHolder(holder[i]))
                {
                    if (closestDistance > Vector3.Distance(holder[i].transform.position, transform.position))
                    {
                        closestDistance = Vector3.Distance(holder[i].transform.position, transform.position);
                        closestHolder = holder[i].transform.position;
                    }
                }
            }
            if (closestHolder != null)
            {
                chase = true;
                stray = false;
                movingPosition = closestHolder + new Vector3(0, up, 0);
                holder[0].GetComponent<Holder>().contained = true;
            }
            else
            {
                BackToStart();
            }
            
            //transform.position = Vector3.MoveTowards(rb.position, closestHolder + new Vector3(0, up, 0), movSpeed);
        }
        else
        {
            BackToStart();
            //transform.position = Vector3.MoveTowards(rb.position, startPosition, movSpeed);
            
        }
    }

    public void MoveToward(Vector3 target)
    {
        if (chase)
        {
            if (transform.position != target)
            {
                transform.position = Vector3.MoveTowards(rb.position, target, movSpeed);
            }
            else
            {
                chase = false;
            }
        }
    }

    public void Highlight()
    {
        (mesh.materials[0], mesh.materials[2]) = (mesh.materials[2], mesh.materials[0]);
    }
}

// Usage
        //swap 2 variable
        //(mesh.materials[1], mesh.materials[0]) = (mesh.materials[0], mesh.materials[1]);