using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Holder : MonoBehaviour
{
    public bool contained;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (contained)
        {
            ShowEvent();
        }
    }
    public void ShowEvent()
    {

    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Piece") && other.GetComponent<Piece>().stray == false)
        {
            contained = false;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Piece") && other.GetComponent<Piece>().stray == false)
        {
            contained = true;
        }
    }
}
