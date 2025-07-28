using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hands_Point : MonoBehaviour
{
    public Vector3 pos;
    public Vector3 tar;
    [SerializeField] private float offset;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        pos = Input.mousePosition;
        tar = Input.mousePosition;
        pos.z = offset;
        
        transform.position = Camera.main.ScreenToWorldPoint(pos);
    }
}
