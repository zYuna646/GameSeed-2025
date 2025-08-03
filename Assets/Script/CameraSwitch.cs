using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    public GameObject[] mainCameras;
    public GameObject[] cardCameras;
    public int player;
    public Camera camera1;
    public Camera camera2;
    public Camera currentCamera;
    // Start is called before the first frame update
    void Start()
    {
        mainCameras = GameObject.FindGameObjectsWithTag("MainCamera");
        cardCameras = new GameObject[2];
        cardCameras[0] = GameObject.FindGameObjectWithTag("Player1Camera");
        cardCameras[1] = GameObject.FindGameObjectWithTag("Player2Camera");
        if(player == 0)
        {
            camera1 = mainCameras[1].GetComponent<Camera>();
            camera2 = cardCameras[0].GetComponent<Camera>();
            mainCameras[0].GetComponent<Camera>().enabled = false;
            cardCameras[1].GetComponent <Camera>().enabled = false;
        }
        else
        {
            camera1 = mainCameras[0].GetComponent<Camera>();
            camera2 = cardCameras[1].GetComponent<Camera>();
            mainCameras[1].GetComponent<Camera>().enabled = false;
            cardCameras[0].GetComponent<Camera>().enabled = false;
        }

        camera1.enabled = true;
        currentCamera = camera1;
        camera2.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButton(0))
        {
            camera1.enabled = true;
            camera2.enabled = false;
            currentCamera = camera1;
        }
        if(Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButton(1))
        {
            camera1.enabled = false;
            camera2.enabled = true;
            currentCamera = camera2;
        }
    }
}
